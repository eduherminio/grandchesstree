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
        public async Task<IActionResult> CreateNewTask(CancellationToken cancellationToken)
        {
            var currentTimestamp = _timeProvider.GetUtcNow().ToUnixTimeSeconds();

            var searchItem = await _dbContext.D10SearchItems.Where(i => i.AvailableAt <= currentTimestamp && !i.Confirmed).OrderBy(i => i.PassCount).FirstOrDefaultAsync(cancellationToken);

            if(searchItem == null)
            {
                return Conflict();
            }

            // Becomes available again in 1 hour
            searchItem.AvailableAt = currentTimestamp + 60 * 60;

            var searchTask = new D10SearchTask()
            {
                SearchItemId = searchItem.Id,
                StartedAt = currentTimestamp,
            };

            _dbContext.D10SearchTasks.Add(searchTask);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(new D10SearchTaskResponse()
            {
                SearchItemId = searchItem.Id,
                Id = searchTask.Id
            });
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
                    NodesPerMinute = g.Sum(i => i.Nps) / 60,
                    TasksPerMinute = g.Count(i => i.FinishedAt > pastMinuteTimestamp)
                })
                .FirstOrDefaultAsync(cancellationToken);

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
                Nps = stats.NodesPerMinute,
                Tpm = stats.TasksPerMinute
            });
        }


        [HttpPost("{id}/result")]
        public async Task<IActionResult> SubmitResult(int id, [FromBody] SubmitSearchTaskResultRequest request, CancellationToken cancellationToken)
        {
            var currentTimestamp = _timeProvider.GetUtcNow().ToUnixTimeSeconds();

            var searchTask = await _dbContext.D10SearchTasks.Include(s => s.SearchItem).FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

            searchTask.SearchItem.PassCount++;
            searchTask.SearchItem.AvailableAt = currentTimestamp;

            var duration = (ulong)(currentTimestamp - searchTask.StartedAt);
            if(duration > 0)
            {
                searchTask.Nps = request.Nodes / duration;
            }
            searchTask.FinishedAt = currentTimestamp;
            searchTask.Nodes = request.Nodes;
            searchTask.Captures = request.Captures;
            searchTask.Enpassant = request.Enpassant;
            searchTask.Castles = request.Castles;
            searchTask.Promotions = request.Promotions;
            searchTask.DirectCheck = request.DirectCheck;
            searchTask.SingleDiscoveredCheck = request.SingleDiscoveredCheck;
            searchTask.DirectDiscoveredCheck = request.DirectDiscoveredCheck;
            searchTask.DoubleDiscoveredCheck = request.DoubleDiscoveredCheck;
            searchTask.DirectCheckmate = request.DirectCheckmate;
            searchTask.SingleDiscoveredCheckmate = request.SingleDiscoveredCheckmate;
            searchTask.DirectDiscoverdCheckmate = request.DirectDiscoverdCheckmate;
            searchTask.DoubleDiscoverdCheckmate = request.DoubleDiscoverdCheckmate;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Ok();
        }
    }
}
