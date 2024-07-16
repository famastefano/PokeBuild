using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Extensibility;

using System.IO;

namespace PokeBuildExtension.Logging;
internal class OutputWindowLogger : ILogger
{
    internal static async ValueTask<ILogger> MakeLoggerAsync(IClientContext context, CancellationToken cancellationToken)
    {
        const string id = "PokeBuildOutputWindow_CE738F6D-906B-4219-8339-EF0CA5100362";
        var outputWindow = await context.Extensibility.Views().Output.GetChannelAsync(id, nameof(Strings.OutputWindowDisplayName), cancellationToken);
        return new OutputWindowLogger(outputWindow.Writer);
    }

    private readonly TextWriter Writer;

    private OutputWindowLogger(TextWriter writer)
    {
        Writer = writer;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        string lvl = logLevel.ToString().ToUpper().PadRight(11);
        string msg = formatter(state, exception);
        Writer.WriteLine($"{lvl}: {msg}");
    }
}
