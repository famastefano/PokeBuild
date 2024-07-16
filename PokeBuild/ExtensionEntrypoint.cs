using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;

using PokeBuildExtension.Commands;

using System.Resources;

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

    protected override ResourceManager? ResourceManager => Strings.ResourceManager;

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
