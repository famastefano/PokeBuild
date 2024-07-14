using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using PokeBuildCore.Logging;

using PokeBuildExtension.Commands;
using PokeBuildExtension.Logging;

namespace PokeBuildExtension;
/// <summary>
/// Extension entrypoint for the VisualStudio.Extensibility extension.
/// </summary>
[VisualStudioContribution]
internal class ExtensionEntrypoint : Extension
{
    /// <inheritdoc />
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        RequiresInProcessHosting = true,
    };

    /// <inheritdoc />
    protected override async void InitializeServices(IServiceCollection serviceCollection)
    {
        try
        {
            base.InitializeServices(serviceCollection);
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (Package.GetGlobalService(typeof(SVsOutputWindow)) is IVsOutputWindow outputWindow)
            {
                var guid = Globals.Guids.PokeBuildPane;
                outputWindow.CreatePane(ref guid, "Poke Build", 1, 0);
                outputWindow.GetPane(guid, out var pane);
                pane.Activate();
                Globals.Singletons.Logger = new PokeBuildLogger(pane, LogLevel.Debug);
            }
        }
        catch (Exception ex)
        {
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
        }
    }

    public static readonly CommandGroupConfiguration TopLevelMenuGroup =
        new()
        {
            Children = [GroupChild.Command<OpenSolutionFolderCommand>()]
        };

    [VisualStudioContribution]
    public static MenuConfiguration MainMenu => new("%PokeBuild.MainMenu.Title%")
    {
        Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Children = [MenuChild.Group(TopLevelMenuGroup)]
    };
}
