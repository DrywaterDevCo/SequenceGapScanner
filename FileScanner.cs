namespace SequenceGapScanner;

public static class FileScanner
{
    public static List<FileRecord> Scan(string rootPath, IEnumerable<string> extensions)
    {
        var extSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in extensions)
        {
            var normalized = e.Trim();
            if (string.IsNullOrEmpty(normalized)) continue;
            extSet.Add(normalized.StartsWith('.') ? normalized : "." + normalized);
        }
        bool filterByExt = extSet.Count > 0;

        var records = new List<FileRecord>();
        foreach (var filePath in Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories))
        {
            var ext = Path.GetExtension(filePath);
            if (filterByExt && !extSet.Contains(ext))
                continue;

            var info = new FileInfo(filePath);
            records.Add(new FileRecord
            {
                Filename     = info.Name,
                FullPath     = filePath,
                Extension    = ext.TrimStart('.').ToUpperInvariant(),
                CreatedDate  = info.CreationTime,
                ModifiedDate = info.LastWriteTime,
            });
        }
        return records;
    }
}
