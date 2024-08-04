using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System;
using System.Collections.Generic;
using System.Text;

namespace PokeBuildCore;
public class ModuleConfiguration
{
    [JsonRequired]
    public string ModuleName { get; set; }

    public string? Inherits { get; set; }

    public bool IsEngineModule { get; set; } = false;

    [JsonRequired]
    public Configuration[] Configurations { get; set; }
}

public enum Optimizations
{
    Enabled,
    Disabled,
}

public enum Warnings
{
    Auto,
    Disabled,
    Maximum,
}

public enum ModuleType
{
    FileContainer,
    Executable,
    SharedLibrary,
    StaticLibrary,
}

public enum TestType
{
    None,
    Standalone,
    InEngine,
    InGame,
}

public class KeyValue
{
    public string Key { get; set; }
    public string Value { get; set; }
}

public class Configuration
{
    [JsonRequired]
    public string Name { get; set; }

    [JsonRequired]
    [JsonConverter(typeof(StringEnumConverter))]
    public ModuleType Type { get; set; }

    [JsonRequired]
    [JsonConverter(typeof(StringEnumConverter))]
    public Optimizations Optimizations { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public Warnings Warnings { get; set; } = Warnings.Auto;

    [JsonConverter(typeof(StringEnumConverter))]
    public TestType Tests { get; set; } = TestType.None;

    public string[] PublicIncludePathModuleNames { get; set; } = [];
    public string[] PublicDependencyModuleNames { get; set; } = [];
    public string[] PrivateIncludePathModuleNames { get; set; } = [];
    public string[] PrivateDependencyModuleNames { get; set; } = [];

    public KeyValue[] PublicDefinitions { get; set; } = [];
    public KeyValue[] PrivateDefinitions { get; set; } = [];
    public string[] CompilerFlags { get; set; } = [];
    public string[] LinkerFlags { get; set; } = [];
    public string[] PublicForceDisabledWarnings { get; set; } = [];
    public string[] PrivateForceDisabledWarnings { get; set; } = [];
}
