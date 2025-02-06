using System.Text;
using System.Text.Json;

namespace GrandChessTree.Client
{
    public static class SearchItemPersistence
    {
        private static readonly string StoragePath = "search_tasks"; // Directory to store files

        static SearchItemPersistence()
        {
            // Ensure storage directory exists
            Directory.CreateDirectory(StoragePath);
        }

        public static async Task<PerftTask?> LoadSearchTask(long id)
        {
            string filePath = GetFilePath(id);
            if (!File.Exists(filePath))
                return null;

            byte[] data = await File.ReadAllBytesAsync(filePath);
            return JsonSerializer.Deserialize(Encoding.UTF8.GetString(data), SourceGenerationContext.Default.PerftTask);
        }

        public static async Task Save(PerftTask task)
        {
            string filePath = GetFilePath(task.PerftTaskId);

            string json = JsonSerializer.Serialize(task, SourceGenerationContext.Default.PerftTask);

            byte[] data = Encoding.UTF8.GetBytes(json);
            await File.WriteAllBytesAsync(filePath, data);
        }

        private static string GetFilePath(long id) => Path.Combine(StoragePath, $"{id}.json");
    }
 }
