namespace FakturoNet;

internal static class StartupLog
{
    private static readonly string LogPath = OperatingSystem.IsMacOS() || OperatingSystem.IsLinux()
        ? "/tmp/FakturoNet-startup.log"
        : Path.Combine(Path.GetTempPath(), "FakturoNet-startup.log");

    public static void Write(string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {message}";
        Console.WriteLine(line);

        try
        {
            File.AppendAllText(LogPath, line + Environment.NewLine);
        }
        catch
        {
        }
    }

    public static string GetLogPath() => LogPath;
}
