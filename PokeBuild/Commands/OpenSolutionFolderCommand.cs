using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.Extensibility.Shell.FileDialog;

using PokeBuildExtension;
using PokeBuildExtension.Logging;

namespace PokeBuildExtension.Commands;
/// <summary>
/// Command1 handler.
/// </summary>
[VisualStudioContribution]
internal class OpenSolutionFolderCommand : Command
{
    private readonly IPokeBuildLogger logger = Globals.Singletons.Logger;

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
        string? folder = await context.Extensibility.Shell().ShowOpenFolderDialogAsync(new FolderDialogOptions
        {
            Title = "Open PokeEngine Solution"
        }, cancellationToken);

        if (folder is not null)
        {

        }
    }
}
