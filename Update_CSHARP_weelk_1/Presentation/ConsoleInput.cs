namespace Update_CSHARP_weelk_1.Presentation;

/// <summary>
/// Reads and type-validates console input, re-prompting until the value is well-formed.
/// Guarantees that only correctly-typed, non-empty values leave this layer.
/// </summary>
public static class ConsoleInput
{
    public static string ReadRequiredString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                return input.Trim();
            }

            Console.WriteLine("  ! A value is required. Please try again.");
        }
    }

    public static int ReadInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (int.TryParse(input, out var value) && value >= min && value <= max)
            {
                return value;
            }

            Console.WriteLine($"  ! Please enter a whole number between {min} and {max}.");
        }
    }

    public static TEnum ReadEnum<TEnum>(string prompt) where TEnum : struct, Enum
    {
        var names = string.Join(", ", Enum.GetNames<TEnum>());
        while (true)
        {
            Console.Write($"{prompt} ({names}): ");
            var input = Console.ReadLine();
            if (Enum.TryParse<TEnum>(input, ignoreCase: true, out var value) && Enum.IsDefined(value))
            {
                return value;
            }

            Console.WriteLine($"  ! Please enter one of: {names}.");
        }
    }

    public static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt} (y/n): ");
            var input = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (input is "y" or "yes")
            {
                return true;
            }

            if (input is "n" or "no")
            {
                return false;
            }

            Console.WriteLine("  ! Please enter y or n.");
        }
    }
}
