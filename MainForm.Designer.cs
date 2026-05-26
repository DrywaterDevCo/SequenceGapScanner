namespace SequenceGapScanner;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    // ── controls ──────────────────────────────────────────────────────────────
    private Panel          topPanel;
    private Label          folderLabel;
    private TextBox        folderPathBox;
    private Button         browseButton;
    private Label          extLabel;
    private TextBox        extFilterBox;
    private Button         scanButton;
    private Button         exportButton;
    private ListView       resultsListView;
    private ColumnHeader   colSeqNum;
    private ColumnHeader   colFilename;
    private ColumnHeader   colGroup;
    private ColumnHeader   colFullPath;
    private ColumnHeader   colCreated;
    private ColumnHeader   colModified;
    private StatusStrip    statusStrip;
    private ToolStripStatusLabel statusLabel;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _headerFont?.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();

        // ── instantiate ──────────────────────────────────────────────────────
        topPanel        = new Panel();
        folderLabel     = new Label();
        folderPathBox   = new TextBox();
        browseButton    = new Button();
        extLabel        = new Label();
        extFilterBox    = new TextBox();
        scanButton      = new Button();
        exportButton    = new Button();
        resultsListView = new ListView();
        colSeqNum       = new ColumnHeader();
        colFilename     = new ColumnHeader();
        colGroup        = new ColumnHeader();
        colFullPath     = new ColumnHeader();
        colCreated      = new ColumnHeader();
        colModified     = new ColumnHeader();
        statusStrip     = new StatusStrip();
        statusLabel     = new ToolStripStatusLabel();

        topPanel.SuspendLayout();
        statusStrip.SuspendLayout();

        // ── topPanel ─────────────────────────────────────────────────────────
        topPanel.Dock      = DockStyle.Top;
        topPanel.Height    = 82;
        topPanel.BackColor = SystemColors.Control;
        topPanel.Padding   = new Padding(6, 6, 6, 6);

        // Row 1 — folder path
        folderLabel.Text      = "Folder:";
        folderLabel.AutoSize  = true;
        folderLabel.Location  = new Point(8, 14);

        folderPathBox.Location = new Point(64, 11);
        folderPathBox.Size     = new Size(1060, 23);
        folderPathBox.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        browseButton.Text     = "Browse…";
        browseButton.Location = new Point(1132, 10);
        browseButton.Size     = new Size(82, 25);
        browseButton.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
        browseButton.Click   += browseButton_Click;

        // Row 2 — extension filter + scan
        extLabel.Text      = "Extensions:";
        extLabel.AutoSize  = true;
        extLabel.Location  = new Point(8, 47);

        extFilterBox.Location        = new Point(105, 44);
        extFilterBox.Size            = new Size(280, 23);
        extFilterBox.PlaceholderText = ".mp4, .mov   -.xml to exclude   (blank = all)";

        scanButton.Text      = "Scan";
        scanButton.Location  = new Point(393, 43);
        scanButton.Size      = new Size(88, 27);
        scanButton.BackColor = Color.FromArgb(0, 120, 215);
        scanButton.ForeColor = Color.White;
        scanButton.FlatStyle = FlatStyle.Flat;
        scanButton.FlatAppearance.BorderSize = 0;
        scanButton.Click    += scanButton_Click;

        exportButton.Text      = "Export…";
        exportButton.Location  = new Point(489, 43);
        exportButton.Size      = new Size(88, 27);
        exportButton.FlatStyle = FlatStyle.Flat;
        exportButton.Enabled   = false;
        exportButton.Click    += exportButton_Click;

        topPanel.Controls.Add(folderLabel);
        topPanel.Controls.Add(folderPathBox);
        topPanel.Controls.Add(browseButton);
        topPanel.Controls.Add(extLabel);
        topPanel.Controls.Add(extFilterBox);
        topPanel.Controls.Add(scanButton);
        topPanel.Controls.Add(exportButton);

        // ── resultsListView ──────────────────────────────────────────────────
        colSeqNum.Text  = "Sequence #";   colSeqNum.Width  = 90;
        colFilename.Text = "Filename";    colFilename.Width = 340;
        colGroup.Text   = "Group";        colGroup.Width    = 160;
        colFullPath.Text = "Full Path";   colFullPath.Width = 380;
        colCreated.Text  = "Created Date";  colCreated.Width  = 160;
        colModified.Text = "Modified Date"; colModified.Width = 160;

        resultsListView.Dock      = DockStyle.Fill;
        resultsListView.View      = View.Details;
        resultsListView.FullRowSelect = true;
        resultsListView.GridLines     = true;
        resultsListView.Font          = new Font("Segoe UI", 9f);
        resultsListView.Columns.AddRange(new[]
        {
            colSeqNum, colFilename, colGroup, colFullPath, colCreated, colModified
        });

        // ── statusStrip ──────────────────────────────────────────────────────
        statusLabel.Text    = "Ready — select a folder and click Scan.";
        statusLabel.Spring  = true;
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;

        statusStrip.Items.Add(statusLabel);
        statusStrip.SizingGrip = true;

        topPanel.ResumeLayout(false);
        statusStrip.ResumeLayout(false);

        // ── form ─────────────────────────────────────────────────────────────
        this.Text          = "Sequence Gap Scanner";
        this.ClientSize    = new Size(1280, 720);
        this.MinimumSize   = new Size(800, 500);
        this.StartPosition = FormStartPosition.CenterScreen;

        // Order: Fill → Bottom → Top (so Top is processed first by layout engine)
        this.Controls.Add(resultsListView);
        this.Controls.Add(statusStrip);
        this.Controls.Add(topPanel);

        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
