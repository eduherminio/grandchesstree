using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using GrandChessTree.Api.ApiKeys;
using GrandChessTree.Api.D10Search;
using GrandChessTree.Api.Database;
using GrandChessTree.Shared.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrandChessTree.Api.Controllers
{
    [ApiController]
    [Route("api/v1/perft")]
    public class PerftController : ControllerBase
    {     
        private readonly ILogger<PerftController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly TimeProvider _timeProvider;
        private readonly ApiKeyAuthenticator _apiKeyAuthenticator;
        public PerftController(ILogger<PerftController> logger, ApplicationDbContext dbContext, TimeProvider timeProvider, ApiKeyAuthenticator apiKeyAuthenticator)
        {
            _logger = logger;
            _dbContext = dbContext;
            _timeProvider = timeProvider;
            _apiKeyAuthenticator = apiKeyAuthenticator;
        }

        [ApiKeyAuthorize]
        [HttpPost("{depth}/tasks")]
        public async Task<IActionResult> CreateNewTaskBatch(int depth, CancellationToken cancellationToken)
        {
            var apiKey = await _apiKeyAuthenticator.GetApiKey(HttpContext, cancellationToken);

            if(apiKey == null)
            {
                return Unauthorized();
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var currentTimestamp = _timeProvider.GetUtcNow().ToUnixTimeSeconds();

            var searchItems = await _dbContext.PerftItems
              .FromSqlRaw(@"
                    SELECT * FROM public.perft_items 
                    WHERE NOT confirmed AND available_at = 0 AND pass_count = 0 AND depth = {0}
                    ORDER BY available_at, pass_count 
                    LIMIT 20 FOR UPDATE SKIP LOCKED", depth)
              .ToListAsync(cancellationToken);

            if (!searchItems.Any())
            {
                return NotFound();
            }

            // Prepare search tasks
            var searchTasks = searchItems.Select(perftItem => new PerftTask
            {
                PerftItem = perftItem,
                PerftItemId = perftItem.Id,
                StartedAt = currentTimestamp,
                Depth = perftItem.Depth,
                AccountId = apiKey.AccountId
            }).ToList();

            await _dbContext.PerftTasks.AddRangeAsync(searchTasks, cancellationToken);

            // Update search items to prevent immediate reprocessing
            foreach (var item in searchItems)
            {
                item.AvailableAt = currentTimestamp + 3600; // Becomes available again in 1 hour
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Commit transaction to finalize changes
            await transaction.CommitAsync(cancellationToken);

            // Prepare response
            var response = searchTasks.Select(task => new PerftTaskResponse
            {
                PerftTaskId = task.Id,
                PerftItemHash = task.PerftItem.Hash,
                Depth = task.Depth,
            }).ToArray();

            return Ok(response);
        }

        public class ProgressStatsModel
        {
            [Column("completed_tasks")]
            [JsonPropertyName("completed_tasks")]
            public ulong completed_tasks { get; set; }
            [Column("total_nodes")]
            [JsonPropertyName("total_nodes")]
            public ulong total_nodes { get; set; }
        }

        public class RealTimeStatsModel
        {
            [Column("tpm")]
            [JsonPropertyName("tpm")]
            public float tpm { get; set; }
            [Column("nps")]
            [JsonPropertyName("nps")]
            public float nps { get; set; }
        }


        [HttpGet("{depth}/stats")]
        [ResponseCache(Duration = 10, VaryByQueryKeys = new[] { "depth" })]
        public async Task<IActionResult> GetStats(int depth, CancellationToken cancellationToken)
        {
            var currentTimestamp = _timeProvider.GetUtcNow().ToUnixTimeSeconds();
            var pastMinuteTimestamp = currentTimestamp - 60;

            var realTimeStatsResult = await _dbContext.Database
                .SqlQueryRaw<RealTimeStatsModel>(@"
        SELECT
        COALESCE(COUNT(*), 0) / 60.0 AS tpm,
        COALESCE(SUM(t.nodes * i.occurrences), 0) / 3600.0 AS nps
        FROM public.perft_tasks t
        JOIN public.perft_items i ON t.perft_item_id = i.id
        WHERE t.depth = {0} 
        AND t.finished_at >= EXTRACT(EPOCH FROM NOW()) - 3600", depth)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);


            var progressStatsResult = await _dbContext.Database
           .SqlQueryRaw<ProgressStatsModel>(@"
        SELECT
        COUNT(*) AS completed_tasks,
        COALESCE(SUM(t.nodes * i.occurrences), 0) AS total_nodes
        FROM public.perft_tasks t
        JOIN public.perft_items i ON t.perft_item_id = i.id
        WHERE t.depth = {0} AND t.finished_at > 0", depth)
           .AsNoTracking()
           .FirstOrDefaultAsync(cancellationToken);


            var response = new PerftStatsResponse()
            {
                Nps = realTimeStatsResult?.nps ?? 0,
                Tpm = realTimeStatsResult?.tpm ?? 0,
                CompletedTasks = progressStatsResult?.completed_tasks ?? 0ul,
                TotalNodes = progressStatsResult?.total_nodes ?? 0ul,
                PercentCompletedTasks = (float)(progressStatsResult?.completed_tasks ?? 0ul) / 101240 * 100
            };
            return Ok(response);
        }

        public class PerformanceChartEntry
        {
            [Column("timestamp")]
            [JsonPropertyName("timestamp")]
            public long timestamp { get; set; }
            [Column("tpm")]
            [JsonPropertyName("tpm")]
            public float tpm { get; set; }
            [Column("nps")]
            [JsonPropertyName("nps")]
            public float nps { get; set; }
        }


        [HttpGet("{depth}/stats/charts/performance")]
        [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "depth" })]
        public async Task<IActionResult> GetPerformanceChart(int depth, CancellationToken cancellationToken)
        {
            var result = await _dbContext.Database
                .SqlQueryRaw<PerformanceChartEntry>(@"
                    WITH time_buckets AS (
                        SELECT generate_series(
                            EXTRACT(EPOCH FROM NOW()) - 43200,  -- 3 hours ago
                            EXTRACT(EPOCH FROM NOW()),         -- Now
                            900                                -- 15-minute intervals (900 seconds)
                        ) AS bucket_start
                    )
                    SELECT 
                        tb.bucket_start AS timestamp,
                        COUNT(t.id) / 15.0 AS tpm,  -- Tasks per minute (since interval is 15 min)
                        COALESCE(SUM(t.nodes * i.occurrences) / (15 * 60), 0) AS nps  -- Nodes per second
                    FROM time_buckets tb
                    LEFT JOIN public.perft_tasks t 
                        ON t.finished_at >= tb.bucket_start 
                        AND t.finished_at < tb.bucket_start + 900  -- 15-minute window
                    LEFT JOIN public.perft_items i 
                        ON t.perft_item_id = i.id
                    WHERE t.depth = {0}
                    GROUP BY tb.bucket_start
                    ORDER BY timestamp
                    ", depth)
                .AsNoTracking()
                .ToListAsync(cancellationToken);


            return Ok(result);
        }

        [HttpGet("{depth}/leaderboard")]
        [ResponseCache(Duration = 120, VaryByQueryKeys = new[] { "depth" })]
        public async Task<IActionResult> GetLeaderboard(int depth, CancellationToken cancellationToken)
        {
            var stats = await _dbContext.PerftTasks
                .AsNoTracking()
                .Include(i => i.Account)
                .Where(i => i.FinishedAt > 0 && i.Depth == depth)
                .GroupBy(i => i.AccountId)
                .Select(g => new PerftLeaderboardResponse()
                {
                    AccountName = g.Select(i => i.Account != null ? i.Account.Name : "Unknown").FirstOrDefault(),
                    TotalNodes = (long)g.Sum(i => (float)i.Nps * (i.FinishedAt - i.StartedAt)),  // Total nodes produced
                    TotalTimeSeconds = g.Sum(i => i.FinishedAt - i.StartedAt),  // Total time in seconds across all tasks
                    CompletedTasks = g.Count()  // Number of tasks completed
                })
                .ToArrayAsync(cancellationToken);

      
            return Ok(stats);
        }


        [HttpGet("{depth}/results")]
        [ResponseCache(Duration = 120, VaryByQueryKeys = new[] { "depth" })]
        public async Task<IActionResult> GetResults(int depth,
           CancellationToken cancellationToken)
        {
            var result = await _dbContext.Database
                .SqlQueryRaw<PerftResult>(@"
            SELECT 
                SUM(t.nodes * i.occurrences) AS nodes,
                SUM(t.captures * i.occurrences) AS captures,
                SUM(t.enpassants * i.occurrences) AS enpassants,
                SUM(t.castles * i.occurrences) AS castles,
                SUM(t.promotions * i.occurrences) AS promotions,
                SUM(t.direct_checks * i.occurrences) AS direct_checks,
                SUM(t.single_discovered_check * i.occurrences) AS single_discovered_check,
                SUM(t.direct_discovered_check * i.occurrences) AS direct_discovered_check,
                SUM(t.double_discovered_check * i.occurrences) AS double_discovered_check,
                SUM(t.direct_checkmate * i.occurrences) AS direct_checkmate,
                SUM(t.single_discovered_checkmate * i.occurrences) AS single_discovered_checkmate,
                SUM(t.direct_discoverd_checkmate * i.occurrences) AS direct_discoverd_checkmate,
                SUM(t.double_discoverd_checkmate * i.occurrences) AS double_discoverd_checkmate
            FROM public.perft_tasks t
            JOIN public.perft_items i ON t.perft_item_id = i.id
            WHERE i.depth = {0}", depth)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [ApiKeyAuthorize]
        [HttpPost("{depth}/results")]
        public async Task<IActionResult> SubmitResults(int depth,
           [FromBody] PerftTaskResultBatch request,
           CancellationToken cancellationToken)
        {
            var currentTimestamp = _timeProvider.GetUtcNow().ToUnixTimeSeconds();

            // Extract IDs to fetch all tasks in one query
            var taskIds = request.Results.Select(r => r.PerftTaskId).ToList();

            // Fetch all tasks with related SearchItem in a single batch query
            var searchTasks = await _dbContext.PerftTasks
                .Include(s => s.PerftItem)
                .Where(t => taskIds.Contains(t.Id) && t.Depth == depth)
                .ToDictionaryAsync(t => t.Id, cancellationToken);

            if (searchTasks.Count == 0)
            {
                return NotFound();
            }

            // Process each request and update the corresponding task
            foreach (var result in request.Results)
            {
                if (!searchTasks.TryGetValue(result.PerftTaskId, out var searchTask))
                {
                    continue; // Skip if task not found (shouldn't happen)
                }

                // Update the search item (parent)
                searchTask.PerftItem.PassCount++;
                searchTask.PerftItem.AvailableAt = currentTimestamp;

                var finishedAt = currentTimestamp == searchTask.StartedAt ? currentTimestamp + 1 : currentTimestamp;

                // Update search task properties
                var duration = (ulong)(finishedAt - searchTask.StartedAt);
                if (duration > 0)
                {
                    searchTask.Nps = result.Nodes * (ulong)searchTask.PerftItem.Occurrences / duration;
                }
                else
                {
                    searchTask.Nps = result.Nodes * (ulong)searchTask.PerftItem.Occurrences;
                }

                searchTask.FinishedAt = finishedAt;
                searchTask.Nodes = result.Nodes;
                searchTask.Captures = result.Captures;
                searchTask.Enpassant = result.Enpassant;
                searchTask.Castles = result.Castles;
                searchTask.Promotions = result.Promotions;
                searchTask.DirectCheck = result.DirectCheck;
                searchTask.SingleDiscoveredCheck = result.SingleDiscoveredCheck;
                searchTask.DirectDiscoveredCheck = result.DirectDiscoveredCheck;
                searchTask.DoubleDiscoveredCheck = result.DoubleDiscoveredCheck;
                searchTask.DirectCheckmate = result.DirectCheckmate;
                searchTask.SingleDiscoveredCheckmate = result.SingleDiscoveredCheckmate;
                searchTask.DirectDiscoverdCheckmate = result.DirectDiscoverdCheckmate;
                searchTask.DoubleDiscoverdCheckmate = result.DoubleDiscoverdCheckmate;
            }

            // Bulk save changes in one transaction
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok();
        }

    }
}
