namespace ToyRobot.ConsoleEndToEndTests;

// E2E-01: every testdata/<name>.txt with a testdata/<name>.expected.txt gives that console output
// (FR-08, FR-12, NFR-06; DEC-23, DEC-31). With no testdata/ folder or no pair, it passes (research R-05).
public class TestDataTests
{
    private const string ExpectedSuffix = ".expected.txt";

    [Fact]
    public void Every_testdata_input_gives_its_expected_console_output()
    {
        var folder = Path.Combine(AppProcess.FindRepositoryRoot(), "testdata");
        if (!Directory.Exists(folder))
        {
            return;
        }

        var mismatches = new List<string>();
        var inputs = Directory.GetFiles(folder, "*.txt")
            .Where(path => !path.EndsWith(ExpectedSuffix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.Ordinal);

        foreach (var input in inputs)
        {
            var expectedPath = input.Substring(0, input.Length - ".txt".Length) + ExpectedSuffix;
            if (!File.Exists(expectedPath))
            {
                continue;
            }

            var name = Path.GetFileName(input);
            var result = AppProcess.Run(File.ReadAllBytes(input));
            var expectedLines = LineComparer.Lines(File.ReadAllText(expectedPath));
            var actualLines = LineComparer.Lines(result.Stdout);

            if (result.ExitCode != 0)
            {
                mismatches.Add($"{name}: exit code {result.ExitCode}, expected 0");
            }
            else if (!expectedLines.SequenceEqual(actualLines))
            {
                mismatches.Add($"{name}: expected [{string.Join(" | ", expectedLines)}] but got [{string.Join(" | ", actualLines)}]");
            }
        }

        Assert.True(mismatches.Count == 0, "Mismatches: " + string.Join("; ", mismatches));
    }
}
