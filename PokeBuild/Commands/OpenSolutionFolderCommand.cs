using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell.FileDialog;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

using PokeBuildCore;

using System;
using System.IO;
using System.Reflection;

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

        if (folder is null)
            return;

        List<ModuleConfiguration> configs = [];

        JSchema moduleSchema = await GetModuleSchemaAsync();
        foreach (string file in Directory.GetFiles(folder, "*.module.json", SearchOption.AllDirectories))
        {
            ModuleConfiguration? config = TryParseModule(file, moduleSchema, out string[] errors);
            if (errors.Length != 0)
            {
                logger.LogError($"Module {file} contains configuration errors:\n\t{string.Join("\n\t", errors)}");
            }
            else if (config is null)
            {
                logger.LogError($"Module {file} couldn't be parsed.");
            }
            else
            {
                logger.LogInformation($"Found module {file}.");
                configs.Add(config);
            }
        }
    }

    private ModuleConfiguration? TryParseModule(string file, JSchema schema, out string[] validationErrors)
    {
        using StreamReader s = File.OpenText(file);
        using JSchemaValidatingReader reader = new(new JsonTextReader(s))
        {
            Schema = schema
        };
        IList<string> messages = [];
        reader.ValidationEventHandler += (o, a) => messages.Add(a.Message);
        JsonSerializer serializer = new();
        ModuleConfiguration? module = serializer.Deserialize<ModuleConfiguration>(reader);
        validationErrors = [.. messages];
        return module;
    }

    private async Task<JSchema> GetModuleSchemaAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "PokeBuildExtension.ModuleConfigurationSchema.json";

        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using StreamReader reader = new(stream);
        string result = await reader.ReadToEndAsync();
        return JSchema.Parse(result);
    }
}

