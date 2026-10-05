using System.Xml.Linq;

namespace ToyRobot.ConsoleEndToEndTests;

// Delivery checks (spec 003): TD-02, the test-data set is complete (DEC-31, DEC-37);
// BS-01, the build settings that let the app run where only a later .NET is installed (NFR-03, DEC-37).
public class DeliveryTests
{
    [Theory]
    [InlineData("challenge-example-1")]
    [InlineData("challenge-example-2")]
    [InlineData("challenge-example-3")]
    [InlineData("bom-challenge-example-1")]
    [InlineData("exit-mid-file")]
    [InlineData("empty")]
    public void Test_data_holds_every_input_and_expected_pair(string name)
    {
        var folder = Path.Combine(AppProcess.FindRepositoryRoot(), "testdata");

        Assert.True(File.Exists(Path.Combine(folder, name + ".txt")), $"testdata/{name}.txt is missing");
        Assert.True(File.Exists(Path.Combine(folder, name + ".expected.txt")), $"testdata/{name}.expected.txt is missing");
    }

    [Fact]
    public void Byte_order_mark_test_file_starts_with_the_mark()
    {
        var path = Path.Combine(AppProcess.FindRepositoryRoot(), "testdata", "bom-challenge-example-1.txt");
        Assert.True(File.Exists(path), "testdata/bom-challenge-example-1.txt is missing");

        var bytes = File.ReadAllBytes(path);

        Assert.True(bytes.Length >= 3, "the file is shorter than a byte-order mark");
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes.Take(3).ToArray());
    }

    [Fact]
    public void Application_targets_net8_and_rolls_forward_to_the_latest_major_version()
    {
        var path = Path.Combine(AppProcess.FindRepositoryRoot(), "src", "ToyRobot", "ToyRobot.csproj");
        var project = XDocument.Load(path);

        var targetFramework = project.Descendants("TargetFramework").Single().Value;
        var rollForward = project.Descendants("RollForward").Single().Value;

        Assert.Equal("net8.0", targetFramework);
        Assert.Equal("LatestMajor", rollForward);
    }
}
