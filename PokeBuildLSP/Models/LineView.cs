using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeBuildLSP.Models;
public class LineView(TextView text, int start, int end)
{
    private readonly TextView Text = text;
    private readonly int Start = start;
    private readonly int End = end;

    public ReadOnlySpan<char> Line => Text.AsSpan().Slice(Start, End);
}
