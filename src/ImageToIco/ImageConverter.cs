using ImageMagick;

namespace ImageToIco;

public static class ImageConverter
{
    public static void Convert(CliOptions options)
    {
        if (!File.Exists(options.InputPath))
            throw new FileNotFoundException($"Input file not found: {options.InputPath}", options.InputPath);
        if (File.Exists(options.OutputPath) && !options.Overwrite)
            throw new IOException($"Output file already exists: {options.OutputPath}. Use --overwrite to replace it.");

        var outputDirectory = Path.GetDirectoryName(Path.GetFullPath(options.OutputPath));
        if (!string.IsNullOrEmpty(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        using var source = new MagickImage(options.InputPath);
        source.AutoOrient();
        source.Alpha(AlphaOption.On);
        if (options.RemoveBackground)
            BackgroundRemover.Apply(source, options.FuzzPercent, options.BackgroundColor);

        using var frames = new MagickImageCollection();
        foreach (var size in options.Sizes)
        {
            var frame = source.Clone();
            frame.Resize((uint)size, (uint)size);
            frame.BackgroundColor = MagickColors.Transparent;
            frame.Extent((uint)size, (uint)size, Gravity.Center, MagickColors.Transparent);
            frames.Add(frame);
        }
        frames.Write(options.OutputPath, MagickFormat.Ico);
    }
}
