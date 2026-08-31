using Update_CSHARP_weelk_1.Domain;
using Update_CSHARP_weelk_1.Repository;

namespace Update_CSHARP_weelk_1.Presentation;

// Command Pattern Interface
public interface ICommand
{
    Task ExecuteAsync();
    string GetDescription();

    /// <summary>True for a command that should end the interactive session.</summary>
    bool TerminatesSession => false;
}

public sealed class AddBookCommand(IBookRepository repository, IBookValidator validator) : ICommand
{
    public async Task ExecuteAsync()
    {
        BookConsoleView.Header("Add New Book");

        // Field-level input validation: each reader re-prompts until the value is well-typed.
        var isbn = ConsoleInput.ReadRequiredString("Enter ISBN: ");
        var title = ConsoleInput.ReadRequiredString("Enter Title: ");
        var author = ConsoleInput.ReadRequiredString("Enter Author: ");
        var year = ConsoleInput.ReadInt("Enter Year: ", validator.MinYear, validator.MaxYear);
        var genre = ConsoleInput.ReadEnum<Genre>("Enter Genre");
        var isRead = ConsoleInput.ReadYesNo("Already read?");

        var book = new Book
        {
            Isbn = isbn,
            Title = title,
            Author = author,
            Year = year,
            Genre = genre,
            IsRead = isRead
        };

        // Input-level validation gate: if the assembled book is invalid, do NOT pass it to the repo.
        var errors = validator.Validate(book);
        if (errors.Count > 0)
        {
            BookConsoleView.ShowErrors(errors);
            return;
        }

        try
        {
            await repository.AddBookAsync(book);
            BookConsoleView.ShowSuccess("Book added successfully!");
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            BookConsoleView.ShowError(ex.Message);
        }
    }

    public string GetDescription() => "Add a new book";
}

public sealed class UpdateBookCommand(IBookRepository repository, IBookValidator validator) : ICommand
{
    public async Task ExecuteAsync()
    {
        BookConsoleView.Header("Update Book");
        var isbn = ConsoleInput.ReadRequiredString("Enter ISBN of the book to update: ");

        Book? existing;
        try
        {
            existing = await repository.GetBookByIdAsync(isbn);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            BookConsoleView.ShowError(ex.Message);
            return;
        }

        if (existing is null)
        {
            BookConsoleView.ShowError($"No book with ISBN '{isbn}' was found.");
            return;
        }

        Console.WriteLine($"Current: {existing}");
        Console.WriteLine("Enter new values:");

        // ISBN is the identity and stays fixed; all other fields are re-entered and re-validated.
        var title = ConsoleInput.ReadRequiredString("Enter Title: ");
        var author = ConsoleInput.ReadRequiredString("Enter Author: ");
        var year = ConsoleInput.ReadInt("Enter Year: ", validator.MinYear, validator.MaxYear);
        var genre = ConsoleInput.ReadEnum<Genre>("Enter Genre");
        var isRead = ConsoleInput.ReadYesNo("Already read?");

        var updated = new Book
        {
            Isbn = existing.Isbn,
            Title = title,
            Author = author,
            Year = year,
            Genre = genre,
            IsRead = isRead
        };

        // Input-level validation gate: if the assembled book is invalid, do NOT pass it to the repo.
        var errors = validator.Validate(updated);
        if (errors.Count > 0)
        {
            BookConsoleView.ShowErrors(errors);
            return;
        }

        try
        {
            await repository.UpdateBookAsync(updated);
            BookConsoleView.ShowSuccess("Book updated successfully!");
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            BookConsoleView.ShowError(ex.Message);
        }
    }

    public string GetDescription() => "Update an existing book";
}

public sealed class RemoveBookCommand(IBookRepository repository) : ICommand
{
    public async Task ExecuteAsync()
    {
        BookConsoleView.Header("Remove Book");
        var isbn = ConsoleInput.ReadRequiredString("Enter ISBN of the book to remove: ");

        try
        {
            await repository.RemoveBookAsync(isbn);
            BookConsoleView.ShowSuccess("Book removed successfully!");
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            BookConsoleView.ShowError(ex.Message);
        }
    }

    public string GetDescription() => "Remove a book by ISBN";
}

public sealed class ListBooksCommand(IBookRepository repository) : ICommand
{
    public async Task ExecuteAsync()
    {
        BookConsoleView.Header("All Books");
        var books = await repository.GetAllBooksAsync();
        BookConsoleView.ShowBooks(books);
    }

    public string GetDescription() => "List all books";
}

public sealed class ExitCommand : ICommand
{
    public Task ExecuteAsync()
    {
        Console.WriteLine("\nExiting application. Goodbye!");
        return Task.CompletedTask;
    }

    public string GetDescription() => "Exit application";

    public bool TerminatesSession => true;
}
