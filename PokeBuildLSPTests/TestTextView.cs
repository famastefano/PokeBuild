using PokeBuildLSPCore.Models;

using System.Text;

using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace PokeBuildLSPTests;

[TestClass]
public class TestTextView
{
    const int BLOCK_SIZE = 8;

    private static TextView CreateView() => new(BLOCK_SIZE);

    private static Random rnd = default!;

    [TestInitialize]
    public void Init()
    {
        const int debugSeed = 0;
        int seed = debugSeed == 0 ? Random.Shared.Next() : debugSeed;
        System.Diagnostics.Debug.WriteLine($"Seed: {seed}");
        Console.WriteLine($"Seed: {seed}");
        rnd = new Random(seed);
    }

    [TestMethod]
    public void EmptyView_HasZeroLength()
    {
        var view = CreateView();
        Assert.AreEqual(view.Length, 0);
    }

    [TestMethod]
    public void EmptyView_HasEmptySpan()
    {
        var view = CreateView();
        var span = view.AsSpan();
        Assert.IsTrue(span.IsEmpty);
    }

    [TestMethod]
    public void ReplacingDocument_MakesLengthEqualToContent()
    {
        string content = RandomString(rnd.Next(1, BLOCK_SIZE * 4));

        var view = CreateView();
        view.ReplaceDocument(content);
        Assert.AreEqual(content.Length, view.Length);
    }

    [TestMethod]
    public void ReplacingDocument_MakesSpanEqualToContent()
    {
        string content = RandomString(rnd.Next(1, BLOCK_SIZE * 4));

        var view = CreateView();
        var actual = new string(view.ReplaceDocument(content).AsSpan());
        Assert.AreEqual(content, actual);
    }

    [TestMethod]
    public void ReplacingDocument_MakesCapacityGrowIfRequired()
    {
        string content = RandomString(rnd.Next(BLOCK_SIZE * 2, BLOCK_SIZE * 4));

        var view = CreateView();
        Assert.IsTrue(view.Capacity <= content.Length);

        view.ReplaceDocument(content);
        Assert.IsTrue(view.Capacity > BLOCK_SIZE);
        Assert.IsTrue(view.Capacity >= content.Length);
    }

    [TestMethod]
    public void ReplacingDocument_LeavesCapacityUnchangedIfPossible()
    {
        const int expectedCapacity = BLOCK_SIZE;
        var view = CreateView();

        for (int i = 0; i < 5; ++i)
        {
            string content = RandomString(expectedCapacity);
            view.ReplaceDocument(content);
            Assert.AreEqual(expectedCapacity, view.Capacity);
        }
    }

    [TestMethod]
    public void Update_WithInvalidDataThrowsException()
    {
        var view = CreateView();
        Assert.ThrowsException<ArgumentNullException>(() => view.UpdateDocument(null, null));
    }

    [TestMethod]
    public void Update_WithNoRangeSimplyReplacesTheDocument()
    {
        var view = CreateView();
        for (int i = 0; i < 5; ++i)
        {
            string content = RandomString(rnd.Next(BLOCK_SIZE, BLOCK_SIZE * 4));
            view.UpdateDocument(content, null);

            string cpy = new(view.AsSpan());
            Assert.AreEqual(content, cpy);
        }
    }

    [TestMethod]
    public void Update_WithNoContentDeletesTheSpecifiedRange()
    {
        string content = RandomString(rnd.Next(BLOCK_SIZE, BLOCK_SIZE * 4));

        var view = CreateView();
        view.ReplaceDocument(content);

        int start = rnd.Next(0, content.Length / 2 - 1);
        int end = rnd.Next(start + 1, content.Length - 1);

        Range r = new(1, start, 1, end);
        view.UpdateDocument(null, r);

        string expected = content[..start] + content[(end + 1)..];
        string actual = new(view.AsSpan());
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void Update_WithBothContentAndRangeReplacesData()
    {
        string content = RandomString(rnd.Next(BLOCK_SIZE, BLOCK_SIZE * 4));

        var view = CreateView();
        view.ReplaceDocument(content);

        int start = rnd.Next(0, content.Length / 2 - 1);
        int end = rnd.Next(start + 1, content.Length - 1);

        string replacedBy = RandomString(end - start);

        Range r = new(1, start, 1, end);
        view.UpdateDocument(replacedBy, r);

        var expectedArr = content.ToCharArray();
        for (int i = start; i < end; ++i)
            expectedArr[i] = replacedBy[i - start];
        string expected = new(expectedArr);
        string actual = new(view.AsSpan());
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void DeleteRange_RangeWithSingleCharacterOnlyRemovesThatCharacter()
    {
        string content = RandomString(rnd.Next(BLOCK_SIZE, BLOCK_SIZE * 4));
        var view = CreateView();
        view.ReplaceDocument(content);
        view.DeleteRange(new(0, 0, 0, 0));
        var span = view.AsSpan();
        Assert.AreEqual(content[1..], new(span));
    }

    [TestMethod]
    public void DeleteRange_RangeThatSpansEntireContentDeletesEntireDocument()
    {
        string content = RandomString(rnd.Next(BLOCK_SIZE, BLOCK_SIZE * 4));
        var view = CreateView();
        view.ReplaceDocument(content);
        view.DeleteRange(new(0, 0, 0, view.Length));
        Assert.IsTrue(view.AsSpan().IsEmpty);
    }

    [TestMethod]
    public void InsertRange_Prepend()
    {
        string content = RandomString(rnd.Next(BLOCK_SIZE, BLOCK_SIZE * 4));
        var view = CreateView();
        view.ReplaceDocument(content);

        string prepended = RandomString(rnd.Next(1, BLOCK_SIZE * 4));

        view.InsertRange(prepended, new(0, 0, 0, 0));

        var expected = prepended + content;
        var actual = new string(view.AsSpan());
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void InsertRange_Append()
    {
        string content = RandomString(rnd.Next(BLOCK_SIZE, BLOCK_SIZE * 4));
        var view = CreateView();
        view.ReplaceDocument(content);

        string appended = RandomString(rnd.Next(1, BLOCK_SIZE * 4));

        view.InsertRange(appended, new(0, view.Length, 0, view.Length));

        var expected = content + appended;
        var actual = new string(view.AsSpan());
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void InsertRange_Middle()
    {
        string content = RandomString(rnd.Next(BLOCK_SIZE, BLOCK_SIZE * 4));
        var view = CreateView();
        view.ReplaceDocument(content);

        string mid = RandomString(rnd.Next(1, BLOCK_SIZE * 4));
        int pos = rnd.Next(1, content.Length - 1);

        view.InsertRange(mid, new(0, pos, 0, pos));

        var expected = content[..pos] + mid + content[pos..];
        var actual = new string(view.AsSpan());
        Assert.AreEqual(expected, actual);
    }

    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[rnd.Next(s.Length)]).ToArray());
    }

    private static string RandomStringWithNewlines(int length, int lines)
    {
        StringBuilder s = new(RandomString(length));
        for (int i = 0; i < lines; ++i)
        {
            s[rnd.Next(s.Length)] = '\n';
        }
        return s.ToString();
    }
}