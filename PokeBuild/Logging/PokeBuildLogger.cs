using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using PokeBuildCore.Logging;

namespace PokeBuildExtension.Logging;
internal class PokeBuildLogger : IPokeBuildLogger
{
    private readonly IVsOutputWindowPane pane;
    private readonly IVsOutputWindowPaneNoPump? paneNoPump;
    private LogLevel currentLevel;

    public PokeBuildLogger(IVsOutputWindowPane outPane, LogLevel defaultLevel)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        pane = Requires.NotNull(outPane, nameof(outPane));
        paneNoPump = outPane as IVsOutputWindowPaneNoPump;
        currentLevel = defaultLevel;
    }

    public bool IsEnabled(LogLevel logLevel) => (int)logLevel <= (int)currentLevel;

    public void Log(LogLevel logLevel, string message, params object[] args)
    {
        Log(logLevel, null, message, args);
    }

    public void Log(LogLevel logLevel, Exception? exception, string message, params object[] args)
    {
        if (!IsEnabled(logLevel))
            return;

        string msg = Format(logLevel, exception, message, args);
        if (ThreadHelper.CheckAccess())
        {
            Log(msg);
        }
        else
        {
            var _ = ThreadHelper.JoinableTaskFactory.RunAsyncAsVsTask(VsTaskRunContext.UIThreadBackgroundPriority, async cancellationToken =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                Log(msg);
                return Task.CompletedTask;
            });
        }
    }

    private void Log(string message)
    {
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
        if (paneNoPump is not null)
            paneNoPump.OutputStringNoPump(message);
        else
            pane.OutputStringThreadSafe(message);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
    }

    private static string Format(LogLevel logLevel, Exception? exception, string message, params object[] args)
    {
        const int lvlPad = 11;
        string lvlStr = logLevel.ToString().ToUpper().PadRight(lvlPad);
        string msg = string.Format(message, args);
        if (exception is not null)
            return $"{lvlStr}: {msg} {exception}\n";
        return $"{lvlStr}: {msg}\n";
    }
}
