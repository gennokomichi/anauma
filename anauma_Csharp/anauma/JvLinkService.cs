using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using JVDTLabLib;

namespace anauma;

internal sealed class JvLinkService
{
    // =========================================
    // 速報オッズ リトライ設定
    // =========================================

    private const int RealtimeOddsMaxRetryCount = 3;

    private const int RealtimeOddsRetryWaitMilliseconds = 3000;


    // =========================================
    // 通常RACEデータ取得
    // =========================================

    /// <summary>
    /// JV-Linkから通常のRACEデータを取得し、解析済みレコードをまとめて返します。
    /// </summary>
    public JvReadResult ReadRaceData(
        string dataSpec,
        string fromTime,
        int openOption)
    {
        JvReadResult readResult =
            new JvReadResult();


        JVLinkClass jvLink =
            new JVLinkClass();


        try
        {
            Initialize(
                jvLink);


            Open(
                jvLink,
                dataSpec,
                fromTime,
                openOption);


            ReadRecords(
                jvLink,
                readResult);


            return readResult;
        }
        finally
        {
            try
            {
                jvLink.JVClose();
            }
            catch
            {
                // 終了処理中の例外で
                // 本来の処理結果を上書きしない
            }
        }
    }


    // =========================================
    // 速報単勝オッズ取得
    // =========================================

    /// <summary>
    /// 指定したRaceIDの速報単勝オッズをJV-Linkから取得します。未提供時は空の一覧を返します。
    /// </summary>
    public List<O1Record> ReadRealtimeOdds(
        string raceId)
    {
        List<O1Record> records =
            new List<O1Record>();


        JVLinkClass jvLink =
            new JVLinkClass();


        try
        {
            Initialize(
                jvLink);


            bool opened =
                OpenRealtimeOdds(
                    jvLink,
                    raceId);


            if (!opened)
            {
                return records;
            }


            return ReadRealtimeOddsRecords(
                jvLink);
        }
        finally
        {
            try
            {
                jvLink.JVClose();
            }
            catch
            {
                // 終了処理中の例外は無視
            }
        }
    }


    // =========================================
    // JVInit
    // =========================================

    /// <summary>
    /// JVInitを実行してJV-Linkを利用可能な状態に初期化します。
    /// </summary>
    private static void Initialize(
        JVLinkClass jvLink)
    {
        const string sid =
            "UNKNOWN";


        int initResult =
            jvLink.JVInit(
                sid);


        if (initResult != 0)
        {
            throw new InvalidOperationException(
                $"JVInitに失敗しました。"
                + $"エラーコード: "
                + $"{initResult}");
        }


        Console.WriteLine(
            "JV-Linkの初期化に成功しました。");
    }


    // =========================================
    // 通常データ JVOpen
    // =========================================

    /// <summary>
    /// 通常データ取得用のJVOpenを実行し、読み込み対象ファイルを開きます。
    /// </summary>
    private static void Open(
        JVLinkClass jvLink,
        string dataSpec,
        string fromTime,
        int openOption)
    {
        int readCount = 0;

        int downloadCount = 0;

        string lastTimestamp =
            "";


        Console.WriteLine();

        Console.WriteLine(
            "JVOpenを実行しています...");


        int openResult =
            jvLink.JVOpen(
                dataSpec,
                fromTime,
                openOption,
                ref readCount,
                ref downloadCount,
                out lastTimestamp);


        if (openResult < 0)
        {
            throw new InvalidOperationException(
                $"JVOpenに失敗しました。"
                + $"エラーコード: "
                + $"{openResult}");
        }


        Console.WriteLine();

        Console.WriteLine(
            "JVOpen成功！");


        Console.WriteLine(
            $"読み込み対象ファイル数 : "
            + $"{readCount}");


        Console.WriteLine(
            $"ダウンロード対象ファイル数 : "
            + $"{downloadCount}");


        Console.WriteLine(
            $"最終タイムスタンプ : "
            + $"{lastTimestamp}");
    }


    // =========================================
    // 速報オッズ JVRTOpen
    //
    // -1
    //   速報オッズなし
    //
    // -413
    //   通信エラー
    //   → 3秒待って最大3回まで再試行
    // =========================================

    /// <summary>
    /// 速報オッズ取得用のJVRTOpenを実行します。一時エラー時は一定回数リトライします。
    /// </summary>
    private static bool OpenRealtimeOdds(
        JVLinkClass jvLink,
        string raceId)
    {
        const string dataSpec =
            "0B31";


        Console.WriteLine();

        Console.WriteLine(
            "===== 速報オッズ取得 =====");


        Console.WriteLine(
            $"DataSpec : {dataSpec}");


        Console.WriteLine(
            $"RaceID   : {raceId}");


        // =====================================
        // 最大3回試行
        // =====================================

        for (
            int attempt = 1;
            attempt <= RealtimeOddsMaxRetryCount;
            attempt++)
        {
            Console.WriteLine();

            Console.WriteLine(
                $"JVRTOpenを実行しています..."
                + $" ({attempt}/"
                + $"{RealtimeOddsMaxRetryCount})");


            int openResult =
                jvLink.JVRTOpen(
                    dataSpec,
                    raceId);


            // =================================
            // 成功
            // =================================

            if (openResult == 0)
            {
                Console.WriteLine(
                    "JVRTOpen成功！");


                if (attempt > 1)
                {
                    Console.WriteLine(
                        $"リトライ "
                        + $"{attempt - 1}回後に"
                        + "取得できました。");
                }


                return true;
            }


            // =================================
            // 速報オッズなし
            // =================================

            if (openResult == -1)
            {
                Console.WriteLine(
                    "速報オッズは"
                    + "まだありません。");


                return false;
            }


            // =================================
            // -413
            // 通信エラー
            // =================================

            if (openResult == -413)
            {
                Console.WriteLine(
                    "JVRTOpenで"
                    + "通信エラー(-413)が"
                    + "発生しました。");


                // 最終試行なら終了
                if (
                    attempt
                    >= RealtimeOddsMaxRetryCount)
                {
                    Console.WriteLine(
                        $"最大"
                        + $"{RealtimeOddsMaxRetryCount}"
                        + "回試行しましたが、"
                        + "取得できませんでした。");


                    throw new InvalidOperationException(
                        $"JVRTOpenに失敗しました。"
                        + $"エラーコード: "
                        + $"{openResult}");
                }


                Console.WriteLine(
                    $"{RealtimeOddsRetryWaitMilliseconds / 1000}"
                    + "秒待って"
                    + "再試行します...");


                Thread.Sleep(
                    RealtimeOddsRetryWaitMilliseconds);


                continue;
            }


            // =================================
            // その他のエラー
            // =================================

            throw new InvalidOperationException(
                $"JVRTOpenに失敗しました。"
                + $"エラーコード: "
                + $"{openResult}");
        }


        return false;
    }


    // =========================================
    // 通常データ読み込み
    // =========================================

    /// <summary>
    /// JVRead/JVGetsで通常データを順番に読み込み、レコード種別ごとに解析して結果へ格納します。
    /// </summary>
    private static void ReadRecords(
        JVLinkClass jvLink,
        JvReadResult readResult)
    {
        Console.WriteLine();

        Console.WriteLine(
            "データを読み込みます...");


        Console.WriteLine(
            "---------------------------------");


        int count = 0;

        bool seBytesShown =
            false;


        while (true)
        {
            object objBuff =
                (object)(IntPtr)0;


            string fileName;


            int result =
                jvLink.JVGets(
                    ref objBuff,
                    AppConfig.BufferSize,
                    out fileName);


            // =================================
            // 全データ読込完了
            // =================================

            if (result == 0)
            {
                Console.WriteLine();

                Console.WriteLine(
                    "すべてのデータを"
                    + "読み込みました。");


                break;
            }


            // =================================
            // ファイル切替
            // =================================

            if (result == -1)
            {
                continue;
            }


            // =================================
            // ダウンロード中
            // =================================

            if (result == -3)
            {
                Thread.Sleep(
                    500);


                continue;
            }


            // =================================
            // エラー
            // =================================

            if (result < 0)
            {
                HandleReadError(
                    jvLink,
                    result,
                    fileName);


                break;
            }


            count++;


            byte[] data =
                SafeArrayInterop
                    .CopyToByteArray(
                        objBuff,
                        result);


            string text =
                Encoding
                    .GetEncoding(932)
                    .GetString(
                        data);


            if (count == 1)
            {
                JvRecordParser
                    .ShowBytes(
                        data);
            }


            string recordType =
                JvRecordParser
                    .GetRecordType(
                        text);


            PrintRecordHeader(
                count,
                fileName,
                result,
                recordType);


            // =================================
            // RA
            // =================================

            if (recordType == "RA")
            {
                RaRecord ra =
                    JvRecordParser
                        .ParseRA(
                            data);


                readResult
                    .RaRecords
                    .Add(
                        ra);


                ConsoleReporter
                    .PrintRa(
                        ra);
            }

            // =================================
            // SE
            // =================================

            else if (
                recordType == "SE")
            {
                if (!seBytesShown)
                {
                    Console.WriteLine();

                    Console.WriteLine(
                        "===== SEレコード発見 =====");


                    JvRecordParser
                        .ShowBytes(
                            data);


                    seBytesShown =
                        true;
                }


                SeRecord se =
                    JvRecordParser
                        .ParseSE(
                            data);


                readResult
                    .SeRecords
                    .Add(
                        se);


                ConsoleReporter
                    .PrintSe(
                        se);
            }

            // =================================
            // その他
            // =================================

            else
            {
                Console.WriteLine(
                    $"RA・SEではありません。"
                    + $"{recordType}"
                    + "をスキップします。");


                jvLink.JVSkip();
            }
        }


        readResult.ReadRecordCount =
            count;
    }


    // =========================================
    // O1速報オッズ読み込み
    // =========================================

    private static List<O1Record>
        ReadRealtimeOddsRecords(
            JVLinkClass jvLink)
    {
        Console.WriteLine();

        Console.WriteLine(
            "速報オッズを読み込みます...");


        Console.WriteLine(
            "---------------------------------");


        int count = 0;

        int o1Count = 0;


        List<O1Record> records =
            new List<O1Record>();


        while (true)
        {
            object objBuff =
                (object)(IntPtr)0;


            string fileName;


            int result =
                jvLink.JVGets(
                    ref objBuff,
                    AppConfig.BufferSize,
                    out fileName);


            // =================================
            // 完了
            // =================================

            if (result == 0)
            {
                Console.WriteLine();

                Console.WriteLine(
                    "速報オッズの読み込みが"
                    + "完了しました。");


                break;
            }


            // =================================
            // ファイル切替
            // =================================

            if (result == -1)
            {
                continue;
            }


            // =================================
            // ダウンロード中
            // =================================

            if (result == -3)
            {
                Thread.Sleep(
                    500);


                continue;
            }


            // =================================
            // エラー
            // =================================

            if (result < 0)
            {
                HandleReadError(
                    jvLink,
                    result,
                    fileName);


                break;
            }


            count++;


            byte[] data =
                SafeArrayInterop
                    .CopyToByteArray(
                        objBuff,
                        result);


            string text =
                Encoding
                    .GetEncoding(932)
                    .GetString(
                        data);


            string recordType =
                JvRecordParser
                    .GetRecordType(
                        text);


            PrintRecordHeader(
                count,
                fileName,
                result,
                recordType);


            // =================================
            // O1以外
            // =================================

            if (recordType != "O1")
            {
                Console.WriteLine(
                    $"O1以外のレコードです : "
                    + $"{recordType}");


                continue;
            }


            // =================================
            // O1解析
            // =================================

            O1Record? o1 =
                JvRecordParser
                    .ParseO1(
                        data);


            if (o1 == null)
            {
                Console.WriteLine(
                    "O1解析に失敗しました。");


                continue;
            }


            o1Count++;


            records.Add(
                o1);


            // =================================
            // O1表示
            // =================================

            Console.WriteLine();

            Console.WriteLine(
                "===== O1単勝オッズ =====");


            Console.WriteLine(
                $"RaceID     : "
                + $"{o1.RaceId}");


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


            Console.WriteLine();


            foreach (
                O1WinOdds odds
                in o1.WinOdds)
            {
                Console.WriteLine(
                    $"{odds.HorseNumber}番 "
                    + $"単勝 "
                    + $"{odds.Odds:F1}倍 "
                    + $"{odds.Popularity}"
                    + "番人気");
            }
        }


        Console.WriteLine();

        Console.WriteLine(
            "===== 速報オッズ取得結果 =====");


        Console.WriteLine(
            $"取得レコード数 : "
            + $"{count}");


        Console.WriteLine(
            $"O1レコード数   : "
            + $"{o1Count}");


        return records;
    }


    // =========================================
    // O1発表日時表示
    // =========================================

    /// <summary>
    /// 速報オッズの発表日時をログ表示用の文字列へ整形します。
    /// </summary>
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

    /// <summary>
    /// 速報オッズのデータ区分コードを表示用名称へ変換します。
    /// </summary>
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


    // =========================================
    // JVGetsエラー
    // =========================================

    /// <summary>
    /// JV-Link読み込み中に返されたエラーコードを判定し、継続または例外化します。
    /// </summary>
    private static void HandleReadError(
        JVLinkClass jvLink,
        int result,
        string fileName)
    {
        Console.WriteLine();

        Console.WriteLine(
            "===== JVGetsエラー =====");


        Console.WriteLine(
            $"エラーコード : "
            + $"{result}");


        Console.WriteLine(
            $"ファイル名   : "
            + $"{fileName}");


        // =====================================
        // 異常ファイル
        // =====================================

        if (result == -402)
        {
            Console.WriteLine();

            Console.WriteLine(
                "異常ファイルを削除します。");


            jvLink.JVFiledelete(
                fileName);


            Console.WriteLine(
                $"削除対象 : "
                + $"{fileName}");
        }
    }


    // =========================================
    // レコードヘッダー表示
    // =========================================

    /// <summary>
    /// 読み込んだレコードの種別やサイズなど、確認用ヘッダー情報をコンソールへ表示します。
    /// </summary>
    private static void PrintRecordHeader(
        int count,
        string fileName,
        int size,
        string recordType)
    {
        Console.WriteLine();

        Console.WriteLine(
            $"========== データ "
            + $"{count} ==========");


        Console.WriteLine(
            $"ファイル : "
            + $"{fileName}");


        Console.WriteLine(
            $"サイズ   : "
            + $"{size}");


        Console.WriteLine(
            $"レコード種別 : "
            + $"{recordType}");
    }
}