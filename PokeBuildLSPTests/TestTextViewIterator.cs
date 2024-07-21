using PokeBuildLSP.Models;

using System.Text;

namespace PokeBuildLSPTests;

[TestClass]
public class TestTextViewIterator
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
    public void EmptyViewHasOneEmptyLine()
    {
        var view = CreateView();

        int iterations = 0;
        foreach (var line in view)
        {
            Assert.IsTrue(line.Line.IsEmpty);
            if (++iterations > 1)
                Assert.Fail();
        }
    }

    [TestMethod]
    public void SingleLineEqualsToContent()
    {
        string content = RandomString(rnd.Next(BLOCK_SIZE, BLOCK_SIZE * 4));

        var view = CreateView();
        view.ReplaceDocument(content);

        int iterations = 0;
        foreach (var line in view)
        {
            string viewString = new(view.AsSpan());
            string lineString = new(line.Line);
            Assert.AreEqual(viewString, lineString);
            ++iterations;
            if (iterations > 1)
                Assert.Fail();
        }
    }

    [TestMethod]
    public void MultipleLinesAreFound()
    {
        int len = rnd.Next(BLOCK_SIZE, BLOCK_SIZE * 4);
        string content = RandomStringWithNewlines(len, rnd.Next(len - 1));

        var view = CreateView();
        view.ReplaceDocument(content);

        string[] lines = content.Split('\n', StringSplitOptions.None);
        int expected = lines.Length;
        int actual = view.AsEnumerable().Count();
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
            int pos = rnd.Next(s.Length);
            while (s[pos] == '\n')
                pos = rnd.Next(s.Length);
            s[pos] = '\n';
        }
        return s.ToString();
    }
}
