using System;

namespace PokeBuildCore.Exceptions;
public class BuildConfigurationException : Exception
{
    public BuildConfigurationException() : base() { }
    public BuildConfigurationException(string message) : base(message) { }
    public BuildConfigurationException(string message, Exception innerException) : base(message, innerException) { }
}
