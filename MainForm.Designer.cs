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
    private ContextMenuStrip     fileContextMenu;
    private ToolStripMenuItem    menuRevealInExplorer;
    private ToolStripMenuItem    menuCopyPath;
    private ToolStripSeparator   menuSep;
    private ToolStripMenuItem    menuCopyExpectedName;
    private MenuStrip            mainMenu;
    private ToolStripMenuItem    helpMenuItem;
    private ToolStripMenuItem    menuAbout;
    private ToolStripMenuItem    menuTip;
    private ToolStripMenuItem    menuGitHub;
    private ToolStripMenuItem    menuReadMe;
    private ToolStripSeparator   menuHelpSep;
    private ToolStripMenuItem    menuNicolasCage;
    private ToolStripSeparator   menuShellExtSep;
    private ToolStripMenuItem    menuInstallShellExt;
    private ToolStripMenuItem    menuUninstallShellExt;

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
        exportButton     = new Button();
        resultsListView  = new ListView();
        colSeqNum        = new ColumnHeader();
        colFilename      = new ColumnHeader();
        colGroup         = new ColumnHeader();
        colFullPath      = new ColumnHeader();
        colCreated       = new ColumnHeader();
        colModified      = new ColumnHeader();
        statusStrip      = new StatusStrip();
        statusLabel      = new ToolStripStatusLabel();
        mainMenu         = new MenuStrip();
        helpMenuItem     = new ToolStripMenuItem("Help");
        menuAbout        = new ToolStripMenuItem("About Sequence Gap Scanner");
        menuTip          = new ToolStripMenuItem("Tip the Author ♥");
        menuGitHub       = new ToolStripMenuItem("View on GitHub");
        menuReadMe       = new ToolStripMenuItem("ReadMe");
        menuHelpSep      = new ToolStripSeparator();
        menuNicolasCage       = new ToolStripMenuItem("Nicolas Cage");
        menuShellExtSep       = new ToolStripSeparator();
        menuInstallShellExt   = new ToolStripMenuItem("Install Explorer Right-Click Menu");
        menuUninstallShellExt = new ToolStripMenuItem("Uninstall Explorer Right-Click Menu");
        fileContextMenu      = new ContextMenuStrip();
        menuRevealInExplorer = new ToolStripMenuItem("Reveal in Explorer");
        menuCopyPath         = new ToolStripMenuItem("Copy Full Path");
        menuSep              = new ToolStripSeparator();
        menuCopyExpectedName = new ToolStripMenuItem("Copy Expected Filename");

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
        folderPathBox.Size     = new Size(500, 23);
        folderPathBox.Anchor   = AnchorStyles.Top | AnchorStyles.Left;

        browseButton.Text     = "Browse…";
        browseButton.Location = new Point(572, 10);
        browseButton.Size     = new Size(82, 25);
        browseButton.Anchor   = AnchorStyles.Top | AnchorStyles.Left;
        browseButton.Click   += browseButton_Click;

        // Row 2 — extension filter + scan
        extLabel.Text      = "Extensions:";
        extLabel.AutoSize  = true;
        extLabel.Location  = new Point(8, 47);

        extFilterBox.Location        = new Point(105, 44);
        extFilterBox.Size            = new Size(300, 23);
        extFilterBox.Anchor          = AnchorStyles.Top | AnchorStyles.Left;
        extFilterBox.PlaceholderText = ".mp4, .mov   -.xml to exclude   (blank = all)";

        scanButton.Text      = "Scan  (F5)";
        scanButton.Location  = new Point(419, 43);
        scanButton.Size      = new Size(96, 27);
        scanButton.Anchor    = AnchorStyles.Top | AnchorStyles.Left;
        scanButton.BackColor = Color.FromArgb(0, 120, 215);
        scanButton.ForeColor = Color.White;
        scanButton.FlatStyle = FlatStyle.Flat;
        scanButton.FlatAppearance.BorderSize = 0;
        scanButton.Click    += scanButton_Click;

        exportButton.Text      = "Export…";
        exportButton.Location  = new Point(523, 43);
        exportButton.Size      = new Size(88, 27);
        exportButton.Anchor    = AnchorStyles.Top | AnchorStyles.Left;
        exportButton.FlatStyle = FlatStyle.Flat;
        exportButton.Enabled   = false;
        exportButton.Click    += exportButton_Click;

        // ── context menu ─────────────────────────────────────────────────────
        menuRevealInExplorer.Click += menuRevealInExplorer_Click;
        menuCopyPath.Click         += menuCopyPath_Click;
        menuCopyExpectedName.Click += menuCopyExpectedName_Click;
        fileContextMenu.Items.AddRange(new ToolStripItem[]
        {
            menuRevealInExplorer, menuCopyPath, menuSep, menuCopyExpectedName
        });
        fileContextMenu.Opening += fileContextMenu_Opening;

        topPanel.Controls.Add(folderLabel);
        topPanel.Controls.Add(folderPathBox);
        topPanel.Controls.Add(browseButton);
        topPanel.Resize += topPanel_Resize;
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
        resultsListView.FullRowSelect    = true;
        resultsListView.GridLines        = true;
        resultsListView.ShowItemToolTips = true;
        resultsListView.Font             = new Font("Segoe UI", 9f);
        resultsListView.Columns.AddRange(new[]
        {
            colSeqNum, colFilename, colGroup, colFullPath, colCreated, colModified
        });
        resultsListView.ContextMenuStrip = fileContextMenu;
        resultsListView.MouseClick  += resultsListView_MouseClick;
        resultsListView.MouseDown   += resultsListView_MouseDown;
        resultsListView.DoubleClick += resultsListView_DoubleClick;

        // ── main menu ────────────────────────────────────────────────────────
        menuAbout.Click             += menuAbout_Click;
        menuTip.Click               += menuTip_Click;
        menuGitHub.Click            += menuGitHub_Click;
        menuReadMe.Click            += menuReadMe_Click;
        menuNicolasCage.Click       += menuNicolasCage_Click;
        menuInstallShellExt.Click   += menuInstallShellExt_Click;
        menuUninstallShellExt.Click += menuUninstallShellExt_Click;
        helpMenuItem.DropDownItems.AddRange(new ToolStripItem[]
        {
            menuAbout, menuTip, menuGitHub, menuReadMe, menuHelpSep, menuNicolasCage,
            menuShellExtSep, menuInstallShellExt, menuUninstallShellExt
        });
        mainMenu.Items.Add(helpMenuItem);

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
        this.ClientSize    = new Size(800, 600);
        this.MinimumSize   = new Size(800, 500);
        this.StartPosition = FormStartPosition.CenterScreen;

        // Order: Fill → Bottom → Top → Menu (menu must be last / highest Z so it claims top)
        this.Controls.Add(resultsListView);
        this.Controls.Add(statusStrip);
        this.Controls.Add(topPanel);
        this.Controls.Add(mainMenu);
        this.MainMenuStrip = mainMenu;

        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
