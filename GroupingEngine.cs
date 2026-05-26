namespace SequenceGapScanner;

public static class GroupingEngine
{
    public static Dictionary<string, List<FileRecord>> Group(List<FileRecord> files)
    {
        var groups = new Dictionary<string, List<FileRecord>>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            var key = DetermineGroupKey(file, groups);
            file.Group = key;
            if (!groups.TryGetValue(key, out var list))
            {
                list = new List<FileRecord>();
                groups[key] = list;
            }
            list.Add(file);
        }

        return groups;
    }

    private static string DetermineGroupKey(FileRecord file, Dictionary<string, List<FileRecord>> existing)
    {
        var baseName = Path.GetFileNameWithoutExtension(file.Filename);

        // Rule 1: underscore present with a non-empty prefix before it
        int underscoreIdx = baseName.IndexOf('_');
        if (underscoreIdx > 0)
            return baseName[..underscoreIdx];

        // Rule 2: leading alphabetic characters before the first digit
        var prefix = PrefixBeforeFirstDigit(baseName);
        if (!string.IsNullOrEmpty(prefix))
            return prefix;

        // Rule 3: group by extension, disambiguate if needed
        return ExtensionGroupName(file.Extension, existing);
    }

    private static string PrefixBeforeFirstDigit(string name)
    {
        int i = 0;
        while (i < name.Length && !char.IsDigit(name[i]))
            i++;
        return name[..i];
    }

    private static string ExtensionGroupName(string ext, Dictionary<string, List<FileRecord>> existing)
    {
        var baseName = string.IsNullOrEmpty(ext) ? "Unknown Group" : $"{ext} Group";

        // If the base name already exists and belongs to a real extension group,
        // keep adding to it. Only create a numbered variant when needed (reserved for
        // future batch-splitting logic).
        return baseName;
    }
}
