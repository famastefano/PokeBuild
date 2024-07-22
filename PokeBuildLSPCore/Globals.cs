using OmniSharp.Extensions.LanguageServer.Protocol;

using PokeBuildLSPCore.Models;

using System.Collections.Concurrent;

namespace PokeBuildLSPCore;
internal class Globals
{
    internal static readonly ConcurrentDictionary<DocumentUri, TextView> Documents = new(Environment.ProcessorCount, 8);
}
