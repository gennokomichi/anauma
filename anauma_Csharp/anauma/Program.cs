using System.Globalization;
using System.Text;
using MySqlConnector;

namespace anauma;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        Encoding.RegisterProvider(
            CodePagesEncodingProvider.Instance);

        Console.OutputEncoding =
            Encoding.UTF8;

        Console.WriteLine(
            "=================================");

        Console.WriteLine(
            " JRA-VAN JV-Link データ取得");

        Console.WriteLine(
            "=================================");

        try
        {
            // =====================================
            // MySQL接続
            // =====================================

            using MySqlConnection connection =
                DatabaseConnectionFactory
                    .CreateOpenConnection();

            Console.WriteLine();
            Console.WriteLine(
                "========================");

            Console.WriteLine(
                " MySQL接続成功！");

            Console.WriteLine(
                "========================");

            // =====================================
            // 通常RACEデータ取得
            // =====================================

            JvLinkService jvLinkService =
                new JvLinkService();

            JvReadResult result =
                jvLinkService.ReadRaceData(
                    AppConfig.DataSpec,
                    AppConfig.FromTime,
                    AppConfig.OpenOption);

            ConsoleReporter.PrintSummary(
                result);

            // =====================================
            // Repository作成
            // =====================================

            RaceRepository raceRepository =
                new RaceRepository(
                    connection);

            HorseRepository horseRepository =
                new HorseRepository(
                    connection);

            RaceEntryRepository raceEntryRepository =
                new RaceEntryRepository(
                    connection);

            RaceResultRepository raceResultRepository =
                new RaceResultRepository(
                    connection);

            // =====================================
            // 通常データ保存
            // =====================================

            raceRepository.SaveAll(
                result.RaRecords);

            horseRepository.SaveAll(
                result.SeRecords);

            raceEntryRepository.SaveAll(
                result.SeRecords);

            raceResultRepository.SaveAll(
                result.SeRecords);

            Console.WriteLine();
            Console.WriteLine(
                "---------------------------------");

            Console.WriteLine(
                "すべての保存処理が完了しました。");

            Console.WriteLine(
                $"取得レコード数 : " +
                $"{result.ReadRecordCount}");

            Console.WriteLine(
                "---------------------------------");

            // =====================================
            // 次回JRA開催日を自動判定
            // =====================================

            DateTime today =
    DateTime.Today;

            var futureRaceDates =
                result.RaRecords
                    .Select(ra =>
                        TryBuildRaceDate(
                            ra.Year,
                            ra.MonthDay))
                    .Where(date =>
                        date.HasValue
                        && date.Value.Date >=
                           today)
                    .Select(date =>
                        date!.Value.Date)
                    .Distinct()
                    .OrderBy(date =>
                        date)
                    .ToList();

            if (futureRaceDates.Count == 0)
            {
                Console.WriteLine();
                Console.WriteLine(
                    "今後の開催日が見つかりませんでした。");

                Console.WriteLine(
                    "JRA-VANの取得期間を確認してください。");

                return;
            }

            DateTime targetDate =
                futureRaceDates.First();

            string targetDateText =
                targetDate.ToString(
                    "yyyyMMdd");

            Console.WriteLine();
            Console.WriteLine(
                "=================================");

            Console.WriteLine(
                " 次回JRA開催日");

            Console.WriteLine(
                "=================================");

            Console.WriteLine(
                $"対象日 : " +
                $"{targetDate:yyyy-MM-dd}");

            // =====================================
            // 次回開催日のRaceIDを全件取得
            // =====================================

            var targetRaceIds =
                result.RaRecords
                    .Select(ra =>
                        RepositoryHelpers
                            .BuildRaceId(
                                ra.Year,
                                ra.MonthDay,
                                ra.RacecourseCode,
                                ra.RaceNumber))
                    .Where(raceId =>
                        raceId.StartsWith(
                            targetDateText))
                    .Distinct()
                    .OrderBy(raceId =>
                        raceId)
                    .ToList();

            Console.WriteLine(
                $"対象レース数 : " +
                $"{targetRaceIds.Count}");

            if (targetRaceIds.Count == 0)
            {
                Console.WriteLine(
                    "対象レースがありません。");

                return;
            }

            // =====================================
            // 次回開催日の速報オッズを全レース取得
            // =====================================

            Console.WriteLine();
            Console.WriteLine(
                "=================================");

            Console.WriteLine(
                " 速報オッズ 全レース取得");

            Console.WriteLine(
                "=================================");

            JvLinkService oddsService =
                new JvLinkService();

            int successRaceCount = 0;
            int failedRaceCount = 0;
            int updatedRecordCount = 0;

            foreach (
                string raceId
                in targetRaceIds)
            {
                try
                {
                    Console.WriteLine();
                    Console.WriteLine(
                        "---------------------------------");

                    Console.WriteLine(
                        $"速報オッズ取得 : " +
                        $"{raceId}");

                    Console.WriteLine(
                        "---------------------------------");

                    List<O1Record> oddsRecords =
                        oddsService
                            .ReadRealtimeOdds(
                                raceId);

                    if (oddsRecords.Count == 0)
                    {
                        Console.WriteLine(
                            "O1データはまだありません。");

                        continue;
                    }

                    foreach (
                        O1Record o1
                        in oddsRecords)
                    {
                        raceEntryRepository
                            .UpdateOddsAndPopularity(
                                o1);

                        updatedRecordCount +=
                            o1.WinOdds.Count;
                    }

                    successRaceCount++;
                }
                catch (Exception ex)
                {
                    failedRaceCount++;

                    Console.WriteLine();
                    Console.WriteLine(
                        "===== 速報オッズ取得失敗 =====");

                    Console.WriteLine(
                        $"RaceID : {raceId}");

                    Console.WriteLine(
                        ex.Message);

                    Console.WriteLine(
                        "このレースはスキップして" +
                        "次へ進みます。");
                }
            }

            // =====================================
            // 結果表示
            // =====================================

            Console.WriteLine();
            Console.WriteLine(
                "=================================");

            Console.WriteLine(
                " 速報オッズ取得結果");

            Console.WriteLine(
                "=================================");

            Console.WriteLine(
                $"開催日       : " +
                $"{targetDate:yyyy-MM-dd}");

            Console.WriteLine(
                $"対象レース数 : " +
                $"{targetRaceIds.Count}");

            Console.WriteLine(
                $"取得成功     : " +
                $"{successRaceCount}");

            Console.WriteLine(
                $"取得失敗     : " +
                $"{failedRaceCount}");

            Console.WriteLine(
                $"オッズ件数   : " +
                $"{updatedRecordCount}");

            Console.WriteLine(
                "=================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine(
                "========================");

            Console.WriteLine(
                " 処理中にエラーが発生しました");

            Console.WriteLine(
                "========================");

            Console.WriteLine(
                ex);
        }

        Console.WriteLine();
        Console.WriteLine(
            "Enterキーで終了します。");

        Console.ReadLine();
    }

    // =========================================
    // RAレコードから開催日を作成
    //
    // Year     = 2026
    // MonthDay = 1003
    //
    // → 2026-10-03
    // =========================================

    private static DateTime? TryBuildRaceDate(
        string year,
        string monthDay)
    {
        string value =
            year.Trim()
            + monthDay.Trim();

        if (DateTime.TryParseExact(
            value,
            "yyyyMMdd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime date))
        {
            return date;
        }

        return null;
    }
}