using System;
using System.Globalization;
using System.Text;

namespace anauma
{
    internal class JvRecordParser
    {
        // CP932（Shift-JIS）
        private static readonly Encoding JvEncoding =
            Encoding.GetEncoding(932);


        // =========================================
        // レコード種別
        // =========================================

        /// <summary>
        /// 固定長レコードの先頭2文字からレコード種別を取得します。

        /// </summary>
        public static string GetRecordType(
            string record)
        {
            if (
                string.IsNullOrEmpty(record)
                || record.Length < 2)
            {
                return "";
            }

            return record.Substring(0, 2);
        }


        // =========================================
        // O1 単勝・複勝オッズ
        // =========================================

        /// <summary>
        /// O1固定長レコードを解析し、RaceID・発表時刻・単勝オッズ・人気をO1Recordへ変換します。

        /// </summary>
        public static O1Record? ParseO1(
            byte[] data)
        {
            if (
                data == null
                || data.Length < 44)
            {
                return null;
            }

            string text =
                JvEncoding.GetString(data);

            if (!text.StartsWith("O1"))
            {
                return null;
            }


            // =====================================
            // データ区分
            // =====================================

            string dataKubunText =
                text.Substring(2, 1);

            int.TryParse(
                dataKubunText,
                out int dataKubun);


            // =====================================
            // RaceID
            //
            // 年      index 11
            // 月日    index 15
            // 場      index 19
            // R       index 25
            // =====================================

            string year =
                text.Substring(11, 4);

            string monthDay =
                text.Substring(15, 4);

            string trackCode =
                text.Substring(19, 2);

            string raceNumber =
                text.Substring(25, 2);


            string raceId =
                year
                + monthDay
                + trackCode
                + raceNumber;


            // =====================================
            // 発表月日時分
            //
            // MMDDHHMM
            // index 27 から8文字
            // =====================================

            string publishedRaw = "";

            if (text.Length >= 35)
            {
                publishedRaw =
                    text.Substring(27, 8)
                        .Trim();
            }


            DateTime? publishedAt =
                ParseO1PublishedTime(
                    year,
                    publishedRaw);


            O1Record record =
                new O1Record
                {
                    RaceId = raceId,
                    DataKubun = dataKubun,
                    PublishedRaw = publishedRaw,
                    PublishedAt = publishedAt
                };


            // =====================================
            // 単勝オッズ
            //
            // 開始 index 43
            //
            // 1頭 8byte
            //
            // 馬番       2
            // 単勝       4
            // 人気       2
            // =====================================

            const int winOddsStart = 43;
            const int blockSize = 8;
            const int maxHorses = 28;


            for (
                int i = 0;
                i < maxHorses;
                i++)
            {
                int start =
                    winOddsStart
                    + (i * blockSize);


                if (
                    start + blockSize
                    > text.Length)
                {
                    break;
                }


                string horseNumberText =
                    text.Substring(
                        start,
                        2)
                    .Trim();


                string oddsText =
                    text.Substring(
                        start + 2,
                        4)
                    .Trim();


                string popularityText =
                    text.Substring(
                        start + 6,
                        2)
                    .Trim();


                // 馬番なし
                if (
                    string.IsNullOrWhiteSpace(
                        horseNumberText))
                {
                    continue;
                }


                // 発売前・取消
                if (
                    string.IsNullOrWhiteSpace(
                        oddsText)
                    || oddsText.Contains("-")
                    || oddsText.Contains("*"))
                {
                    continue;
                }


                if (
                    !int.TryParse(
                        horseNumberText,
                        out int horseNumber))
                {
                    continue;
                }


                if (
                    !int.TryParse(
                        oddsText,
                        out int oddsRaw))
                {
                    continue;
                }


                int popularity = 0;

                int.TryParse(
                    popularityText,
                    out popularity);


                double odds =
                    oddsRaw / 10.0;


                record.WinOdds.Add(
                    new O1WinOdds
                    {
                        HorseNumber =
                            horseNumber,

                        Odds =
                            odds,

                        Popularity =
                            popularity
                    });
            }


            return record;
        }


        // =========================================
        // O1 発表日時
        //
        // year + MMDDHHMM
        // → yyyyMMddHHmm
        // =========================================

        private static DateTime?
            ParseO1PublishedTime(
                string year,
                string publishedRaw)
        {
            if (
                string.IsNullOrWhiteSpace(
                    publishedRaw)
                || publishedRaw.Length != 8)
            {
                return null;
            }


            string dateTimeText =
                year + publishedRaw;


            if (
                DateTime.TryParseExact(
                    dateTimeText,
                    "yyyyMMddHHmm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime result))
            {
                return result;
            }


            return null;
        }


        // =========================================
        // JG
        // =========================================

        /// <summary>
        /// JG固定長レコードを解析し、騎手情報をJgRecordへ変換します。

        /// </summary>
        private static JgRecord ParseJG(
            byte[] data)
        {
            Console.WriteLine();
            Console.WriteLine(
                "===== JGレコード解析 =====");


            JgRecord record =
                new JgRecord();


            record.RecordType =
                GetString(
                    data,
                    0,
                    2);


            record.DataKubun =
                GetString(
                    data,
                    2,
                    1);


            record.DataCreateDate =
                GetString(
                    data,
                    3,
                    8);


            record.Year =
                GetString(
                    data,
                    11,
                    4);


            record.MonthDay =
                GetString(
                    data,
                    15,
                    4);


            record.RacecourseCode =
                GetString(
                    data,
                    19,
                    2);


            record.Kai =
                GetString(
                    data,
                    21,
                    2);


            record.Day =
                GetString(
                    data,
                    23,
                    2);


            record.RaceNumber =
                GetString(
                    data,
                    25,
                    2);


            record.BloodlineNumber =
                GetString(
                    data,
                    27,
                    10);


            record.HorseName =
                GetString(
                    data,
                    37,
                    36)
                .Trim();


            record.EntryOrder =
                GetString(
                    data,
                    73,
                    3);


            record.RunningStatus =
                GetString(
                    data,
                    76,
                    1);


            record.ExclusionStatus =
                GetString(
                    data,
                    77,
                    1);


            Console.WriteLine(
                $"レコード種別ID     : "
                + $"{record.RecordType}");

            Console.WriteLine(
                $"データ区分         : "
                + $"{record.DataKubun}");

            Console.WriteLine(
                $"データ作成年月日   : "
                + $"{record.DataCreateDate}");

            Console.WriteLine(
                $"開催年             : "
                + $"{record.Year}");

            Console.WriteLine(
                $"開催月日           : "
                + $"{record.MonthDay}");

            Console.WriteLine(
                $"競馬場コード       : "
                + $"{record.RacecourseCode}");

            Console.WriteLine(
                $"開催回             : "
                + $"{record.Kai}");

            Console.WriteLine(
                $"開催日             : "
                + $"{record.Day}");

            Console.WriteLine(
                $"レース番号         : "
                + $"{record.RaceNumber}");

            Console.WriteLine(
                $"血統登録番号       : "
                + $"{record.BloodlineNumber}");

            Console.WriteLine(
                $"馬名               : "
                + $"{record.HorseName}");

            Console.WriteLine(
                $"出馬投票受付順番   : "
                + $"{record.EntryOrder}");

            Console.WriteLine(
                $"出走区分           : "
                + $"{record.RunningStatus}");

            Console.WriteLine(
                $"除外状態区分       : "
                + $"{record.ExclusionStatus}");

            Console.WriteLine(
                "==========================");


            return record;
        }


        // =========================================
        // RA
        // =========================================

        /// <summary>
        /// RA固定長レコードを解析し、レース基本情報をRaRecordへ変換します。

        /// </summary>
        public static RaRecord ParseRA(
            byte[] data)
        {
            RaRecord record =
                new RaRecord();


            record.RecordType =
                GetString(data, 0, 2);

            record.DataKubun =
                GetString(data, 2, 1);

            record.DataCreateDate =
                GetString(data, 3, 8);


            record.Year =
                GetString(data, 11, 4);

            record.MonthDay =
                GetString(data, 15, 4);

            record.RacecourseCode =
                GetString(data, 19, 2);

            record.Kai =
                GetString(data, 21, 2);

            record.Day =
                GetString(data, 23, 2);

            record.RaceNumber =
                GetString(data, 25, 2);


            record.RaceNameMain =
                GetString(
                    data,
                    32,
                    60)
                .Trim();


            record.GradeCode =
                GetString(
                    data,
                    614,
                    1);


            record.RaceTypeCode =
                GetString(
                    data,
                    616,
                    2);


            record.Distance =
                GetString(
                    data,
                    697,
                    4);


            record.TrackCode =
                GetString(
                    data,
                    705,
                    2);


            record.StartTime =
                GetString(
                    data,
                    873,
                    4);


            record.StartersCount =
                GetString(
                    data,
                    883,
                    2);


            record.FinishersCount =
                GetString(
                    data,
                    885,
                    2);


            record.WeatherCode =
                GetString(
                    data,
                    887,
                    1);


            record.TurfConditionCode =
                GetString(
                    data,
                    888,
                    1);


            record.DirtConditionCode =
                GetString(
                    data,
                    889,
                    1);


            return record;
        }


        // =========================================
        // SE
        // =========================================

        /// <summary>
        /// SE固定長レコードを解析し、出走馬・着順・馬体重などをSeRecordへ変換します。

        /// </summary>
        public static SeRecord ParseSE(
            byte[] data)
        {
            SeRecord record =
                new SeRecord();


            record.RecordType =
                GetString(data, 0, 2);

            record.DataKubun =
                GetString(data, 2, 1);

            record.DataCreateDate =
                GetString(data, 3, 8);


            record.Year =
                GetString(data, 11, 4);

            record.MonthDay =
                GetString(data, 15, 4);

            record.RacecourseCode =
                GetString(data, 19, 2);

            record.Kai =
                GetString(data, 21, 2);

            record.Day =
                GetString(data, 23, 2);

            record.RaceNumber =
                GetString(data, 25, 2);


            record.FrameNumber =
                GetString(data, 27, 1);

            record.HorseNumber =
                GetString(data, 28, 2);


            record.BloodlineNumber =
                GetString(data, 30, 10);

            record.HorseName =
                GetString(
                    data,
                    40,
                    36)
                .Trim();


            record.WeightCarried =
                GetString(
                    data,
                    288,
                    3);


            record.JockeyCode =
                GetString(
                    data,
                    296,
                    5);


            record.JockeyName =
                GetString(
                    data,
                    306,
                    8)
                .Trim();


            record.HorseWeight =
                GetString(
                    data,
                    324,
                    3);


            record.HorseWeightSign =
                GetString(
                    data,
                    327,
                    1);


            record.HorseWeightDiff =
                GetString(
                    data,
                    328,
                    3);


            record.FinishOrder =
                GetString(
                    data,
                    334,
                    2);


            record.WinOdds =
                GetString(
                    data,
                    359,
                    4);


            record.WinPopularity =
                GetString(
                    data,
                    363,
                    2);


            return record;
        }


        // =========================================
        // byte[] → CP932文字列
        // =========================================

        /// <summary>
        /// 固定長バイト列から指定位置の文字列をCP932で安全に取り出します。

        /// </summary>
        private static string GetString(
            byte[] data,
            int startIndex,
            int length)
        {
            if (
                startIndex < 0
                || startIndex + length
                    > data.Length)
            {
                return "";
            }


            return JvEncoding.GetString(
                data,
                startIndex,
                length);
        }


        // =========================================
        // バイト表示
        // =========================================

        /// <summary>
        /// 解析確認用に、指定範囲のバイト列と文字表現をコンソールへ表示します。

        /// </summary>
        public static void ShowBytes(
            byte[] data)
        {
            Console.WriteLine();
            Console.WriteLine(
                "----- バイト解析 -----");

            Console.WriteLine(
                $"バイト数 : {data.Length}");


            for (
                int i = 0;
                i < data.Length;
                i++)
            {
                Console.WriteLine(
                    $"{i:D2} : "
                    + $"{data[i]:X2}");
            }


            Console.WriteLine(
                "----------------------");
        }
    }
}