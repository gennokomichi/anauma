namespace anauma;

internal static class AppConfig
{
    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("ANAUMA_DB_CONNECTION_STRING")
        ?? throw new InvalidOperationException(
            "環境変数 ANAUMA_DB_CONNECTION_STRING が設定されていません。"
        );

    public const string DataSpec = "RACE";
    public const string FromTime = "20250901000000";
    public const int OpenOption = 4;

    public const int BufferSize = 1024 * 1024;
}