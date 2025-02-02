namespace GrandChessTree.Client
{
    public class WorkerReport
    {
        public string Fen { get; set; } = "";
        public int TotalSubtasks { get; set; }
        public int CompletedSubtasks { get; set; }
        public float Nps { get; set; }
        public ulong Nodes { get; set; }
        public int TotalCompletedTasks { get; set; }
        public int TotalCompletedSubTasks { get; set; }

        public void BeginSubTask(LocalSearchTask task)
        {
            Fen = task.Fen;
            TotalSubtasks = task.SubTaskCount;
            CompletedSubtasks = task.CompletedSubTaskResults.Count;
            Nodes = (ulong)task.CompletedSubTaskResults.Sum(t => (float)t.results[0] * t.occurences);
        }

        public void EndSubTask(LocalSearchTask task, float nps)
        {
            Fen = task.Fen;
            TotalSubtasks = task.SubTaskCount;
            CompletedSubtasks = task.CompletedSubTaskResults.Count;
            Nps = nps;
            Nodes = (ulong)task.CompletedSubTaskResults.Sum(t => (float)t.results[0] * t.occurences);
            TotalCompletedSubTasks++;
        }

        public void CompleteTask(LocalSearchTask task)
        {
            Fen = task.Fen;
            TotalSubtasks = task.SubTaskCount;
            CompletedSubtasks = task.CompletedSubTaskResults.Count;
            Nodes = (ulong)task.CompletedSubTaskResults.Sum(t => (float)t.results[0] * t.occurences);
            TotalCompletedTasks++;
        }
    }
 }
