namespace ToyRobot.EndToEndTests;

/// <summary>Splits text into lines so expected and actual text compare line by line (DEC-31, NFR-04).</summary>
public static class LineComparer
{
    public static IReadOnlyList<string> Lines(string text)
    {
        if (text.Length == 0)
        {
            return Array.Empty<string>();
        }

        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
        if (lines[^1].Length == 0)
        {
            lines.RemoveAt(lines.Count - 1);
        }

        return lines;
    }
}
