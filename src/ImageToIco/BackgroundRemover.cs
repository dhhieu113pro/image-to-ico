using ImageMagick;

namespace ImageToIco;

public static class BackgroundRemover
{
    public static void Apply(MagickImage image, double fuzzPercent, string? backgroundColor)
    {
        ArgumentNullException.ThrowIfNull(image);
        image.Alpha(AlphaOption.On);
        image.ColorFuzz = new Percentage(fuzzPercent);

        IMagickColor<byte>? target = string.IsNullOrWhiteSpace(backgroundColor)
            ? null
            : new MagickColor(backgroundColor);

        FloodCorner(image, 0, 0, target);
        FloodCorner(image, (int)image.Width - 1, 0, target);
        FloodCorner(image, 0, (int)image.Height - 1, target);
        FloodCorner(image, (int)image.Width - 1, (int)image.Height - 1, target);
    }

    private static void FloodCorner(MagickImage image, int x, int y, IMagickColor<byte>? target)
    {
        if (target is null)
            image.FloodFill(MagickColors.Transparent, x, y);
        else
            image.FloodFill(MagickColors.Transparent, x, y, target);
    }
}
