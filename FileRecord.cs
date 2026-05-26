namespace SequenceGapScanner;

public class FileRecord
{
    public string Filename    { get; set; } = "";
    public string FullPath    { get; set; } = "";
    public string Group       { get; set; } = "";
    public string Extension   { get; set; } = "";
    public int    SequenceNumber { get; set; }
    public DateTime CreatedDate  { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool IsMissing { get; set; }
}
