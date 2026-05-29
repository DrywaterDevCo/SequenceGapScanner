namespace SequenceGapScanner;

public class WhatsNewForm : Form
{
    public WhatsNewForm(IEnumerable<(string Version, string Notes)> entries)
    {
        Text            = "What's New in Sequence Gap Scanner";
        ClientSize      = new Size(520, 440);
        MinimumSize     = new Size(400, 300);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;

        var textBox = new RichTextBox
        {
            Dock        = DockStyle.Fill,
            ReadOnly    = true,
            BackColor   = SystemColors.Window,
            BorderStyle = BorderStyle.None,
            Font        = new Font("Segoe UI", 10f),
            ScrollBars  = RichTextBoxScrollBars.Vertical,
        };

        var contentPanel = new Panel
        {
            Dock    = DockStyle.Fill,
            Padding = new Padding(12),
        };
        contentPanel.Controls.Add(textBox);

        var boldEmerald = new Font("Segoe UI", 11f, FontStyle.Bold);
        var normal      = textBox.Font;
        var emerald     = Color.FromArgb(15, 90, 50);

        foreach (var (ver, notes) in entries)
        {
            int start = textBox.TextLength;
            textBox.AppendText($"v{ver}\n");
            textBox.Select(start, textBox.TextLength - start - 1);
            textBox.SelectionFont  = boldEmerald;
            textBox.SelectionColor = emerald;

            textBox.SelectionStart  = textBox.TextLength;
            textBox.SelectionLength = 0;
            textBox.SelectionFont   = normal;
            textBox.SelectionColor  = textBox.ForeColor;
            textBox.AppendText($"{notes}\n\n");
        }

        textBox.SelectionStart = 0;

        var okButton = new Button
        {
            Text         = "Got it!",
            Size         = new Size(90, 30),
            Anchor       = AnchorStyles.Bottom | AnchorStyles.Right,
            DialogResult = DialogResult.OK,
        };

        var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 44 };
        buttonPanel.Controls.Add(okButton);
        okButton.Location = new Point(buttonPanel.Width - okButton.Width - 12, 7);
        buttonPanel.Resize += (_, _) =>
            okButton.Location = new Point(buttonPanel.Width - okButton.Width - 12, 7);

        Controls.Add(contentPanel);
        Controls.Add(buttonPanel);
        AcceptButton = okButton;
    }
}
