using OmniSharp.Extensions.LanguageServer.Server;

using System.Diagnostics;
using System.IO.Pipelines;

using PokeBuildLSPCore.DocumentHandlers;

namespace PokeBuildLSP;

internal class Program
{
    static async Task Main(string[] args)
    {
        LanguageServer srv = LanguageServer.Create(GetOptions());
        await srv.WaitForExit.ConfigureAwait(false);
    }

    private static LanguageServerOptions GetOptions()
    {
        var opt = new LanguageServerOptions()
            .WithInput(PipeReader.Create(Process.GetCurrentProcess().StandardInput.BaseStream))
            .WithOutput(PipeWriter.Create(Process.GetCurrentProcess().StandardOutput.BaseStream))
            .WithDefaultScheduler(System.Reactive.Concurrency.ThreadPoolScheduler.Instance)
            .WithServerInfo(new()
            {
                Name = "PokeBuild Module BuildScript LSP",
                Version = "0.1.0"
            })
            .WithHandler<ModuleDocumentHandler>();

            return opt;
    }
}
