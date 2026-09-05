using System.Globalization;

namespace ImageToIco;

public sealed record CliOptions(
    string InputPath,
    string OutputPath,
    IReadOnlyList<int> Sizes,
    bool RemoveBackground,
    double FuzzPercent,
    string? BackgroundColor,
    bool Overwrite)
{
    public static CliParseResult Parse(string[] args)
    {
        try
        {
            if (args.Length < 2)
                return CliParseResult.Fail("Usage: image-to-ico <input> <output> [options]");

            var input = args[0];
            var output = args[1];
            string? sizesValue = null;
            string? backgroundColor = null;
            var removeBackground = false;
            var overwrite = false;
            var fuzz = 8d;

            for (var i = 2; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--remove-background": removeBackground = true; break;
                    case "--overwrite": overwrite = true; break;
                    case "--sizes": sizesValue = RequireValue(args, ref i, "--sizes"); break;
                    case "--background-color": backgroundColor = RequireValue(args, ref i, "--background-color"); break;
                    case "--fuzz":
                        var value = RequireValue(args, ref i, "--fuzz");
                        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out fuzz) || fuzz is < 0 or > 100)
                            throw new ArgumentException("--fuzz must be a number between 0 and 100.");
                        break;
                    default: throw new ArgumentException($"Unknown option '{args[i]}'.");
                }
            }

            return CliParseResult.Ok(new CliOptions(input, output, IconSizeParser.Parse(sizesValue), removeBackground, fuzz, backgroundColor, overwrite));
        }
        catch (ArgumentException ex)
        {
            return CliParseResult.Fail(ex.Message);
        }
    }

    private static string RequireValue(string[] args, ref int index, string option)
    {
        if (++index >= args.Length) throw new ArgumentException($"{option} requires a value.");
        return args[index];
    }
}

public sealed record CliParseResult(bool Success, CliOptions? Options, string? Error)
{
    public static CliParseResult Ok(CliOptions options) => new(true, options, null);
    public static CliParseResult Fail(string error) => new(false, null, error);
}
