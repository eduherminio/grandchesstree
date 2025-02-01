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

        public static async Task<LocalSearchTask?> LoadSearchTask(int id)
        {
            string filePath = GetFilePath(id);
            if (!File.Exists(filePath))
                return null;

            byte[] data = await File.ReadAllBytesAsync(filePath);
            return JsonSerializer.Deserialize<LocalSearchTask>(Encoding.UTF8.GetString(data));
        }

        public static async Task Save(LocalSearchTask task)
        {
            string filePath = GetFilePath(task.Id); // Ensure ID is numeric

            string json = JsonSerializer.Serialize(task, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            byte[] data = Encoding.UTF8.GetBytes(json);
            await File.WriteAllBytesAsync(filePath, data);
        }

        private static string GetFilePath(int id) => Path.Combine(StoragePath, $"{id}.json");
    }
 }
