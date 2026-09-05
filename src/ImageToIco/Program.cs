namespace ImageToIco;

public static class Program
{
    public static int Main(string[] args)
    {
        var parsed = CliOptions.Parse(args);
        if (!parsed.Success)
        {
            Console.Error.WriteLine(parsed.Error);
            return 2;
        }

        try
        {
            ImageConverter.Convert(parsed.Options!);
            Console.WriteLine($"Created {parsed.Options!.OutputPath}");
            return 0;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or ImageMagick.MagickException)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
