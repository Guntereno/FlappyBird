using System;
using System.Diagnostics;

namespace Momo.System;


public enum LogLevel
{
    Verbose = 0,
    Info = 1,
    Warning = 2,
    Error = 3
}

public static class Logger
{
#if DEBUG || LOGGING
    public static LogLevel MinimumLevel { get; set; } = LogLevel.Verbose;
#endif

    [Conditional("DEBUG"), Conditional("LOGGING")]
    public static void Log(FName category, LogLevel level, string message)
    {
#if DEBUG || LOGGING
        if (level < MinimumLevel)
            return;

        var output = $"[{DateTime.Now:HH:mm:ss}] [{level}] [{category}] {message}";
        Debug.WriteLine(output);
        Console.WriteLine(output);
#endif
    }

    [Conditional("DEBUG"), Conditional("LOGGING")]
    public static void Verbose(FName category, string message)
        => Log(category, LogLevel.Verbose, message);

    [Conditional("DEBUG"), Conditional("LOGGING")]
    public static void Info(FName category, string message)
        => Log(category, LogLevel.Info, message);

    [Conditional("DEBUG"), Conditional("LOGGING")]
    public static void Warning(FName category, string message)
        => Log(category, LogLevel.Warning, message);

    [Conditional("DEBUG"), Conditional("LOGGING")]
    public static void Error(FName category, string message)
        => Log(category, LogLevel.Error, message);
}
