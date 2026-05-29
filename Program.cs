namespace SequenceGapScanner;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        var startupFolder = args.Length > 0 && Directory.Exists(args[0]) ? args[0] : null;
        Application.Run(new MainForm(startupFolder));
    }
}
