using GrandChessTree.Shared;
using Npgsql;

namespace GrandChessTree.Toolkit
{
    public static class PositionD10Seeder
    {
        public static async Task SeedD10()
        {
            Console.WriteLine("Enter pgsql connection string...");
            var connectionString = Console.ReadLine();

            // Open a connection to PostgreSQL
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            Console.WriteLine($"Starting bulk insert of {PositionsD4.Dict.Count} rows...");

            // Use COPY for bulk insert
            await using var writer = await conn.BeginTextImportAsync("COPY d10_search_items (id, available_at, pass_count, confirmed) FROM STDIN (FORMAT csv)");

            try
            {
                // Iterate over the dictionary and write each row to the COPY stream
                foreach (var (id, _) in PositionsD4.Dict)
                {
                    await writer.WriteLineAsync($"{id},0,0,false"); // CSV format: id, available_at, pass_count
                }

                Console.WriteLine("Bulk insert completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during bulk insert: {ex.Message}");
            }
        }
    }
}
