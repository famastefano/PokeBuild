using OmniSharp.Extensions.LanguageServer.Protocol.Models;

using System.Collections;

using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace PokeBuildLSPCore.Models;

public class TextView(int blockSize) : IEnumerable<LineView>
{
    private char[] Content = [];

    public int BlockSize { get; private set; } = blockSize;

    public int Length { get; private set; } = 0;

    public int Capacity => Content.Length;

    public TextView ReplaceDocument(string content)
    {
        if (Capacity < content.Length)
            Content = new char[RoundToNextBlockSize(content.Length)];

        content.AsSpan().CopyTo(Content);
        Length = content.Length;
        return this;
    }

    public TextView UpdateDocument(string? content, Range? range)
    {
        if (content is null && range is null)
            throw new ArgumentNullException(nameof(content) + ", " + nameof(range));

        if (content is null)
            return DeleteRange(range!);

        if (range is null)
            return ReplaceDocument(content!);

        if (range!.Start == range!.End)
            return InsertRange(content!, range!);

        return ReplaceRange(content!, range!);
    }

    public TextView DeleteRange(Range range)
    {
        int from = Map(range.Start);
        int to = Map(range.End);
        DeleteRange(from, to);
        return this;
    }

    public TextView InsertRange(string content, Range range)
    {
        int pos = Map(range.Start);
        int newLength = Length + content.Length;
        if (newLength > Capacity)
        {
            var buf = new char[RoundToNextBlockSize(newLength)];
            Content.CopyTo(buf, 0);
            Content = buf;
        }

        var rightSpan = Content.AsSpan()[pos..Length];
        rightSpan.CopyTo(new Span<char>(Content, pos + content.Length, rightSpan.Length));
        content.CopyTo(0, Content, pos, content.Length);

        Length = newLength;
        return this;
    }

    public TextView ReplaceRange(string content, Range range)
    {
        int from = Map(range.Start);
        int to = Map(range.End);
        int rangeLength = to - from;
        int delta = Math.Abs(rangeLength - content.Length);

        if (content.Length > rangeLength)
        {
            if (content.Length + from > Capacity)
            {
                var buf = new char[RoundToNextBlockSize(content.Length + from)];
                Array.Copy(Content, buf, from);
                content.CopyTo(new Span<char>(buf, from, content.Length));

                int srcOffset = to + 1;
                int dstOffset = from + content.Length + 1;
                int size = Length - srcOffset;
                new ReadOnlySpan<char>(Content, srcOffset, size).CopyTo(new Span<char>(buf, dstOffset, size));
                Content = buf;
            }
            else
            {
                int offset = from + content.Length + 1;
                int size = Length - offset;
                content.CopyTo(new Span<char>(Content, from, content.Length));
                new ReadOnlySpan<char>(Content, offset, size).CopyTo(new Span<char>(Content, offset, size));
            }
        }
        else
        {
            if (content.Length < rangeLength)
                DeleteRange(to - delta, to);
            content.AsSpan().CopyTo(new Span<char>(Content, from, content.Length));
        }
        Length += delta;
        return this;
    }

    public ReadOnlySpan<char> AsSpan() => new(Content, 0, Length);

    private void DeleteRange(int from, int to)
    {
        // Given a range j,k
        // and an array 0-N
        // to delete the range j,k we need to:
        // 1. if |j,k| < N, shift left |j,k| positions from k+1
        // 2. N -= |j,k|
        int deleteLength = to - from + 1;
        int shiftLength = Length - to - 1;
        if (deleteLength < Length)
            Array.Copy(Content, to + 1, Content, from, shiftLength);
        Length -= deleteLength;
        if (Length < 0)
            Length = 0;
    }

    private int Map(Position p)
    {
        int pos = 0;
        while (p.Line > 0)
        {
            ReadOnlySpan<char> span = new(Content, pos, Length - pos);
            pos = span.IndexOf('\n');
            pos += --p.Line > 0 ? 0 : 1;
        }
        return pos + p.Character;
    }

    private Position Map(int pos)
    {
        Position p = new();
        int offset = 0;
        int lastLineOffset = 0;
        while (true)
        {
            ReadOnlySpan<char> span = new(Content, offset, Content.Length - offset);
            offset = span.IndexOf('\n');
            if (offset == -1)
                break;
            lastLineOffset = offset;
            ++p.Line;
        }
        p.Character = pos - lastLineOffset;
        return p;
    }

    private int RoundToNextBlockSize(int length)
        => BlockSize * (int)Math.Ceiling((double)length / BlockSize);

    public IEnumerator<LineView> GetEnumerator()
    {
        return new TextViewEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
