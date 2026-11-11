using System.Linq;

namespace anauma;

internal static class ConsoleReporter
{
    public static void PrintRa(
        RaRecord ra)
    {
        Console.WriteLine();
        Console.WriteLine(
            "===== RAレコード解析 =====");

        Console.WriteLine(
            $"レコード種別     : {ra.RecordType}");
        Console.WriteLine(
            $"データ区分       : {ra.DataKubun}");
        Console.WriteLine(
            $"データ作成日     : {ra.DataCreateDate}");
        Console.WriteLine(
            $"開催年           : {ra.Year}");
        Console.WriteLine(
            $"開催月日         : {ra.MonthDay}");
        Console.WriteLine(
            $"競馬場コード     : {ra.RacecourseCode}");
        Console.WriteLine(
            $"開催回           : {ra.Kai}");
        Console.WriteLine(
            $"開催日           : {ra.Day}");
        Console.WriteLine(
            $"レース番号       : {ra.RaceNumber}");
        Console.WriteLine(
            $"レース名         : {ra.RaceNameMain}");
        Console.WriteLine(
            $"グレード         : {ra.GradeCode}");
        Console.WriteLine(
            $"レース種別       : {ra.RaceTypeCode}");
        Console.WriteLine(
            $"距離             : {ra.Distance}");
        Console.WriteLine(
            $"トラック         : {ra.TrackCode}");
        Console.WriteLine(
            $"発走時刻         : {ra.StartTime}");
        Console.WriteLine(
            $"出走頭数         : {ra.StartersCount}");
        Console.WriteLine(
            $"完走頭数         : {ra.FinishersCount}");
        Console.WriteLine(
            $"天候             : {ra.WeatherCode}");
        Console.WriteLine(
            $"芝馬場状態       : {ra.TurfConditionCode}");
        Console.WriteLine(
            $"ダート馬場状態   : {ra.DirtConditionCode}");
    }

    public static void PrintSe(
        SeRecord se)
    {
        Console.WriteLine();
        Console.WriteLine(
            "===== SEレコード解析 =====");

        Console.WriteLine(
            $"開催年       : {se.Year}");
        Console.WriteLine(
            $"開催月日     : {se.MonthDay}");
        Console.WriteLine(
            $"競馬場       : {se.RacecourseCode}");
        Console.WriteLine(
            $"レース番号   : {se.RaceNumber}");
        Console.WriteLine(
            $"枠番         : {se.FrameNumber}");
        Console.WriteLine(
            $"馬番         : {se.HorseNumber}");
        Console.WriteLine(
            $"血統登録番号 : {se.BloodlineNumber}");
        Console.WriteLine(
            $"馬名         : {se.HorseName}");
        Console.WriteLine(
            $"負担重量     : {se.WeightCarried}");
        Console.WriteLine(
            $"騎手コード   : {se.JockeyCode}");
        Console.WriteLine(
            $"騎手名       : {se.JockeyName}");
        Console.WriteLine(
            $"馬体重       : {se.HorseWeight}");
        Console.WriteLine(
            $"増減符号     : {se.HorseWeightSign}");
        Console.WriteLine(
            $"増減差       : {se.HorseWeightDiff}");
        Console.WriteLine(
            $"確定着順     : {se.FinishOrder}");
        Console.WriteLine(
            $"単勝オッズ   : {se.WinOdds}");
        Console.WriteLine(
            $"単勝人気     : {se.WinPopularity}");
    }

    public static void PrintSummary(
        JvReadResult result)
    {
        Console.WriteLine();
        Console.WriteLine(
            "===== List確認 =====");
        Console.WriteLine(
            $"RA件数 : {result.RaRecords.Count}");
        Console.WriteLine(
            $"SE件数 : {result.SeRecords.Count}");

        Console.WriteLine();
        Console.WriteLine(
            "===== RAとSEの紐付け確認 =====");

        foreach (RaRecord ra in result.RaRecords)
        {
            List<SeRecord> horses =
                result.SeRecords
                    .Where(se =>
                        se.Year == ra.Year &&
                        se.MonthDay == ra.MonthDay &&
                        se.RacecourseCode == ra.RacecourseCode &&
                        se.Kai == ra.Kai &&
                        se.Day == ra.Day &&
                        se.RaceNumber == ra.RaceNumber)
                    .ToList();

            Console.WriteLine();
            Console.WriteLine(
                $"{ra.Year}/{ra.MonthDay} " +
                $"{ra.RacecourseCode} " +
                $"{ra.RaceNumber}R");

            Console.WriteLine(
                $"出走馬データ件数 : {horses.Count}");

            foreach (SeRecord horse in horses)
            {
                Console.WriteLine(
                    $"  {horse.HorseNumber}番 " +
                    $"{horse.HorseName} " +
                    $"{horse.FinishOrder}着 " +
                    $"{horse.WinPopularity}人気");
            }
        }
    }
}
