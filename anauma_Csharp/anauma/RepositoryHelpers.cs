namespace anauma;

internal static class RepositoryHelpers
{
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

    public static int ParseInt(
        string value)
    {
        int.TryParse(
            value?.Trim(),
            out int result);

        return result;
    }

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
