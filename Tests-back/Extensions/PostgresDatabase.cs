using Npgsql;

namespace Tests_back.Extensions;

public static class PostgresDatabase
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=p2p_db;Username=postgres;Password=postgres";

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