using MySqlConnector;

namespace anauma;

internal static class DatabaseConnectionFactory
{
    public static MySqlConnection CreateOpenConnection()
    {
        MySqlConnection connection =
            new MySqlConnection(AppConfig.ConnectionString);

        connection.Open();

        return connection;
    }
}
