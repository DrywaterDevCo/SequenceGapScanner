namespace SequenceGapScanner;

public partial class MainForm : Form
{
    private Font _headerFont = null!;

    private static readonly Color HeaderBack  = Color.FromArgb(60,  60,  80);
    private static readonly Color HeaderFore  = Color.White;
    private static readonly Color MissingBack = Color.FromArgb(255, 180, 180);
    private static readonly Color MissingFore = Color.DarkRed;

    private static readonly Color[] GroupPalette =
    {
        Color.FromArgb(245, 245, 255),
        Color.FromArgb(245, 255, 245),
    };

    // Retained after each scan so Export can write from the same data
    private Dictionary<string, List<FileRecord>>? _lastResults;
    private string _lastFolder     = "";
    private string _lastExtensions = "";

    public MainForm()
    {
        InitializeComponent();
        _headerFont = new Font(resultsListView.Font, FontStyle.Bold);
    }

    // ── event handlers ───────────────────────────────────────────────────────

    private void browseButton_Click(object sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description        = "Select folder to scan",
            UseDescriptionForTitle = true,
            ShowNewFolderButton    = false,
            SelectedPath           = folderPathBox.Text.Trim(),
        };
        if (dlg.ShowDialog() == DialogResult.OK)
            folderPathBox.Text = dlg.SelectedPath;
    }

    private void scanButton_Click(object sender, EventArgs e)
    {
        var folder = folderPathBox.Text.Trim();
        if (!Directory.Exists(folder))
        {
            MessageBox.Show("Please select a valid folder.", "Invalid Folder",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        resultsListView.Items.Clear();
        statusLabel.Text   = "Scanning...";
        scanButton.Enabled = false;
        Application.DoEvents();

        try
        {
            var extInput   = extFilterBox.Text.Trim();
            var extensions = string.IsNullOrEmpty(extInput)
                ? Array.Empty<string>()
                : extInput.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var files  = FileScanner.Scan(folder, extensions);
            var groups = GroupingEngine.Group(files);

            int totalGapPositions = 0;
            int totalMissingFiles = 0;
            var results = new Dictionary<string, List<FileRecord>>(StringComparer.OrdinalIgnoreCase);

            foreach (var (name, groupFiles) in groups)
            {
                var analyzed = SequenceAnalyzer.AnalyzeGroup(groupFiles);
                results[name] = analyzed;

                var missing = analyzed.Where(r => r.IsMissing).ToList();
                totalMissingFiles += missing.Count;
                totalGapPositions += missing.Select(r => r.SequenceNumber).Distinct().Count();
            }

            _lastResults    = results;
            _lastFolder     = folder;
            _lastExtensions = extInput;
            exportButton.Enabled = true;

            PopulateListView(results);

            var gapStr     = $"{totalGapPositions} gap{(totalGapPositions == 1 ? "" : "s")}";
            var missingStr = totalMissingFiles != totalGapPositions
                ? $"  |  {totalMissingFiles} missing file{(totalMissingFiles == 1 ? "" : "s")}"
                : "";
            statusLabel.Text =
                $"{results.Count} group{(results.Count == 1 ? "" : "s")}  |  " +
                $"{files.Count} file{(files.Count == 1 ? "" : "s")}  |  " +
                gapStr + missingStr;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Scan failed: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            statusLabel.Text = "Scan failed.";
        }
        finally
        {
            scanButton.Enabled = true;
        }
    }

    // ── export ───────────────────────────────────────────────────────────────

    private void exportButton_Click(object sender, EventArgs e)
    {
        if (_lastResults == null) return;

        using var dlg = new SaveFileDialog
        {
            Title      = "Export Results",
            Filter     = "Text file (*.txt)|*.txt",
            DefaultExt = "txt",
            FileName   = $"SequenceScan_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            ExportToFile(dlg.FileName, _lastResults);
            statusLabel.Text = $"Exported -> {dlg.FileName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Export failed: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportToFile(string path, Dictionary<string, List<FileRecord>> allGroups)
    {
        int totalFiles        = allGroups.Values.Sum(g => g.Count(r => !r.IsMissing));
        int totalMissingFiles = allGroups.Values.Sum(g => g.Count(r => r.IsMissing));
        int totalGapPositions = allGroups.Values.Sum(g =>
            g.Where(r => r.IsMissing).Select(r => r.SequenceNumber).Distinct().Count());

        // UTF-8 without BOM + ASCII-only characters so the file is readable in any editor
        using var w = new StreamWriter(path, append: false,
            encoding: new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        w.WriteLine("Sequence Gap Scanner -- Export Report");
        w.WriteLine($"Generated  : {DateTime.Now:yyyy-MM-dd  HH:mm:ss}");
        w.WriteLine($"Folder     : {_lastFolder}");
        w.WriteLine($"Extensions : {(string.IsNullOrWhiteSpace(_lastExtensions) ? "(all)" : _lastExtensions)}");
        w.WriteLine($"Summary    : {allGroups.Count} groups  |  {totalFiles} files  |  " +
                    $"{totalGapPositions} gaps  |  {totalMissingFiles} missing files");
        w.WriteLine(new string('=', 80));
        w.WriteLine();

        foreach (var (groupName, records) in allGroups)
        {
            int fileCount = records.Count(r => !r.IsMissing);
            int gapCount  = records.Where(r => r.IsMissing).Select(r => r.SequenceNumber).Distinct().Count();
            int missCount = records.Count(r => r.IsMissing);

            w.WriteLine($"GROUP: {groupName}  ({fileCount} files" +
                        (gapCount > 0 ? $"  |  {gapCount} gap{(gapCount == 1 ? "" : "s")}  |  {missCount} missing" : "") +
                        ")");
            w.WriteLine(new string('-', 80));

            foreach (var rec in records)
            {
                if (rec.IsMissing)
                {
                    w.WriteLine($"  {rec.SequenceNumber,6}  *** MISSING ***  {rec.Filename}");
                }
                else
                {
                    w.WriteLine($"  {rec.SequenceNumber,6}                   {rec.Filename}");
                    w.WriteLine($"          Path   : {rec.FullPath}");
                }
            }
            w.WriteLine();
        }
    }

    // ── list-view population ─────────────────────────────────────────────────

    private void PopulateListView(Dictionary<string, List<FileRecord>> allGroups)
    {
        resultsListView.BeginUpdate();
        resultsListView.Items.Clear();

        int groupIndex = 0;
        foreach (var (groupName, records) in allGroups)
        {
            int fileCount = records.Count(r => !r.IsMissing);
            int gapCount  = records.Where(r => r.IsMissing).Select(r => r.SequenceNumber).Distinct().Count();
            int missCount = records.Count(r => r.IsMissing);

            // Group header row
            var header = new ListViewItem($"  {groupName}");
            header.SubItems.Add(
                $"{fileCount} file{(fileCount == 1 ? "" : "s")}" +
                (gapCount > 0
                    ? $"  |  {gapCount} gap{(gapCount == 1 ? "" : "s")}  |  {missCount} missing"
                    : ""));
            for (int c = 2; c < resultsListView.Columns.Count; c++)
                header.SubItems.Add("");
            header.BackColor = HeaderBack;
            header.ForeColor = HeaderFore;
            header.Font      = _headerFont;
            resultsListView.Items.Add(header);

            var baseColor = GroupPalette[groupIndex % GroupPalette.Length];
            var altColor  = Color.FromArgb(
                Math.Max(baseColor.R - 12, 0),
                Math.Max(baseColor.G - 12, 0),
                Math.Max(baseColor.B - 12, 0));

            int rowParity = 0;
            foreach (var rec in records)
            {
                var item = new ListViewItem(rec.SequenceNumber.ToString());
                item.SubItems.Add(rec.Filename);
                item.SubItems.Add(rec.Group);
                item.SubItems.Add(rec.FullPath);
                item.SubItems.Add(rec.IsMissing ? "" : rec.CreatedDate.ToString("yyyy-MM-dd  HH:mm:ss"));
                item.SubItems.Add(rec.IsMissing ? "" : rec.ModifiedDate.ToString("yyyy-MM-dd  HH:mm:ss"));

                if (rec.IsMissing)
                {
                    item.BackColor = MissingBack;
                    item.ForeColor = MissingFore;
                }
                else
                {
                    item.BackColor = rowParity % 2 == 0 ? baseColor : altColor;
                    item.ForeColor = Color.Black;
                    rowParity++;
                }

                resultsListView.Items.Add(item);
            }

            groupIndex++;
        }

        resultsListView.EndUpdate();
    }
}
