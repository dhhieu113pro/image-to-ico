using Xunit;

namespace ImageToIco.Tests;

public class CliOptionsTests
{
    [Fact]
    public void Parse_ReadsConversionOptions()
    {
        var result = CliOptions.Parse(new[]
        {
            "logo.png", "app.ico", "--remove-background", "--sizes", "32,256",
            "--fuzz", "12.5", "--background-color", "#ffffff", "--overwrite"
        });

        Assert.True(result.Success);
        Assert.Equal("logo.png", result.Options!.InputPath);
        Assert.Equal("app.ico", result.Options.OutputPath);
        Assert.Equal(new[] { 32, 256 }, result.Options.Sizes);
        Assert.True(result.Options.RemoveBackground);
        Assert.Equal(12.5, result.Options.FuzzPercent);
        Assert.Equal("#ffffff", result.Options.BackgroundColor);
        Assert.True(result.Options.Overwrite);
    }

    [Theory]
    [InlineData(new string[] { })]
    [InlineData(new[] { "logo.png" })]
    [InlineData(new[] { "logo.png", "app.ico", "--fuzz", "-1" })]
    [InlineData(new[] { "logo.png", "app.ico", "--fuzz", "101" })]
    public void Parse_RejectsInvalidArguments(string[] args)
    {
        Assert.False(CliOptions.Parse(args).Success);
    }
}
