using System.Xml.Linq;

namespace GrandChessTree.Client
{
    public class WorkerReport
    {
        public string Fen { get; set; } = "";
        public int TotalSubtasks { get; set; }
        public int CompletedSubtasks { get; set; }
        public float Nps { get; set; }
        public int TotalCompletedTasks { get; set; }
        public int TotalCompletedSubTasks { get; set; }
        public int TotalCachedSubTasks { get; set; }
        public long WorkerCpuTime { get; set; }
        public ulong WorkerComputedNodes { get; set; }
        public ulong TotalComputedNodes { get; set; }
        public ulong TotalNodes { get; set; }

        public bool IsRunning { get; set; }

        public void BeginTask(PerftTask task, int workItemOccurrences)
        {
            Fen = task.Fen;
            TotalSubtasks = task.SubTaskCount;
            CompletedSubtasks = task.CompletedSubTaskResults.Count;
            TotalCompletedSubTasks += task.CachedSubTaskCount;
            TotalCachedSubTasks += task.CachedSubTaskCount;
            TotalNodes += (ulong)workItemOccurrences * (ulong)task.CompletedSubTaskResults.Sum(t => (float)t.Results[0] * t.Occurrences);
            WorkerComputedNodes = 0;
        }

        public void BeginSubTask(PerftTask task)
        {
            Fen = task.Fen;
            TotalSubtasks = task.SubTaskCount;
            CompletedSubtasks = task.CompletedSubTaskResults.Count;
        }

        public void EndSubTaskWorkCompleted(PerftTask task, ulong nodes, int workItemOccurrences, int subTaskOccurrences)
        {
            Fen = task.Fen;
            TotalSubtasks = task.SubTaskCount;
            CompletedSubtasks = task.CompletedSubTaskResults.Count;
            TotalCompletedSubTasks++;
            TotalNodes += nodes * (ulong)workItemOccurrences * (ulong)subTaskOccurrences;
            TotalComputedNodes += nodes;
            WorkerComputedNodes += nodes;

        }

        public void EndSubTaskFoundInCache(PerftTask task, ulong nodes, int workItemOccurrences, int subTaskOccurrences)
        {
            Fen = task.Fen;
            TotalSubtasks = task.SubTaskCount;
            CompletedSubtasks = task.CompletedSubTaskResults.Count;
            TotalCompletedSubTasks++;
            TotalCachedSubTasks++;
            TotalNodes += nodes * (ulong)workItemOccurrences * (ulong)subTaskOccurrences;
        }

        public void CompleteTask(PerftTask task, long duration)
        {
            Fen = task.Fen;
            TotalSubtasks = task.SubTaskCount;
            CompletedSubtasks = task.CompletedSubTaskResults.Count;
            TotalCompletedTasks++;
            WorkerCpuTime += duration;
        }
    }
 }
