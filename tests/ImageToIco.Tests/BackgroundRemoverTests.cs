using ImageMagick;
using Xunit;

namespace ImageToIco.Tests;

public class BackgroundRemoverTests
{
    [Fact]
    public void Apply_RejectsNull() => Assert.Throws<ArgumentNullException>(() => BackgroundRemover.Apply(null!, 8, null));

    [Fact]
    public void Apply_MakesInferredCornerBackgroundTransparent()
    {
        using var image = new MagickImage(MagickColors.White, 64, 64);
        BackgroundRemover.Apply(image, 8, null);
        Assert.False(image.IsOpaque);
    }

    [Fact]
    public void Apply_UsesExplicitBackgroundColor()
    {
        using var image = new MagickImage(MagickColors.White, 64, 64);
        BackgroundRemover.Apply(image, 0, "#ffffff");
        Assert.False(image.IsOpaque);
    }

    [Fact]
    public void Apply_TreatsWhitespaceColorAsInference()
    {
        using var image = new MagickImage(MagickColors.White, 64, 64);
        BackgroundRemover.Apply(image, 8, "   ");
        Assert.False(image.IsOpaque);
    }
}
