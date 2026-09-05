namespace ImageToIco;

public static class IconSizeParser
{
    public static readonly int[] DefaultSizes = [16, 24, 32, 48, 64, 128, 256];

    public static IReadOnlyList<int> Parse(string? value)
    {
        if (value is null)
            return DefaultSizes;
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Icon sizes cannot be empty.");

        var sizes = new SortedSet<int>();
        foreach (var part in value.Split(',', StringSplitOptions.TrimEntries))
        {
            if (!int.TryParse(part, out var size) || size is < 1 or > 256)
                throw new ArgumentException($"Invalid icon size '{part}'. Sizes must be between 1 and 256.");
            sizes.Add(size);
        }

        if (sizes.Count == 0)
            throw new ArgumentException("At least one icon size is required.");
        return sizes.ToArray();
    }
}
