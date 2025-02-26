﻿using Npgsql;

namespace GrandChessTree.Toolkit
{
    public static class PerftClearer
    {
        public static async Task FullReset(int depth)
        {
            Console.WriteLine("Enter PostgreSQL connection string...");
            var connectionString = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine("Error: Connection string cannot be empty.");
                return;
            }

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Delete from perft_tasks
                const string deleteQuery = "DELETE FROM perft_tasks WHERE depth = @depth;";

                await using (var deleteCmd = new NpgsqlCommand(deleteQuery, conn, transaction))
                {
                    deleteCmd.Parameters.AddWithValue("depth", depth);
                    int deletedRows = await deleteCmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"Deleted {deletedRows} rows from perft_tasks.");
                }

                // Update perft_items
                const string updateQuery = @"
                    UPDATE perft_items 
                    SET available_at = 0, pass_count = 0, confirmed = false 
                    WHERE depth = @depth;";

                await using (var updateCmd = new NpgsqlCommand(updateQuery, conn, transaction))
                {
                    updateCmd.Parameters.AddWithValue("depth", depth);
                    int updatedRows = await updateCmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"Updated {updatedRows} rows in perft_items.");
                }

                // Commit the transaction
                await transaction.CommitAsync();
                Console.WriteLine("Update and cleanup completed successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error during update and cleanup: {ex.Message}");
            }
        }

        public static async Task ReleaseIncompleteTasks(int depth)
        {
            Console.WriteLine("Enter PostgreSQL connection string...");
            var connectionString = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine("Error: Connection string cannot be empty.");
                return;
            }

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Get the current time minus 1 minute
                var timeLimit = DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds();

                // Delete incomplete tasks that started more than 1 minute ago and have not been finished
                const string deleteQuery = @"
            DELETE FROM perft_tasks 
            WHERE depth = @depth
            AND (finished_at IS NULL OR finished_at = 0) 
            AND started_at <= @timeLimit;";

                await using (var deleteCmd = new NpgsqlCommand(deleteQuery, conn, transaction))
                {
                    deleteCmd.Parameters.AddWithValue("depth", depth);
                    deleteCmd.Parameters.AddWithValue("timeLimit", timeLimit);

                    int deletedRows = await deleteCmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"Deleted {deletedRows} incomplete tasks from perft_tasks.");
                }

                // Reset the associated perft_items for the deleted tasks
                const string updateQuery = @"
                    UPDATE perft_items 
                    SET available_at = 0
                    WHERE depth = @depth;";

                await using (var updateCmd = new NpgsqlCommand(updateQuery, conn, transaction))
                {
                    updateCmd.Parameters.AddWithValue("depth", depth);
                    updateCmd.Parameters.AddWithValue("timeLimit", timeLimit);

                    int updatedRows = await updateCmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"Reset {updatedRows} perft_items.");
                }

                // Commit the transaction
                await transaction.CommitAsync();
                Console.WriteLine("Update and cleanup completed successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error during update and cleanup: {ex.Message}");
            }
        }



    }
}
