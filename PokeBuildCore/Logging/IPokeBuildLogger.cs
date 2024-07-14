using System;

namespace PokeBuildCore.Logging;

public enum LogLevel
{
    None,
    Trace,
    Debug,
    Information,
    Warning,
    Error,
    Fatal
}

public interface IPokeBuildLogger
{
    public void Log(LogLevel level, string message, params object[] args);
    public void Log(LogLevel level, Exception? exception, string message, params object[] args);
}
