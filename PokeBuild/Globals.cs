using PokeBuildExtension.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeBuildExtension;
internal class Globals
{
    internal class Guids
    {
        internal static readonly Guid PokeBuildPane = Guid.NewGuid();
    }

    internal class Singletons
    {
        internal static IPokeBuildLogger Logger;
    }
}
