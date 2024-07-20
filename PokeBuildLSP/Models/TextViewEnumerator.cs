using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeBuildLSP.Models;

public class TextViewEnumerator(TextView text, int pos = 0) : IEnumerator<LineView>
{
    private TextView? Text = text;
    private readonly int InitialOffset = pos;
    private int Offset = 0;
    private int Length = 0;

    public LineView Current => new(Text!, Offset, Length);
    object IEnumerator.Current => Current;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Text = null;
    }

    public bool MoveNext()
    {
        FindNextLine();
        return Length > 0;
    }

    public void Reset()
    {
        Offset = InitialOffset;
        Length = 0;
        FindNextLine();
    }

    private void FindNextLine()
    {
        if(Text is null)
        {
            Length = 0;
            return;
        }

        var span = Text.AsSpan();
        ++Offset;
        if(span.IsEmpty || span.Length <= Offset)
        {
            Length = 0;
            return;
        }

        span = span[(Offset + 1)..];

        int pos = span.IndexOf('\n');
        Length = pos == -1 ? span.Length - Offset : pos - Offset;
    }
}
