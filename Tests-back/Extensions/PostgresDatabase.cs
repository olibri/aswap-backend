using Npgsql;

namespace Tests_back.Extensions;

public static class PostgresDatabase
{
    private const string ConnectionString = "Host=65.109.86.205;Port=5434;Database=aswap_db;Username=aswap_user;Password=i6oDTaswap2pY9v2WqN7Dl6j0B0V8";

    public static void ResetState(string tableName)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();

        var query = $"TRUNCATE TABLE \"{tableName}\" RESTART IDENTITY CASCADE;";

        using var command = new NpgsqlCommand(query, connection);

        var result = command.ExecuteNonQuery();

        Console.WriteLine($"Table '{tableName}': Truncated with {result} affected rows (if applicable).");

        connection.Close();
    }
}
