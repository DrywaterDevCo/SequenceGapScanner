namespace SequenceGapScanner;

public partial class MainForm : Form
{
    private const string AppVersion   = "1.2.0";
    private const string ReleasesApi  = "https://api.github.com/repos/DrywaterDevCo/SequenceGapScanner/releases/latest";
    private const string TipUrl       = "https://buymeacoffee.com/drywater";

    private string? _updateUrl;

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

    private enum GroupExpand { Collapsed, ErrorsOnly, AllFiles }
    private readonly Dictionary<string, GroupExpand> _groupState = new(StringComparer.OrdinalIgnoreCase);
    private ListViewItem? _rightClickedItem;

    private static readonly string SettingsPath = Path.Combine(
        Application.UserAppDataPath, "settings.txt");

    public MainForm()
    {
        InitializeComponent();
        _headerFont = new Font(resultsListView.Font, FontStyle.Bold);
        this.Text   = $"Sequence Gap Scanner  v{AppVersion}";
        AdjustRows();
        LoadSettings();
        statusLabel.Click += statusLabel_Click;
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await CheckForUpdatesAsync();
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("SequenceGapScanner");
            client.Timeout = TimeSpan.FromSeconds(6);

            var json = await client.GetStringAsync(ReleasesApi);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var tag = doc.RootElement.GetProperty("tag_name").GetString()?.TrimStart('v') ?? "";
            var url = doc.RootElement.GetProperty("html_url").GetString() ?? "";

            if (Version.TryParse(tag, out var latest) &&
                Version.TryParse(AppVersion, out var current) &&
                latest > current)
            {
                _updateUrl = url;
                statusLabel.Text         = $"Update available: v{tag} — click here to download";
                statusLabel.ForeColor    = Color.Yellow;
                statusLabel.IsLink       = true;
            }
        }
        catch
        {
            // No internet or API unavailable — silently ignore
        }
    }

    private void statusLabel_Click(object? sender, EventArgs e)
    {
        if (_updateUrl == null) return;
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName        = _updateUrl,
            UseShellExecute = true,
        });
    }

    private void topPanel_Resize(object? sender, EventArgs e) => AdjustRows();

    private void AdjustRows()
    {
        const int gap = 6;
        const int rightMargin = 8;

        // Row 1 — folder path + Browse
        browseButton.Left   = topPanel.ClientSize.Width - rightMargin - browseButton.Width;
        folderPathBox.Width = browseButton.Left - folderPathBox.Left - gap;

        // Row 2 — extensions + Scan + Export (pinned right)
        exportButton.Left  = topPanel.ClientSize.Width - rightMargin - exportButton.Width;
        scanButton.Left    = exportButton.Left - gap - scanButton.Width;
        extFilterBox.Width = scanButton.Left - extFilterBox.Left - gap;
    }

    private void LoadSettings()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return;
            var lines = File.ReadAllLines(SettingsPath);
            if (lines.Length > 0) folderPathBox.Text = lines[0];
            if (lines.Length > 1) extFilterBox.Text  = lines[1];
        }
        catch { }
    }

    private void SaveSettings()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllLines(SettingsPath,
                new[] { folderPathBox.Text.Trim(), extFilterBox.Text.Trim() });
        }
        catch { }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F5 && scanButton.Enabled)
        {
            scanButton_Click(this, EventArgs.Empty);
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
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
        _groupState.Clear();
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
            SaveSettings();

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

    // ── context menu ─────────────────────────────────────────────────────────

    private void resultsListView_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
            _rightClickedItem = resultsListView.GetItemAt(e.X, e.Y);
    }

    private void resultsListView_DoubleClick(object sender, EventArgs e)
    {
        if (resultsListView.FocusedItem?.Tag is FileRecord { IsMissing: false } rec)
            RevealInExplorer(rec.FullPath);
    }

    private void fileContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_rightClickedItem?.Tag is not FileRecord rec)
        {
            e.Cancel = true;
            return;
        }
        menuRevealInExplorer.Enabled = !rec.IsMissing;
        menuCopyPath.Enabled         = !rec.IsMissing;
        menuCopyExpectedName.Visible  = rec.IsMissing;
    }

    private void menuRevealInExplorer_Click(object sender, EventArgs e)
    {
        if (_rightClickedItem?.Tag is FileRecord { IsMissing: false } rec)
            RevealInExplorer(rec.FullPath);
    }

    private void menuCopyPath_Click(object sender, EventArgs e)
    {
        if (_rightClickedItem?.Tag is FileRecord { IsMissing: false } rec)
            Clipboard.SetText(rec.FullPath);
    }

    private void menuCopyExpectedName_Click(object sender, EventArgs e)
    {
        if (_rightClickedItem?.Tag is FileRecord { IsMissing: true } rec)
            Clipboard.SetText(rec.Filename);
    }

    private static void RevealInExplorer(string filePath) =>
        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");

    private void menuAbout_Click(object sender, EventArgs e)
    {
        MessageBox.Show(
            $"Sequence Gap Scanner\nVersion {AppVersion}\n\n" +
            "Scans folders for missing files in numbered sequences.\n\n" +
            "© 2026 Stephen Pickering | Drywater Dev Co.",
            "About",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void menuGitHub_Click(object sender, EventArgs e) =>
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName        = "https://github.com/DrywaterDevCo/SequenceGapScanner",
            UseShellExecute = true,
        });

    private void menuNicolasCage_Click(object sender, EventArgs e) =>
        EasterEgg.Show(this);

    private void menuTip_Click(object sender, EventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName        = TipUrl,
            UseShellExecute = true,
        });
    }

    // ── list-view population ─────────────────────────────────────────────────

    private void resultsListView_MouseClick(object sender, MouseEventArgs e)
    {
        var item = resultsListView.GetItemAt(e.X, e.Y);
        if (item?.Tag is not string groupName || _lastResults == null) return;
        if (!_lastResults.TryGetValue(groupName, out var records)) return;

        bool hasGaps = records.Any(r => r.IsMissing);
        var current  = _groupState.GetValueOrDefault(groupName, GroupExpand.Collapsed);

        _groupState[groupName] = current switch
        {
            GroupExpand.Collapsed  when hasGaps => GroupExpand.ErrorsOnly,
            GroupExpand.Collapsed               => GroupExpand.AllFiles,
            GroupExpand.ErrorsOnly              => GroupExpand.AllFiles,
            _                                   => GroupExpand.Collapsed,
        };

        PopulateListView(_lastResults);
    }

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
            var state     = _groupState.GetValueOrDefault(groupName, GroupExpand.Collapsed);

            // Group header row
            var arrow = state switch
            {
                GroupExpand.ErrorsOnly => "▼!",
                GroupExpand.AllFiles   => "▼ ",
                _                      => "▶ ",
            };
            var header  = new ListViewItem($"  {arrow}  {groupName}");
            var baseSummary = $"{fileCount} file{(fileCount == 1 ? "" : "s")}" +
                (gapCount > 0
                    ? $"  |  {gapCount} gap{(gapCount == 1 ? "" : "s")}  |  {missCount} missing"
                    : "  |  no gaps");
            var stateSuffix = state == GroupExpand.ErrorsOnly ? "  [errors only — click for all]" : "";
            header.SubItems.Add(baseSummary + stateSuffix);
            for (int c = 2; c < resultsListView.Columns.Count; c++)
                header.SubItems.Add("");
            header.BackColor = gapCount > 0 ? Color.FromArgb(80, 30, 30) : HeaderBack;
            header.ForeColor = HeaderFore;
            header.Font      = _headerFont;
            header.Tag       = groupName;
            resultsListView.Items.Add(header);

            if (state == GroupExpand.Collapsed)
            {
                groupIndex++;
                continue;
            }

            var baseColor = GroupPalette[groupIndex % GroupPalette.Length];
            var altColor  = Color.FromArgb(
                Math.Max(baseColor.R - 12, 0),
                Math.Max(baseColor.G - 12, 0),
                Math.Max(baseColor.B - 12, 0));

            var rowsToShow = state == GroupExpand.ErrorsOnly
                ? records.Where(r => r.IsMissing)
                : records.AsEnumerable();

            int rowParity = 0;
            foreach (var rec in rowsToShow)
            {
                var item = new ListViewItem(rec.SequenceNumber.ToString());
                item.SubItems.Add(rec.Filename);
                item.SubItems.Add(rec.Group);
                item.SubItems.Add(rec.FullPath);
                item.SubItems.Add(rec.IsMissing ? "" : rec.CreatedDate.ToString("yyyy-MM-dd  HH:mm:ss"));
                item.SubItems.Add(rec.IsMissing ? "" : rec.ModifiedDate.ToString("yyyy-MM-dd  HH:mm:ss"));
                item.Tag = rec;

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
