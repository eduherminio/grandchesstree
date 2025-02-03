using GrandChessTree.Shared;
using Npgsql;

namespace GrandChessTree.Toolkit
{
    public static class PositionSeeder
    {
        public static async Task Seed(int depth)
        {
            Console.WriteLine("Enter pgsql connection string...");
            var connectionString = Console.ReadLine();

            // Open a connection to PostgreSQL
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            Console.WriteLine($"Starting bulk insert of {PositionsD4.Dict.Count} rows...");

            // Use COPY for bulk insert
            await using var writer = await conn.BeginTextImportAsync("COPY perft_items (hash, depth, available_at, pass_count, confirmed, occurrences) FROM STDIN (FORMAT csv)");

            try
            {
                // Iterate over the dictionary and write each row to the COPY stream
                foreach (var (hash, _) in PositionsD4.Dict)
                {
                    OccurrencesD4.Dict.TryGetValue(hash, out var occurrences);
                    await writer.WriteLineAsync($"{hash},{depth},0,0,false,{occurrences}");
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
