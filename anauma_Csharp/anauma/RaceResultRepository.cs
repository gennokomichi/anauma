using MySqlConnector;

namespace anauma;

internal sealed class RaceResultRepository
{
    private readonly MySqlConnection _connection;

    public RaceResultRepository(
        MySqlConnection connection)
    {
        _connection = connection;
    }

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
