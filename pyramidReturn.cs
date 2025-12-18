using System;
using System.Text;

/// <summary>
/// Reads a size value from standard input and prints an upside-down pyramid of asterisks.
/// Each level decreases in width by two asterisks to keep the shape symmetric.
/// </summary>
public static class PyramidReturn
{
    public static void Main(string[] args)
    {
        if (!int.TryParse(Console.ReadLine(), out int size) || size < 1)
        {
            return;
        }

        Console.Write(GeneratePyramid(size));
    }

    private static string GeneratePyramid(int size)
    {
        var builder = new StringBuilder();

        for (int row = size; row >= 1; row--)
        {
            int stars = row * 2 - 1;
            int spaces = size - row;

            builder.Append(' ', spaces);
            builder.Append('*', stars);
            builder.Append('\n');
        }

        return builder.ToString();
    }
}
