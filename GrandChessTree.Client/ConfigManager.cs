using System.Text.Json.Serialization;

namespace GrandChessTree.Client
{
    public class Config
    {
        [JsonPropertyName("api_url")]
        public string ApiUrl { get; set; } = "";

        [JsonPropertyName("api_key")]
        public string ApiKey { get; set; } = "";

        [JsonPropertyName("workers")]
        public int Workers { get; set; } = 4;
    }

    public static class ConfigManager
    {
        public static bool IsValidConfig(Config config)
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(config.ApiUrl))
            {
                Console.WriteLine("Error: API URL cannot be empty.");
                isValid = false;
            }
            else if (!Uri.TryCreate(config.ApiUrl, UriKind.Absolute, out _))
            {
                Console.WriteLine("Error: API URL is not a valid URI.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(config.ApiKey))
            {
                Console.WriteLine("Error: API Key cannot be empty.");
                isValid = false;
            }

            if (config.Workers <= 0)
            {
                Console.WriteLine("Error: Workers must be greater than zero.");
                isValid = false;
            }

            return isValid;
        }


        public static Config LoadOrCreateConfig()
        {
            var config = WorkerPersistence.LoadConfig();
            if (config != null)
            {
                return config;
            }
            return CreateNewConfig();
        }



        private static Config CreateNewConfig()
        {
            var config = new Config();

            Console.Write("Enter API URL: ");
            config.ApiUrl = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Enter API Key: ");
            config.ApiKey = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Enter number of workers: ");
            if (int.TryParse(Console.ReadLine(), out int workers))
            {
                config.Workers = workers;
            }

            WorkerPersistence.SaveConfig(config);
            return config;
        }

    }

}
