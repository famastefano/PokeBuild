using System.IO;
using System.Collections.Generic;
using PokeBuildCore.Exceptions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PokeBuildCore
{
    public class BuildScriptOptions
    {
        public ILogger Logger { get; set; }
        public string RootDir { get; set; }
        public string ModuleDir { get; set; }
        public BuildScript.Configurations Configuration { get; set; }
        public bool IsEngineModule { get; set; }
    }

    public abstract class BuildScript
    {
        /// <summary>
        /// Which configuration we are going to be built with.
        /// </summary>
        public enum Configurations
        {
            /// <summary>
            /// Full debug support.
            /// <br />
            /// All modules defaults to optimization disabled.
            /// </summary>
            Debug,

            /// <summary>
            /// In-game only debug support.
            /// <br />
            /// Engine modules defaults to optimization enabled.
            /// <br />
            /// Game modules defaults to optimization disabled.
            /// </summary>
            DebugGame,

            /// <summary>
            /// Normal development iteration.
            /// <br />
            /// All modules defaults to optimization enabled.
            /// </summary>
            Development,

            /// <summary>
            /// Building for Release.
            /// <br />
            /// Optimizations are forced for every module.
            /// <br />
            /// No debugging support.
            /// <br />
            /// Tests won't be built.
            /// <br />
            /// Everything will be split into monolithic modules:
            /// <br />
            /// - Engine.dll
            /// <br />
            /// - Game.dll
            /// <br />
            /// - Data.bin
            /// </summary>
            Shipping,
        }

        public enum Optimizations
        {
            Disabled,
            Enabled,
        }

        public enum WarningLevels
        {
            Disabled,
            Default,
            Maximum
        }

        public enum ModuleTypes
        {
            Unset,
            Executable,
            SharedLibrary,
            StaticLibrary
        }

        public enum TestTypes
        {
            /// <summary>
            /// No tests
            /// </summary>
            Unset,

            /// <summary>
            /// Runs in an indipendent executable
            /// </summary>
            Indipendent,

            /// <summary>
            /// Will run when the engine starts
            /// </summary>
            InEngine,
        }

        public readonly Configurations Configuration;

        public Optimizations Optimization;

        public WarningLevels WarningLevel = WarningLevels.Default;

        public ModuleTypes ModuleType = ModuleTypes.Unset;

        public TestTypes TestType = TestTypes.Unset;

        /// <summary>
        /// Absolute path to the Root folder
        /// </summary>
        public readonly string RootDir;

        /// <summary>
        /// Absolute path to the Engine folder
        /// </summary>
        public readonly string EngineDir;

        /// <summary>
        /// Absolute path to the Game folder
        /// </summary>
        public readonly string GameDir;

        /// <summary>
        /// Absolute path to the Data folder
        /// </summary>
        public readonly string DataDir;

        /// <summary>
        /// Absolute path to the current Module folder (where the *.Build.cs resides)
        /// </summary>
        public readonly string ModuleDir;

        public readonly bool IsEngineModule;

        public string ModuleName => Path.GetDirectoryName(ModuleDir);

        public readonly ILogger Logger;

        /// <summary>
        /// List of Modules we need for our public headers, but won't link against.
        /// <br />
        /// Automatically prepopulate include paths of other Modules that depends on us.
        /// </summary>
        public readonly List<string> PublicIncludePathModuleNames = [];

        /// <summary>
        /// List of Modules we need to compile and will link against.
        /// <br />
        /// Automatically prepopulate include paths and link libraries of other Modules that depends on us.
        /// </summary>
        public readonly List<string> PublicDependencyModuleNames = [];

        /// <summary>
        /// List of Modules we need to compile, but won't link against.
        /// </summary>
        public readonly List<string> PrivateIncludePathModuleNames = [];

        /// <summary>
        /// List of Modules we need to compile and link against.
        /// </summary>
        public readonly List<string> PrivateDependencyModuleNames = [];

        /// <summary>
        /// List of preprocessor definitions for our public headers.
        /// <br />
        /// Automatically prepopulate the definitions of other Modules that depends on us.
        /// </summary>
        public readonly Dictionary<string, object> PublicDefinitions = [];

        /// <summary>
        /// List of preprocessor definitions that will remain private.
        /// </summary>
        public readonly Dictionary<string, object> PrivateDefinitions = [];

        /// <summary>
        /// Additional compiler flags you might need.
        /// <br />
        /// Only relevant for <strong>this Module.</strong>
        /// </summary>
        public readonly List<string> CompilerFlags = [];

        /// <summary>
        /// Additional linker flags you might need.
        /// <br />
        /// Only relevant for <strong>this Module.</strong>
        /// </summary>
        public readonly List<string> LinkerFlags = [];

        /// <summary>
        /// List of warnings to forcebly disable when compiling this module.
        /// <br />
        /// Modules that depends on us inherit this property publicly or privately depending how they consumes us.
        /// </summary>
        public readonly List<int> PublicForceDisabledWarnings = [];

        /// <summary>
        /// List of warnings to forcebly disable when compiling this module.
        /// <br />
        /// Only relevant for <strong>this Module.</strong>
        /// </summary>
        public readonly List<int> PrivateForceDisabledWarnings = [];

        public BuildScript(BuildScriptOptions opt)
        {
            Logger = opt.Logger;
            RootDir = opt.RootDir;
            EngineDir = Path.Combine(RootDir, "Engine");
            GameDir = Path.Combine(RootDir, "Game");
            DataDir = Path.Combine(RootDir, "Data");
            ModuleDir = opt.ModuleDir;
            Configuration = opt.Configuration;
            IsEngineModule = opt.IsEngineModule;

            Optimization = Configuration switch
            {
                Configurations.Debug => Optimizations.Disabled,
                Configurations.DebugGame => IsEngineModule ? Optimizations.Enabled : Optimizations.Disabled,
                Configurations.Development or Configurations.Shipping => Optimizations.Enabled,
                _ => throw new BuildConfigurationException($"Invalid configuration {Configuration}"),
            };
        }

        public void Configure()
        {
            Logger.LogInformation("Configuring {Module}", ModuleName);
        }

        public void PostConfigure()
        {
            if (Configuration != Configurations.Shipping && ModuleType == ModuleTypes.SharedLibrary)
                PublicDefinitions.Add("HOT_RELOAD", 1);
        }
    }
}
