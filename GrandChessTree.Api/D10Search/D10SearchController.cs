using GrandChessTree.Api.D10Search;
using GrandChessTree.Api.Database;
using GrandChessTree.Shared.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrandChessTree.Api.Controllers
{
    [ApiController]
    [Route("api/v1/search/d10/tasks")]
    public class D10SearchController : ControllerBase
    {     
        private readonly ILogger<D10SearchController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly TimeProvider _timeProvider;
        public D10SearchController(ILogger<D10SearchController> logger, ApplicationDbContext dbContext, TimeProvider timeProvider)
        {
            _logger = logger;
            _dbContext = dbContext;
            _timeProvider = timeProvider;
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewTaskBatch(CancellationToken cancellationToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var currentTimestamp = _timeProvider.GetUtcNow().ToUnixTimeSeconds();

            // Select and lock 400 available search items (Prevents duplicates)
            var searchItems = await _dbContext.D10SearchItems
                .FromSqlInterpolated($@"
                    SELECT * FROM public.d10_search_items 
                    WHERE NOT confirmed AND available_at = 0 
                    ORDER BY available_at, pass_count 
                    LIMIT 400 FOR UPDATE SKIP LOCKED")
                .ToListAsync(cancellationToken);

            if (!searchItems.Any())
            {
                return Conflict();
            }

            // Prepare search tasks
            var searchTasks = searchItems.Select(searchItem => new D10SearchTask
            {
                SearchItemId = searchItem.Id,
                StartedAt = currentTimestamp
            }).ToList();

            await _dbContext.D10SearchTasks.AddRangeAsync(searchTasks, cancellationToken);

            // Update search items to prevent immediate reprocessing
            foreach (var item in searchItems)
            {
                item.AvailableAt = currentTimestamp + 3600; // Becomes available again in 1 hour
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Commit transaction to finalize changes
            await transaction.CommitAsync(cancellationToken);

            // Prepare response
            var response = searchTasks.Select(task => new D10SearchTaskResponse
            {
                SearchItemId = task.SearchItemId,
                Id = task.Id,
                Depth = 6
            }).ToArray();

            return Ok(response);
        }


        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
        {
            var currentTimestamp = _timeProvider.GetUtcNow().ToUnixTimeSeconds();
            var pastMinuteTimestamp = currentTimestamp - 60;

            var stats = await _dbContext.D10SearchTasks
                .AsNoTracking()
                .Where(i => i.FinishedAt > pastMinuteTimestamp)
                .GroupBy(i => 1)
                .Select(g => new
                {
                    // Total nodes produced in the last minute
                    TotalNodes = g.Sum(i => (float)i.Nps * (i.FinishedAt - i.StartedAt)),

                    // Total time in seconds across all tasks
                    TotalTimeSeconds = g.Sum(i => i.FinishedAt - i.StartedAt),

                    // Number of tasks completed in the last minute
                    TasksPerMinute = g.Count()
                })
                .FirstOrDefaultAsync(cancellationToken);

            float nps = 0;
            if (stats?.TotalTimeSeconds > 0)
            {
                nps = (float)stats.TotalNodes / 60 ;
            }

            if (stats == null)
            {
                return Ok(new D10SearchTaskStatsResponse()
                {
                    Nps = 0,
                    Tpm = 0
                });
            }

            return Ok(new D10SearchTaskStatsResponse()
            {
                Nps = nps,
                Tpm = stats.TasksPerMinute
            });
        }


        [HttpGet("results")]
        public async Task<IActionResult> GetResults(
           CancellationToken cancellationToken)
        {
            var result = await _dbContext.Database
                .SqlQueryRaw<SearchResult>($@"
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
            FROM public.d10_search_tasks t
            JOIN public.d10_search_items i ON t.search_item_id = i.id
        ")
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost("results")]
        public async Task<IActionResult> SubmitResults(
           [FromBody] SearchTaskResultBatch request,
           CancellationToken cancellationToken)
        {
            var currentTimestamp = _timeProvider.GetUtcNow().ToUnixTimeSeconds();

                       // Extract IDs to fetch all tasks in one query
            var taskIds = request.Results.Select(r => r.Id).ToList();

            // Fetch all tasks with related SearchItem in a single batch query
            var searchTasks = await _dbContext.D10SearchTasks
                .Include(s => s.SearchItem)
                .Where(t => taskIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, cancellationToken);

            if (searchTasks.Count == 0)
            {
                return NotFound();
            }

            // Process each request and update the corresponding task
            foreach (var result in request.Results)
            {
                if (!searchTasks.TryGetValue(result.Id, out var searchTask))
                {
                    continue; // Skip if task not found (shouldn't happen)
                }

                // Update the search item (parent)
                searchTask.SearchItem.PassCount++;
                searchTask.SearchItem.AvailableAt = currentTimestamp;

                var finishedAt = currentTimestamp == searchTask.StartedAt ? currentTimestamp + 1 : currentTimestamp;

                // Update search task properties
                var duration = (ulong)(finishedAt - searchTask.StartedAt);
                if (duration > 0)
                {
                    searchTask.Nps = result.Nodes * (ulong)searchTask.SearchItem.Occurrences / duration;
                }
                else
                {
                    searchTask.Nps = result.Nodes * (ulong)searchTask.SearchItem.Occurrences;
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
