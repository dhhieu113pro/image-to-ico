using Xunit;

namespace ImageToIco.Tests;

public class CliOptionsTests
{
    public static TheoryData<string[]> InvalidArguments => new()
    {
        Array.Empty<string>(),
        new[] { "logo.png" },
        new[] { "logo.png", "app.ico", "--fuzz", "-1" },
        new[] { "logo.png", "app.ico", "--fuzz", "101" },
        new[] { "logo.png", "app.ico", "--fuzz", "not-a-number" },
        new[] { "logo.png", "app.ico", "--unknown" },
        new[] { "logo.png", "app.ico", "--sizes" },
        new[] { "logo.png", "app.ico", "--fuzz" },
        new[] { "logo.png", "app.ico", "--background-color" },
        new[] { "logo.png", "app.ico", "--sizes", "257" }
    };

    [Fact]
    public void Parse_ReadsConversionOptions()
    {
        var result = CliOptions.Parse(new[]
        {
            "logo.png", "app.ico", "--remove-background", "--sizes", "32,256",
            "--fuzz", "12.5", "--background-color", "#ffffff", "--overwrite"
        });

        Assert.True(result.Success);
        Assert.Null(result.Error);
        Assert.Equal("logo.png", result.Options!.InputPath);
        Assert.Equal("app.ico", result.Options.OutputPath);
        Assert.Equal(new[] { 32, 256 }, result.Options.Sizes);
        Assert.True(result.Options.RemoveBackground);
        Assert.Equal(12.5, result.Options.FuzzPercent);
        Assert.Equal("#ffffff", result.Options.BackgroundColor);
        Assert.True(result.Options.Overwrite);
    }

    [Fact]
    public void Parse_UsesDefaultsWhenNoOptionsAreProvided()
    {
        var result = CliOptions.Parse(new[] { "logo.png", "app.ico" });

        Assert.True(result.Success);
        Assert.Equal(IconSizeParser.DefaultSizes, result.Options!.Sizes);
        Assert.False(result.Options.RemoveBackground);
        Assert.Equal(8d, result.Options.FuzzPercent);
        Assert.Null(result.Options.BackgroundColor);
        Assert.False(result.Options.Overwrite);
    }

    [Theory]
    [MemberData(nameof(InvalidArguments))]
    public void Parse_RejectsInvalidArguments(string[] args)
    {
        var result = CliOptions.Parse(args);

        Assert.False(result.Success);
        Assert.Null(result.Options);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    [Fact]
    public void CliParseResult_FactoriesReturnExpectedShape()
    {
        var options = new CliOptions("in.png", "out.ico", [16], false, 8, null, false);

        var ok = CliParseResult.Ok(options);
        var fail = CliParseResult.Fail("bad input");

        Assert.True(ok.Success);
        Assert.Same(options, ok.Options);
        Assert.Null(ok.Error);
        Assert.False(fail.Success);
        Assert.Null(fail.Options);
        Assert.Equal("bad input", fail.Error);
    }
}
