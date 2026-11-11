using MySqlConnector;

namespace anauma;

internal sealed class HorseRepository
{
    private readonly MySqlConnection _connection;

    public HorseRepository(
        MySqlConnection connection)
    {
        _connection = connection;
    }

    public void SaveAll(
        IEnumerable<SeRecord> records)
    {
        Console.WriteLine();
        Console.WriteLine(
            "===== horses 保存開始 =====");

        foreach (SeRecord se in records)
        {
            Save(se);
        }

        Console.WriteLine(
            "horses 保存完了！");
    }

    private void Save(
        SeRecord se)
    {
        const string sql = @"
INSERT INTO horses
(
    HorseID,
    HorseName
)
VALUES
(
    @HorseID,
    @HorseName
)
ON DUPLICATE KEY UPDATE
    HorseName = VALUES(HorseName);";

        using MySqlCommand command =
            new MySqlCommand(
                sql,
                _connection);

        command.Parameters.AddWithValue(
            "@HorseID",
            se.BloodlineNumber);

        command.Parameters.AddWithValue(
            "@HorseName",
            se.HorseName);

        command.ExecuteNonQuery();
    }
}
