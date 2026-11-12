using MySqlConnector;

namespace anauma;

internal sealed class RaceRepository
{
    private readonly MySqlConnection _connection;

    /// <summary>
    /// レース情報を保存するためのリポジトリを初期化します。
    /// </summary>
    public RaceRepository(
        MySqlConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// 取得したRAレコードを順番にDBへ保存します。
    /// </summary>
    public void SaveAll(
        IEnumerable<RaRecord> records)
    {
        Console.WriteLine();
        Console.WriteLine(
            "===== races 保存開始 =====");

        foreach (RaRecord ra in records)
        {
            Save(ra);
        }

        Console.WriteLine(
            "races 保存完了！");
    }

    /// <summary>
    /// 1レース分の基本情報をDBへ保存または更新します。
    /// </summary>
    private void Save(
        RaRecord ra)
    {
        string raceId =
            RepositoryHelpers.BuildRaceId(
                ra.Year,
                ra.MonthDay,
                ra.RacecourseCode,
                ra.RaceNumber);

        string raceDate =
            $"{ra.Year}-" +
            $"{ra.MonthDay.Substring(0, 2)}-" +
            $"{ra.MonthDay.Substring(2, 2)}";

        const string sql = @"
INSERT INTO races
(
    RaceID,
    RaceDate,
    TrackCode,
    Distance,
    TrackType,
    Weather,
    TrackCondition,
    RaceNumber,
    Kai,
    Day,
    RaceName,
    GradeCode,
    StartTime,
    StartersCount,
    FinishersCount,
    RaceTypeCode,
    TurfConditionCode,
    DirtConditionCode,
    DataCreateDate
)
VALUES
(
    @RaceID,
    @RaceDate,
    @TrackCode,
    @Distance,
    @TrackType,
    @Weather,
    @TrackCondition,
    @RaceNumber,
    @Kai,
    @Day,
    @RaceName,
    @GradeCode,
    @StartTime,
    @StartersCount,
    @FinishersCount,
    @RaceTypeCode,
    @TurfConditionCode,
    @DirtConditionCode,
    @DataCreateDate
)
ON DUPLICATE KEY UPDATE
    RaceDate = VALUES(RaceDate),
    TrackCode = VALUES(TrackCode),
    Distance = VALUES(Distance),
    TrackType = VALUES(TrackType),
    Weather = VALUES(Weather),
    TrackCondition = VALUES(TrackCondition),
    RaceNumber = VALUES(RaceNumber),
    Kai = VALUES(Kai),
    Day = VALUES(Day),
    RaceName = VALUES(RaceName),
    GradeCode = VALUES(GradeCode),
    StartTime = VALUES(StartTime),
    StartersCount = VALUES(StartersCount),
    FinishersCount = VALUES(FinishersCount),
    RaceTypeCode = VALUES(RaceTypeCode),
    TurfConditionCode = VALUES(TurfConditionCode),
    DirtConditionCode = VALUES(DirtConditionCode),
    DataCreateDate = VALUES(DataCreateDate);";

        using MySqlCommand command =
            new MySqlCommand(
                sql,
                _connection);

        command.Parameters.AddWithValue(
            "@RaceID",
            raceId);

        command.Parameters.AddWithValue(
            "@RaceDate",
            raceDate);

        command.Parameters.AddWithValue(
            "@TrackCode",
            ra.RacecourseCode);

        command.Parameters.AddWithValue(
            "@Distance",
            RepositoryHelpers.ParseInt(
                ra.Distance));

        command.Parameters.AddWithValue(
            "@TrackType",
            ra.TrackCode);

        command.Parameters.AddWithValue(
            "@Weather",
            ra.WeatherCode);

        command.Parameters.AddWithValue(
            "@TrackCondition",
            ra.TurfConditionCode != "0"
                ? ra.TurfConditionCode
                : ra.DirtConditionCode);

        command.Parameters.AddWithValue(
            "@RaceNumber",
            RepositoryHelpers.ParseInt(
                ra.RaceNumber));

        command.Parameters.AddWithValue(
            "@Kai",
            RepositoryHelpers.ParseInt(
                ra.Kai));

        command.Parameters.AddWithValue(
            "@Day",
            RepositoryHelpers.ParseInt(
                ra.Day));

        command.Parameters.AddWithValue(
            "@RaceName",
            ra.RaceNameMain);

        command.Parameters.AddWithValue(
            "@GradeCode",
            ra.GradeCode);

        command.Parameters.AddWithValue(
            "@StartTime",
            ra.StartTime);

        command.Parameters.AddWithValue(
            "@StartersCount",
            RepositoryHelpers.ParseInt(
                ra.StartersCount));

        command.Parameters.AddWithValue(
            "@FinishersCount",
            RepositoryHelpers.ParseInt(
                ra.FinishersCount));

        command.Parameters.AddWithValue(
            "@RaceTypeCode",
            ra.RaceTypeCode);

        command.Parameters.AddWithValue(
            "@TurfConditionCode",
            ra.TurfConditionCode);

        command.Parameters.AddWithValue(
            "@DirtConditionCode",
            ra.DirtConditionCode);

        command.Parameters.AddWithValue(
            "@DataCreateDate",
            ra.DataCreateDate);

        command.ExecuteNonQuery();
    }
}
