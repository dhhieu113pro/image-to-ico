using Xunit;

namespace ImageToIco.Tests;

public class IconSizeParserTests
{
    [Fact]
    public void Parse_UsesExpectedDefaults()
    {
        var sizes = IconSizeParser.Parse(null);

        Assert.Equal(new[] { 16, 24, 32, 48, 64, 128, 256 }, sizes);
        Assert.Same(IconSizeParser.DefaultSizes, sizes);
    }

    [Fact]
    public void Parse_SortsAndDeduplicatesCustomSizes()
    {
        Assert.Equal(new[] { 16, 32, 256 }, IconSizeParser.Parse("256, 32,16,32"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("257")]
    [InlineData("16,nope")]
    [InlineData(",")]
    public void Parse_RejectsInvalidSizes(string value)
    {
        var exception = Assert.Throws<ArgumentException>(() => IconSizeParser.Parse(value));
        Assert.False(string.IsNullOrWhiteSpace(exception.Message));
    }

    [Theory]
    [InlineData("1", 1)]
    [InlineData("256", 256)]
    public void Parse_AcceptsBoundarySizes(string value, int expected)
    {
        Assert.Equal(new[] { expected }, IconSizeParser.Parse(value));
    }
}
