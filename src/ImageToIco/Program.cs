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

        Console.Error.WriteLine("Image conversion is not implemented yet.");
        return 1;
    }
}
