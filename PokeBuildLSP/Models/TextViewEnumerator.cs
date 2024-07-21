using System.Collections;

namespace PokeBuildLSP.Models;

public class TextViewEnumerator(TextView text, int pos = 0) : IEnumerator<LineView>
{
    private TextView? Text = text;
    private readonly int InitialOffset = pos;
    private int Offset = 0;
    private int Length = -1;

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
        return Length != -1;
    }

    public void Reset()
    {
        Offset = InitialOffset;
        Length = -1;
        FindNextLine();
    }

    private void FindNextLine()
    {
        if (Text is null)
        {
            Length = -1;
            return;
        }

        Offset += Length + 1;
        var span = Text.AsSpan();
        if (span.IsEmpty || span.Length <= Offset)
        {
            Length = -1;
            return;
        }

        span = span[Offset..];
        Length = span.IndexOf('\n');
        if (Length == -1)
            Length = span.Length;
    }
}
