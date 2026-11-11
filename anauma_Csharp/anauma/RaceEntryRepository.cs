using MySqlConnector;

namespace anauma;

internal sealed class RaceEntryRepository
{
    private readonly MySqlConnection _connection;


    public RaceEntryRepository(
        MySqlConnection connection)
    {
        _connection = connection;
    }


    // =========================================
    // 通常のSEレコード保存
    // =========================================

    public void SaveAll(
        IEnumerable<SeRecord> records)
    {
        Console.WriteLine();

        Console.WriteLine(
            "===== raceentries 保存開始 =====");


        foreach (
            SeRecord se
            in records)
        {
            Save(se);
        }


        Console.WriteLine(
            "raceentries 保存完了！");
    }


    // =========================================
    // O1速報オッズをraceentriesへ反映
    // =========================================

    public void UpdateOddsAndPopularity(
        O1Record o1)
    {
        Console.WriteLine();

        Console.WriteLine(
            "===== 速報オッズ DB更新開始 =====");


        Console.WriteLine(
            $"RaceID     : {o1.RaceId}");


        Console.WriteLine(
            $"データ区分 : "
            + $"{GetO1DataKubunName(
                o1.DataKubun)}");


        Console.WriteLine(
            $"発表Raw    : "
            + $"{o1.PublishedRaw}");


        Console.WriteLine(
            $"発表日時   : "
            + $"{FormatPublishedTime(
                o1.PublishedAt)}");


        const string sql = @"
UPDATE raceentries
SET
    Odds = @Odds,
    Popularity = @Popularity
WHERE
    RaceID = @RaceID
    AND GateNumber = @GateNumber;";


        int updatedCount = 0;


        foreach (
            O1WinOdds odds
            in o1.WinOdds)
        {
            using MySqlCommand command =
                new MySqlCommand(
                    sql,
                    _connection);


            command.Parameters.AddWithValue(
                "@RaceID",
                o1.RaceId);


            command.Parameters.AddWithValue(
                "@GateNumber",
                odds.HorseNumber);


            command.Parameters.AddWithValue(
                "@Odds",
                Convert.ToDecimal(
                    odds.Odds));


            command.Parameters.AddWithValue(
                "@Popularity",
                odds.Popularity);


            int affectedRows =
                command.ExecuteNonQuery();


            if (affectedRows > 0)
            {
                updatedCount +=
                    affectedRows;


                Console.WriteLine(
                    $"{odds.HorseNumber}番 "
                    + $"単勝 "
                    + $"{odds.Odds:F1}倍 "
                    + $"{odds.Popularity}"
                    + "番人気 "
                    + "→ 更新成功");
            }
            else
            {
                Console.WriteLine(
                    $"{odds.HorseNumber}番 "
                    + "→ 更新対象なし");
            }
        }


        Console.WriteLine();

        Console.WriteLine(
            $"更新件数 : "
            + $"{updatedCount}");


        Console.WriteLine(
            "===== 速報オッズ DB更新完了 =====");
    }


    // =========================================
    // SEレコード1件保存
    // =========================================

    private void Save(
        SeRecord se)
    {
        string raceId =
            RepositoryHelpers
                .BuildRaceId(
                    se.Year,
                    se.MonthDay,
                    se.RacecourseCode,
                    se.RaceNumber);


        int gateNumber =
            RepositoryHelpers
                .ParseInt(
                    se.HorseNumber);


        decimal odds =
            RepositoryHelpers
                .ParseTenths(
                    se.WinOdds);


        int popularity =
            RepositoryHelpers
                .ParseInt(
                    se.WinPopularity);


        int frameNumber =
            RepositoryHelpers
                .ParseInt(
                    se.FrameNumber);


        decimal weightCarried =
            RepositoryHelpers
                .ParseTenths(
                    se.WeightCarried);


        int horseWeight =
            RepositoryHelpers
                .ParseInt(
                    se.HorseWeight);


        int horseWeightDiff =
            RepositoryHelpers
                .ParseInt(
                    se.HorseWeightDiff);


        if (se.HorseWeightSign == "-")
        {
            horseWeightDiff *= -1;
        }


        const string sql = @"
INSERT INTO raceentries
(
    RaceID,
    HorseID,
    GateNumber,
    Odds,
    Popularity,
    FrameNumber,
    WeightCarried,
    JockeyCode,
    JockeyName,
    HorseWeight,
    HorseWeightDiff,
    DataCreateDate
)
VALUES
(
    @RaceID,
    @HorseID,
    @GateNumber,
    @Odds,
    @Popularity,
    @FrameNumber,
    @WeightCarried,
    @JockeyCode,
    @JockeyName,
    @HorseWeight,
    @HorseWeightDiff,
    @DataCreateDate
)
ON DUPLICATE KEY UPDATE
    GateNumber = VALUES(GateNumber),
    Odds = VALUES(Odds),
    Popularity = VALUES(Popularity),
    FrameNumber = VALUES(FrameNumber),
    WeightCarried = VALUES(WeightCarried),
    JockeyCode = VALUES(JockeyCode),
    JockeyName = VALUES(JockeyName),
    HorseWeight = VALUES(HorseWeight),
    HorseWeightDiff = VALUES(HorseWeightDiff),
    DataCreateDate = VALUES(DataCreateDate);";


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
            "@GateNumber",
            gateNumber);


        command.Parameters.AddWithValue(
            "@Odds",
            odds);


        command.Parameters.AddWithValue(
            "@Popularity",
            popularity);


        command.Parameters.AddWithValue(
            "@FrameNumber",
            frameNumber);


        command.Parameters.AddWithValue(
            "@WeightCarried",
            weightCarried);


        command.Parameters.AddWithValue(
            "@JockeyCode",
            se.JockeyCode);


        command.Parameters.AddWithValue(
            "@JockeyName",
            se.JockeyName);


        command.Parameters.AddWithValue(
            "@HorseWeight",
            horseWeight);


        command.Parameters.AddWithValue(
            "@HorseWeightDiff",
            horseWeightDiff);


        command.Parameters.AddWithValue(
            "@DataCreateDate",
            se.DataCreateDate);


        command.ExecuteNonQuery();
    }


    // =========================================
    // O1発表日時表示
    // =========================================

    private static string FormatPublishedTime(
        DateTime? publishedAt)
    {
        if (!publishedAt.HasValue)
        {
            return "不明";
        }


        return publishedAt
            .Value
            .ToString(
                "yyyy-MM-dd HH:mm");
    }


    // =========================================
    // O1データ区分表示
    // =========================================

    private static string GetO1DataKubunName(
        int dataKubun)
    {
        return dataKubun switch
        {
            1 => "中間",
            2 => "前日売最終",
            3 => "最終",
            4 => "確定",
            5 => "確定(月曜)",
            _ => $"不明({dataKubun})"
        };
    }
}