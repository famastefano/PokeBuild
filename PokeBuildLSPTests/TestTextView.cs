using PokeBuildLSP.Models;

using System;
using System.Text;

namespace PokeBuildLSPTests;

[TestClass]
public class TestTextView
{
    const int BLOCK_SIZE = 8;

    [TestMethod]
    public void EmptyViewHasZeroLength()
    {
        var view = new TextView(BLOCK_SIZE);
        Assert.AreEqual(view.Length, 0);
    }

    [TestMethod]
    public void EmptyViewHasEmptySpan()
    {
        var view = new TextView(BLOCK_SIZE);
        var span = view.AsSpan();
        Assert.AreEqual(span.Length, 0);
    }

    [TestMethod]
    public void ReplacingDocumentMakesLengthEqualToContent()
    {
        var view = new TextView(BLOCK_SIZE);
    }

    // 3.  Replacing    content makes the length == content.length
    // 4.  Replacing    content makes span() == content.length
    // 5.  Replacing    content with content.length > Capacity still works
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