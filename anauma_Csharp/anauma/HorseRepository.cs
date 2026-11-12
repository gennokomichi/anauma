using MySqlConnector;

namespace anauma;

internal sealed class HorseRepository
{
    private readonly MySqlConnection _connection;

    /// <summary>
    /// 馬マスタを保存するためのリポジトリを初期化します。
    /// </summary>
    public HorseRepository(
        MySqlConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// SEレコードから得た馬情報をまとめて保存します。既存馬は更新し、新規馬は追加します。
    /// </summary>
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

    /// <summary>
    /// 1頭分の馬情報をDBへ保存または更新します。
    /// </summary>
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
