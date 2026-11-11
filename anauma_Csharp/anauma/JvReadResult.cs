namespace anauma;

internal sealed class JvReadResult
{
    public List<RaRecord> RaRecords { get; } = new();
    public List<SeRecord> SeRecords { get; } = new();

    public int ReadRecordCount { get; set; }
}
