using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell.FileDialog;

namespace PokeBuildExtension.Commands;

[VisualStudioContribution]
internal class OpenSolutionFolderCommand : Command
{
    public override CommandConfiguration CommandConfiguration => new("%PokeBuild.Commands.OpenSolutionFolder%")
    {
        Icon = new(ImageMoniker.KnownValues.OpenProjectFolder, IconSettings.IconAndText),
    };

    public override Task InitializeAsync(CancellationToken cancellationToken)
    {
        return base.InitializeAsync(cancellationToken);
    }

    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        var logger = await Logging.OutputWindowLogger.MakeLoggerAsync(context, cancellationToken);

        string? folder = await context.Extensibility.Shell().ShowOpenFolderDialogAsync(new FolderDialogOptions
        {
            Title = "Open PokeEngine Solution"
        }, cancellationToken);

        if (folder is not null)
        {
            logger.LogInformation("Opening solution folder `{Folder}`", folder);
        }
    }
}
