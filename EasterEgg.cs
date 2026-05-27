namespace SequenceGapScanner;

internal static class EasterEgg
{
    private static bool _showing = false;

    public static void Show(Form owner)
    {
        if (_showing) return;
        _showing = true;

        var overlay = new NicCageOverlay(owner);
        overlay.FormClosed += (_, _) => _showing = false;
        overlay.Show(owner);
    }
}

internal sealed class NicCageOverlay : Form
{
    private static readonly Random _rng = new();

    private static readonly string[] Facts =
    [
        "Nicolas Cage legally changed his surname from Coppola to avoid nepotism — his uncle is Francis Ford Coppola.",
        "He once bought a pyramid-shaped tomb in New Orleans for himself. It is nine feet tall and inscribed with 'Omni Ab Uno' (Everything From One).",
        "Cage paid $276,000 for a dinosaur skull, only to later discover it had been stolen from Mongolia. He returned it.",
        "He has been to space. Well — he owns two islands, a castle in Germany, and a castle in England, but not space. Close enough.",
        "His acting style is called 'Nouveau Shamanic.' He invented it. Nobody else uses it.",
        "He ate a real cockroach for the film Vampire's Kiss. The director offered a gummy bear. He chose the cockroach.",
        "Cage was so committed to Ghost Rider he wore a black suit covered in Egyptian symbols and voodoo relics on set every day.",
        "He once outbid Leonardo DiCaprio for a 67-million-year-old Tyrannosaurus bataar skull at auction.",
        "He owns a pet octopus. He says it helps his acting. He has not elaborated further.",
        "Cage filed for bankruptcy in 2009 after spending his fortune on castles, islands, yachts, a shrunken pygmy head, and a Lamborghini Miura.",
        "He has a 'Declaration of Independence' — he reportedly memorized the entire thing to prepare for National Treasure.",
        "His son is named Kal-El. That is Superman's birth name. Nicolas Cage is a lifelong Superman superfan.",
        "He appeared in a direct-to-video film under the pseudonym 'John Milton' — the name of the devil in The Devil's Advocate.",
        "A rare Action Comics #1 (first appearance of Superman) was stolen from Cage's collection. It was recovered in a storage unit 11 years later.",
        "In preparation for Birdy, Cage had two teeth surgically removed without anaesthetic to 'feel the pain of war.'",
    ];

    public NicCageOverlay(Form owner)
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition   = FormStartPosition.Manual;
        Bounds          = owner.Bounds;
        BackColor       = Color.FromArgb(15, 15, 25);
        Opacity         = 0.96;
        ShowInTaskbar   = false;
        KeyPreview      = true;

        var title = new Label
        {
            Text      = "🎬  A NICOLAS CAGE FACT  🎬",
            ForeColor = Color.FromArgb(255, 200, 50),
            BackColor = Color.Transparent,
            Font      = new Font("Segoe UI", 16f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize  = false,
            Size      = new Size(620, 40),
        };

        var fact = new Label
        {
            Text      = Facts[_rng.Next(Facts.Length)],
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            Font      = new Font("Segoe UI", 13f, FontStyle.Italic),
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize  = false,
            Size      = new Size(620, 160),
        };

        var source = new Label
        {
            Text      = "Source: AI (so it's probably true)",
            ForeColor = Color.FromArgb(100, 100, 120),
            BackColor = Color.Transparent,
            Font      = new Font("Segoe UI", 8f, FontStyle.Italic),
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize  = false,
            Size      = new Size(620, 18),
        };

        var hint = new Label
        {
            Text      = "click anywhere or press Esc to dismiss",
            ForeColor = Color.FromArgb(100, 100, 120),
            BackColor = Color.Transparent,
            Font      = new Font("Segoe UI", 8f),
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize  = false,
            Size      = new Size(620, 20),
        };

        int cx = owner.ClientSize.Width / 2;
        int cy = owner.ClientSize.Height / 2;

        title.Location  = new Point(cx - title.Width / 2,  cy - 130);
        fact.Location   = new Point(cx - fact.Width / 2,   title.Bottom + 20);
        source.Location = new Point(cx - source.Width / 2, fact.Bottom + 10);
        hint.Location   = new Point(cx - hint.Width / 2,   source.Bottom + 6);

        Controls.AddRange(new Control[] { title, fact, source, hint });

        Click += (_, _) => Close();
        foreach (Control c in Controls)
            c.Click += (_, _) => Close();
        KeyDown += (_, e) => { if (e.KeyCode == Keys.Escape) Close(); };
    }
}
