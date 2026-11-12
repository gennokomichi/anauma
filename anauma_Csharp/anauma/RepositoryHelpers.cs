namespace anauma;

internal static class RepositoryHelpers
{
    /// <summary>
    /// 年月日・競馬場コード・レース番号を連結してシステム共通のRaceIDを生成します。
    /// </summary>
    public static string BuildRaceId(
        string year,
        string monthDay,
        string racecourseCode,
        string raceNumber)
    {
        return
            year +
            monthDay +
            racecourseCode +
            raceNumber;
    }

    /// <summary>
    /// 数値文字列をintへ変換します。変換できない場合は0を返します。
    /// </summary>
    public static int ParseInt(
        string value)
    {
        int.TryParse(
            value?.Trim(),
            out int result);

        return result;
    }

    /// <summary>
    /// 整数で保持された10分の1単位の値をdecimalへ変換します。
    /// </summary>
    public static decimal ParseTenths(
        string value)
    {
        if (!decimal.TryParse(
                value?.Trim(),
                out decimal parsed))
        {
            return 0m;
        }

        return parsed / 10m;
    }
}
