using MySqlConnector;

namespace anauma;

internal sealed class RaceResultRepository
{
    private readonly MySqlConnection _connection;

    /// <summary>
    /// レース結果を保存するためのリポジトリを初期化します。
    /// </summary>
    public RaceResultRepository(
        MySqlConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// 取得したSEレコードから確定着順をまとめて保存します。
    /// </summary>
    public void SaveAll(
        IEnumerable<SeRecord> records)
    {
        Console.WriteLine();
        Console.WriteLine(
            "===== raceresults 保存開始 =====");

        foreach (SeRecord se in records)
        {
            Save(se);
        }

        Console.WriteLine(
            "raceresults 保存完了！");
    }

    /// <summary>
    /// 1頭分の着順結果をDBへ保存または更新します。
    /// </summary>
    private void Save(
        SeRecord se)
    {
        string raceId =
            RepositoryHelpers.BuildRaceId(
                se.Year,
                se.MonthDay,
                se.RacecourseCode,
                se.RaceNumber);

        int resultOrder =
            RepositoryHelpers.ParseInt(
                se.FinishOrder);

        const string sql = @"
INSERT INTO raceresults
(
    RaceID,
    HorseID,
    ResultOrder
)
VALUES
(
    @RaceID,
    @HorseID,
    @ResultOrder
)
ON DUPLICATE KEY UPDATE
    ResultOrder = VALUES(ResultOrder);";

        using MySqlCommand command =
            new MySqlCommand(
                sql,
                _connection);

        command.Parameters.AddWithValue(
            "@RaceID",
            raceId);

        command.Parameters.AddWithValue(
            "@HorseID",
            se.BloodlineNumber);

        command.Parameters.AddWithValue(
            "@ResultOrder",
            resultOrder);

        command.ExecuteNonQuery();
    }
}
