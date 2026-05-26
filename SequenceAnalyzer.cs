namespace SequenceGapScanner;

public static class SequenceAnalyzer
{
    private const int MaxIndividualMissing = 200;

    private record NumericSegment(int Start, int Length, int Value);

    public static List<FileRecord> AnalyzeGroup(List<FileRecord> files)
    {
        if (files.Count == 0) return files;

        var allSegments = files
            .Select(f => ExtractSegments(Path.GetFileNameWithoutExtension(f.Filename)))
            .ToList();

        int maxSlots = allSegments.Max(s => s.Count);

        if (maxSlots == 0)
        {
            for (int i = 0; i < files.Count; i++)
                files[i].SequenceNumber = i + 1;
            return files;
        }

        int dominant = FindDominantSlot(allSegments, maxSlots);

        for (int i = 0; i < files.Count; i++)
        {
            var segs = allSegments[i];
            files[i].SequenceNumber = dominant < segs.Count ? segs[dominant].Value : 0;
        }

        var paired = files
            .Zip(allSegments, (f, s) => (File: f, Segs: s))
            .OrderBy(p => p.File.SequenceNumber)
            .ToList();

        // One template per distinct extension so missing rows are generated
        // for every file type that belongs to a sequence position.
        var templates = paired
            .GroupBy(p => p.File.Extension, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        var result = new List<FileRecord>();
        for (int i = 0; i < paired.Count; i++)
        {
            result.Add(paired[i].File);
            if (i >= paired.Count - 1) continue;

            int cur     = paired[i].File.SequenceNumber;
            int nxt     = paired[i + 1].File.SequenceNumber;
            int gapSize = nxt - cur - 1;
            if (gapSize <= 0) continue;

            if (gapSize <= MaxIndividualMissing)
            {
                for (int missing = cur + 1; missing < nxt; missing++)
                {
                    foreach (var tmpl in templates)
                        result.Add(BuildMissingRecord(tmpl.File, tmpl.Segs, dominant, missing));
                }
            }
            else
            {
                // Large gap: one summary record to avoid flooding the list
                result.Add(BuildGapSummaryRecord(paired[i].File, cur + 1, nxt - 1));
            }
        }

        return result;
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static int FindDominantSlot(List<List<NumericSegment>> allSegments, int maxSlots)
    {
        int bestSlot = 0, bestDistinct = 0;
        for (int slot = 0; slot < maxSlots; slot++)
        {
            int distinct = allSegments
                .Where(s => slot < s.Count)
                .Select(s => s[slot].Value)
                .Distinct()
                .Count();
            if (distinct > bestDistinct) { bestDistinct = distinct; bestSlot = slot; }
        }
        return bestSlot;
    }

    private static FileRecord BuildMissingRecord(
        FileRecord template, List<NumericSegment> templateSegs,
        int dominantSlot, int missingNumber)
    {
        string filename;
        if (dominantSlot < templateSegs.Count)
        {
            var seg    = templateSegs[dominantSlot];
            var base_  = Path.GetFileNameWithoutExtension(template.Filename);
            var ext    = Path.GetExtension(template.Filename);
            var numStr = missingNumber.ToString().PadLeft(seg.Length, '0');
            filename   = $"[MISSING: {base_[..seg.Start]}{numStr}{base_[(seg.Start + seg.Length)..]}{ext}]";
        }
        else
        {
            filename = $"[MISSING: #{missingNumber}]";
        }

        return new FileRecord
        {
            IsMissing      = true,
            Group          = template.Group,
            Extension      = template.Extension,
            SequenceNumber = missingNumber,
            Filename       = filename,
        };
    }

    private static FileRecord BuildGapSummaryRecord(FileRecord template, int from, int to)
    {
        return new FileRecord
        {
            IsMissing      = true,
            Group          = template.Group,
            Extension      = template.Extension,
            SequenceNumber = from,
            Filename       = $"[MISSING: {to - from + 1} files -- #{from} through #{to}]",
        };
    }

    private static List<NumericSegment> ExtractSegments(string name)
    {
        var segs = new List<NumericSegment>();
        int i = 0;
        while (i < name.Length)
        {
            if (!char.IsDigit(name[i])) { i++; continue; }
            int start = i;
            while (i < name.Length && char.IsDigit(name[i])) i++;
            if (int.TryParse(name[start..i], out int value))
                segs.Add(new NumericSegment(start, i - start, value));
        }
        return segs;
    }
}
