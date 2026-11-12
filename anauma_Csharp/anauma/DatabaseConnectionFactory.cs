using MySqlConnector;

namespace anauma;

internal static class DatabaseConnectionFactory
{
    /// <summary>
    /// 設定された接続文字列を使ってMySQL接続を作成し、接続済みの状態で返します。
    /// </summary>
    public static MySqlConnection CreateOpenConnection()
    {
        MySqlConnection connection =
            new MySqlConnection(AppConfig.ConnectionString);

        connection.Open();

        return connection;
    }
}
