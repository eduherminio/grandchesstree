using System.Text.Json.Serialization;
using GrandChessTree.Shared.Api;

namespace GrandChessTree.Client
{
    public class LocalSearchTask
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("fen")]
        public string Fen { get; set; } = "";

        [JsonPropertyName("search_item_id")]
        public ulong SearchItemId { get; set; }

        [JsonPropertyName("sub_task_depth")]
        public int SubTaskDepth { get; set; }

        [JsonPropertyName("sub_task_count")]
        public int SubTaskCount { get; set; }

        [JsonPropertyName("remaining_sub_tasks")]
        public List<string> RemainingSubTasks { get; set; } = new List<string>();

        [JsonPropertyName("completed_sub_tasks")]
        public List<ulong[]> CompletedSubTaskResults { get; set; } = new List<ulong[]>();

        public string? WorkingTask { get; set; }
        public bool IsCompleted()
        {
            return RemainingSubTasks.Count == 0 && SubTaskCount == CompletedSubTaskResults.Count;
        }

        public string? GetNextSubTask()
        {
            if(RemainingSubTasks.Count == 0) return null;
            WorkingTask = RemainingSubTasks[0];
            RemainingSubTasks.RemoveAt(0);
            return WorkingTask;
        }

        public bool CompleteSubTask(WorkerResult result)
        {
            WorkingTask = null;

            CompletedSubTaskResults.Add(
            [
                result.Nodes,
                result.Captures,
                result.Enpassant,
                result.Castles,
                result.Promotions,
                result.DirectCheck,
                result.SingleDiscoveredCheck,
                result.DirectDiscoveredCheck,
                result.DoubleDiscoveredCheck,
                result.DirectCheckmate,
                result.SingleDiscoveredCheckmate,
                result.DirectDiscoverdCheckmate,
                result.DoubleDiscoverdCheckmate,
            ]);

            return true;
        }

        public SearchTaskResults? ToSubmission()
        {
            if (!IsCompleted())
            {
                return null;
            }

            var request = new SearchTaskResults()
            {
                Id = Id,
                SearchItemId = SearchItemId,
                Nodes = 0,
                Captures = 0,
                Enpassant = 0,
                Castles = 0,
                Promotions = 0,
                DirectCheck = 0,
                SingleDiscoveredCheck = 0,
                DirectDiscoveredCheck = 0,
                DoubleDiscoveredCheck = 0,
                DirectCheckmate = 0,
                SingleDiscoveredCheckmate = 0,
                DirectDiscoverdCheckmate = 0,
                DoubleDiscoverdCheckmate = 0,
            };

            foreach(var subTaskResult in CompletedSubTaskResults)
            {
                if(subTaskResult.Length != 13)
                {
                    return null;
                }

                request.Nodes += subTaskResult[0];
                request.Captures += subTaskResult[1];
                request.Enpassant += subTaskResult[2];
                request.Castles += subTaskResult[3];
                request.Promotions += subTaskResult[4];
                request.DirectCheck += subTaskResult[5];
                request.SingleDiscoveredCheck += subTaskResult[6];
                request.DirectDiscoveredCheck += subTaskResult[7];
                request.DoubleDiscoveredCheck += subTaskResult[8];
                request.DirectCheckmate += subTaskResult[9];
                request.SingleDiscoveredCheckmate += subTaskResult[10];
                request.DirectDiscoverdCheckmate += subTaskResult[11];
                request.DoubleDiscoverdCheckmate += subTaskResult[12];
            }

            return request;
        }
    }
 }
