using Xunit;

namespace ImageToIco.Tests;

public class IconSizeParserTests
{
    [Fact]
    public void Parse_UsesExpectedDefaults()
    {
        Assert.Equal(new[] { 16, 24, 32, 48, 64, 128, 256 }, IconSizeParser.Parse(null));
    }

    [Fact]
    public void Parse_SortsAndDeduplicatesCustomSizes()
    {
        Assert.Equal(new[] { 16, 32, 256 }, IconSizeParser.Parse("256,32,16,32"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("257")]
    [InlineData("16,nope")]
    public void Parse_RejectsInvalidSizes(string value)
    {
        Assert.Throws<ArgumentException>(() => IconSizeParser.Parse(value));
    }
}
