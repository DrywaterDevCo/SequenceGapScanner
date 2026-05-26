namespace SequenceGapScanner;

public static class FileScanner
{
    public static List<FileRecord> Scan(string rootPath, IEnumerable<string> extensions)
    {
        var includeExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var excludeExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var e in extensions)
        {
            var raw = e.Trim();
            if (string.IsNullOrEmpty(raw)) continue;

            if (raw.StartsWith('-'))
            {
                var ext = raw[1..];
                excludeExts.Add(ext.StartsWith('.') ? ext : "." + ext);
            }
            else
            {
                includeExts.Add(raw.StartsWith('.') ? raw : "." + raw);
            }
        }

        var records = new List<FileRecord>();
        foreach (var filePath in Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories))
        {
            var ext = Path.GetExtension(filePath);

            if (excludeExts.Contains(ext)) continue;
            if (includeExts.Count > 0 && !includeExts.Contains(ext)) continue;

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
