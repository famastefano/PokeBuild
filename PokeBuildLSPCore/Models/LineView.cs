using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeBuildLSPCore.Models;
public class LineView(TextView text, int start, int end, int lineNumber)
{
    private readonly TextView Text = text;
    private readonly int Start = start;
    private readonly int End = end;
    public readonly int Number = lineNumber;
    public ReadOnlySpan<char> View => Text.AsSpan().Slice(Start, End);
}
