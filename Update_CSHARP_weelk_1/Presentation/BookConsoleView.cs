using Update_CSHARP_weelk_1.Domain;

namespace Update_CSHARP_weelk_1.Presentation;

/// <summary>
/// Owns all console rendering for books and status messages, keeping presentation
/// output concerns out of the command/handler logic.
/// </summary>
public static class BookConsoleView
{
    public static void Header(string title) => Console.WriteLine($"\n=== {title} ===");

    public static void ShowBooks(IReadOnlyList<Book> books)
    {
        if (books.Count == 0)
        {
            Console.WriteLine("No books in the repository.");
            return;
        }

        foreach (var book in books)
        {
            Console.WriteLine(book.ToString());
        }
    }

    public static void ShowErrors(IReadOnlyList<string> errors)
    {
        foreach (var error in errors)
        {
            Console.WriteLine($"  ! {error}");
        }
    }

    public static void ShowError(string message) => Console.WriteLine($"Error: {message}");

    public static void ShowSuccess(string message) => Console.WriteLine(message);
}
