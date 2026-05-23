using GrandChessTree.Shared.Api;
using GrandChessTree.Shared.ApiKeys;
using Npgsql;

public static class ApiKeySeeder
{
    public static async Task Seed()
    {
        Console.WriteLine("Enter PostgreSQL connection string...");
        var connectionString = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("Error: Connection string cannot be empty.");
            return;
        }

        Console.Write("Enter Account ID: ");
        if (!long.TryParse(Console.ReadLine(), out long accountId))
        {
            Console.WriteLine("Error: Invalid Account ID.");
            return;
        }

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        // Fetch account role
        const string getAccountQuery = "SELECT role FROM accounts WHERE id = @id;";
        await using var getCmd = new NpgsqlCommand(getAccountQuery, conn);
        getCmd.Parameters.AddWithValue("id", accountId);

        var roleObj = await getCmd.ExecuteScalarAsync();
        if (roleObj == null)
        {
            Console.WriteLine("Error: Account not found.");
            return;
        }

        var role = Enum.Parse<AccountRole>(roleObj.ToString()!);

        var (id, apiKey, tail) = ApiKeyGenerator.Create();

        // Insert into DB
        const string insertQuery = @"
            INSERT INTO api_keys (id, created_at, apikey_tail, role, account_id) 
            VALUES (@id, @created_at, @apikey_tail, @role, @account_id);";

        await using var cmd = new NpgsqlCommand(insertQuery, conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("created_at", currentTime);
        cmd.Parameters.AddWithValue("apikey_tail", tail);
        cmd.Parameters.AddWithValue("role", role.ToString());
        cmd.Parameters.AddWithValue("account_id", accountId);

        await cmd.ExecuteNonQueryAsync();

        Console.WriteLine($"API Key successfully generated! Here is your key: {apiKey}");
    }
}
