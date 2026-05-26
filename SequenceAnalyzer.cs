namespace SequenceGapScanner;

public static class SequenceAnalyzer
{
    // Cap how many individual missing-file rows we generate per gap to avoid
    // flooding the list view when a huge block of files is absent.
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
            // No numbers anywhere — just return in scan order, numbered sequentially
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

        // Pair files with their segment list, then sort by sequence number
        var paired = files
            .Zip(allSegments, (f, s) => (File: f, Segs: s))
            .OrderBy(p => p.File.SequenceNumber)
            .ToList();

        var result = new List<FileRecord>();
        for (int i = 0; i < paired.Count; i++)
        {
            result.Add(paired[i].File);

            if (i >= paired.Count - 1) continue;

            int cur = paired[i].File.SequenceNumber;
            int nxt = paired[i + 1].File.SequenceNumber;
            int gapSize = nxt - cur - 1;

            if (gapSize <= 0) continue;

            if (gapSize <= MaxIndividualMissing)
            {
                for (int missing = cur + 1; missing < nxt; missing++)
                    result.Add(BuildMissingRecord(paired[i].File, paired[i].Segs, dominant, missing));
            }
            else
            {
                // Summarise large gaps with start + end placeholders
                result.Add(BuildMissingRecord(paired[i].File, paired[i].Segs, dominant, cur + 1));
                result.Add(BuildGapSummaryRecord(paired[i].File, cur + 2, nxt - 1));
                result.Add(BuildMissingRecord(paired[i].File, paired[i].Segs, dominant, nxt - 1));
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

            if (distinct > bestDistinct)
            {
                bestDistinct = distinct;
                bestSlot = slot;
            }
        }
        return bestSlot;
    }

    private static FileRecord BuildMissingRecord(
        FileRecord template,
        List<NumericSegment> templateSegs,
        int dominantSlot,
        int missingNumber)
    {
        string filename;
        if (dominantSlot < templateSegs.Count)
        {
            var seg     = templateSegs[dominantSlot];
            var base_   = Path.GetFileNameWithoutExtension(template.Filename);
            var ext     = Path.GetExtension(template.Filename);
            var numStr  = missingNumber.ToString().PadLeft(seg.Length, '0');
            var newBase = base_[..seg.Start] + numStr + base_[(seg.Start + seg.Length)..];
            filename = $"[MISSING: {newBase}{ext}]";
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
            Filename       = $"[MISSING: {to - from + 1} files — #{from} through #{to}]",
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
