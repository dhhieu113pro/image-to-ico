using ImageMagick;
using Xunit;

namespace ImageToIco.Tests;

public class ImageConverterTests
{
    [Fact]
    public void Convert_CreatesRequestedIcoSizes()
    {
        using var temp = new TempDirectory();
        var input = Path.Combine(temp.Path, "logo.png");
        var output = Path.Combine(temp.Path, "nested", "app.ico");
        using (var image = new MagickImage(MagickColors.Blue, 200, 100)) image.Write(input, MagickFormat.Png);
        ImageConverter.Convert(new CliOptions(input, output, [16, 32, 256], false, 8, null, false));
        Assert.True(File.Exists(output));
        Assert.Equal(new[] { 16, 32, 256 }, ReadIcoSizes(output));
    }

    [Fact]
    public void Convert_RejectsMissingInput() => Assert.Throws<FileNotFoundException>(() =>
        ImageConverter.Convert(new CliOptions("missing.png", "app.ico", [32], false, 8, null, false)));

    [Fact]
    public void Convert_RejectsExistingOutputWithoutOverwrite()
    {
        using var temp = new TempDirectory();
        var input = Path.Combine(temp.Path, "logo.jpg"); var output = Path.Combine(temp.Path, "app.ico");
        using (var image = new MagickImage(MagickColors.Blue, 64, 64)) image.Write(input, MagickFormat.Jpg);
        File.WriteAllText(output, "existing");
        Assert.Throws<IOException>(() => ImageConverter.Convert(new CliOptions(input, output, [32], false, 8, null, false)));
    }

    [Fact]
    public void Convert_OverwritesAndRemovesBackground()
    {
        using var temp = new TempDirectory();
        var input = Path.Combine(temp.Path, "logo.png"); var output = Path.Combine(temp.Path, "app.ico");
        using (var image = new MagickImage(MagickColors.White, 64, 64)) image.Write(input, MagickFormat.Png);
        File.WriteAllText(output, "old");
        ImageConverter.Convert(new CliOptions(input, output, [32], true, 8, "#ffffff", true));
        Assert.Equal(new[] { 32 }, ReadIcoSizes(output));
    }

    private static int[] ReadIcoSizes(string path)
    {
        using var reader = new BinaryReader(File.OpenRead(path));
        Assert.Equal((ushort)0, reader.ReadUInt16()); Assert.Equal((ushort)1, reader.ReadUInt16());
        var count = reader.ReadUInt16(); var sizes = new List<int>();
        for (var i = 0; i < count; i++) { var width = reader.ReadByte(); var height = reader.ReadByte(); sizes.Add(width == 0 ? 256 : width); Assert.Equal(width, height); reader.ReadBytes(14); }
        return sizes.Order().ToArray();
    }

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "image-to-ico-tests-" + Guid.NewGuid().ToString("N"));
        public TempDirectory() => Directory.CreateDirectory(Path);
        public void Dispose() => Directory.Delete(Path, true);
    }
}
