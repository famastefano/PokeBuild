using PokeBuildLSP.Models;

using System;
using System.Text;

namespace PokeBuildLSPTests;

[TestClass]
public class TestTextView
{
    const int BLOCK_SIZE = 8;

    [TestMethod]
    public void EmptyView_HasZeroLength()
    {
        var view = new TextView(BLOCK_SIZE);
        Assert.AreEqual(view.Length, 0);
    }

    [TestMethod]
    public void EmptyView_HasEmptySpan()
    {
        var view = new TextView(BLOCK_SIZE);
        var span = view.AsSpan();
        Assert.IsTrue(span.IsEmpty);
    }

    [TestMethod]
    public void ReplacingDocument_MakesLengthEqualToContent()
    {
        string content = RandomString(Random.Shared.Next(1, BLOCK_SIZE*4));
        
        var view = new TextView(BLOCK_SIZE);
        view.ReplaceDocument(content);
        Assert.AreEqual(content.Length, view.Length);
    }

    [TestMethod]
    public void ReplacingDocument_MakesSpanEqualToContent()
    {
        string content = RandomString(Random.Shared.Next(1, BLOCK_SIZE * 4));

        var view = new TextView(BLOCK_SIZE);
        var actual = new string(view.ReplaceDocument(content).AsSpan());
        Assert.AreEqual(content, actual);
    }

    [TestMethod]
    public void ReplacingDocument_MakesCapacityGrowIfRequired()
    {
        string content = RandomString(Random.Shared.Next(BLOCK_SIZE * 2, BLOCK_SIZE * 4));

        var view = new TextView(BLOCK_SIZE);
        Assert.IsTrue(view.Capacity <= content.Length);

        view.ReplaceDocument(content);
        Assert.IsTrue(view.Capacity > BLOCK_SIZE);
        Assert.IsTrue(view.Capacity >= content.Length);
    }

    [TestMethod]
    public void ReplacingDocument_LeavesCapacityUnchangedIfPossible()
    {
        const int expectedCapacity = BLOCK_SIZE;
        var view = new TextView(BLOCK_SIZE);

        for(int i = 0; i < 5; ++i)
        {
            string content = RandomString(Random.Shared.Next(expectedCapacity));
            view.ReplaceDocument(content);
            Assert.AreEqual(expectedCapacity, view.Capacity);
        }
    }

    // 6.  Update       with invalid data, throws exception
    // 7.  Update       with no range, replaces the document
    // 8.  Update       with no content, deletes data in the range
    // 9.  Update       with both, replaces data in the range
    // 10. DeleteRange  deletes data in the range
    //                   a. Range.len = 0
    //                   b. Range.len = Length
    //                   c. Range.len = X
    // 11. ReplaceRange replaces data in the range
    //                   a. Range.len = 0      -> shall behave like Insert
    //                   b. Range.len = Length -> shall behave like Append
    //                   c. Range.len = X      -> shall behave like Replace
    //                   d. content.len >  range.len
    //                   e. content.len == range.len
    //                   f. content.len <  range.len

    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
    }

    private static string RandomStringWithNewlines(int length, int lines)
    {
        StringBuilder s = new(RandomString(length));
        for(int i = 0; i < lines; ++i)
        {
            s[Random.Shared.Next(s.Length)] = '\n';
        }
        return s.ToString();
    }
}