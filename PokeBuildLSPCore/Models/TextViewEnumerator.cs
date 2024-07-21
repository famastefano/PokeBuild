using System.Collections;

namespace PokeBuildLSPCore.Models;

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
        if (Text is not null && Text.Length < Offset)
            throw new InvalidOperationException();
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
        if (span.IsEmpty || span.Length < Offset)
        {
            Length = -1;
        }
        else if (Offset == span.Length)
        {
            Length = 0;
        }
        else
        {
            span = span[Offset..Text.Length];
            Length = span.IndexOf('\n');
            if (Length == -1)
                Length = span.Length;
        }
    }
}
