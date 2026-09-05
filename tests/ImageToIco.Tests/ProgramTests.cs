using Xunit;

namespace ImageToIco.Tests;

public class ProgramTests
{
    [Fact]
    public void Main_ReturnsValidationExitCodeForInvalidArguments()
    {
        Assert.Equal(2, Program.Main([]));
    }

    [Fact]
    public void Main_ReturnsNotImplementedExitCodeForValidArguments()
    {
        Assert.Equal(1, Program.Main(["logo.png", "app.ico"]));
    }
}
