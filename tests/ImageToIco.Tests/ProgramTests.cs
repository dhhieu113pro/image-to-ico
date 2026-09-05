using ImageMagick;
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

    [Fact]
    public void Main_ReturnsSuccessExitCodeAndCreatesIco()
    {
        var temp = Path.Combine(Path.GetTempPath(), "image-to-ico-program-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            var input = Path.Combine(temp, "logo.png");
            var output = Path.Combine(temp, "app.ico");
            using (var image = new MagickImage(MagickColors.Blue, 64, 64))
                image.Write(input, MagickFormat.Png);

            Assert.Equal(0, Program.Main([input, output, "--sizes", "32"]));
            Assert.True(File.Exists(output));
        }
        finally
        {
            Directory.Delete(temp, true);
        }
    }
}
