namespace SequenceGapScanner;

public partial class MainForm : Form
{
    // Shared bold font for group-header rows; created after InitializeComponent
    // so it inherits the ListView's resolved font.
    private Font _headerFont = null!;

    private static readonly Color HeaderBack  = Color.FromArgb(60,  60,  80);
    private static readonly Color HeaderFore  = Color.White;
    private static readonly Color MissingBack = Color.FromArgb(255, 180, 180);
    private static readonly Color MissingFore = Color.DarkRed;

    // Two alternating base colours for even/odd groups
    private static readonly Color[] GroupPalette =
    {
        Color.FromArgb(245, 245, 255),
        Color.FromArgb(245, 255, 245),
    };

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
        statusLabel.Text = "Scanning…";
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

            int totalGaps = 0;
            var results   = new Dictionary<string, List<FileRecord>>(StringComparer.OrdinalIgnoreCase);

            foreach (var (name, groupFiles) in groups)
            {
                var analyzed = SequenceAnalyzer.AnalyzeGroup(groupFiles);
                results[name] = analyzed;
                totalGaps += analyzed.Count(r => r.IsMissing);
            }

            PopulateListView(results);
            statusLabel.Text =
                $"{groups.Count} group{(groups.Count == 1 ? "" : "s")} found  ·  " +
                $"{files.Count} file{(files.Count == 1 ? "" : "s")}  ·  " +
                $"{totalGaps} gap{(totalGaps == 1 ? "" : "s")} detected";
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

    // ── list-view population ─────────────────────────────────────────────────

    private void PopulateListView(Dictionary<string, List<FileRecord>> allGroups)
    {
        resultsListView.BeginUpdate();
        resultsListView.Items.Clear();

        int groupIndex = 0;
        foreach (var (groupName, records) in allGroups)
        {
            int fileCount = records.Count(r => !r.IsMissing);
            int gapCount  = records.Count(r => r.IsMissing);

            // ── Group header row ──────────────────────────────────────────
            var header = new ListViewItem($"  {groupName}");
            header.SubItems.Add($"{fileCount} file{(fileCount == 1 ? "" : "s")}" +
                                 (gapCount > 0 ? $"  ·  {gapCount} gap{(gapCount == 1 ? "" : "s")}" : ""));
            for (int c = 2; c < resultsListView.Columns.Count; c++)
                header.SubItems.Add("");
            header.BackColor = HeaderBack;
            header.ForeColor = HeaderFore;
            header.Font      = _headerFont;
            resultsListView.Items.Add(header);

            // ── File / missing rows ───────────────────────────────────────
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
