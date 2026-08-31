namespace Update_CSHARP_weelk_1.Domain;

/// <summary>
/// Default <see cref="IBookValidator"/> enforcing the core book invariants.
/// </summary>
public sealed class BookValidator : IBookValidator
{
    public int MinYear => 1;

    public int MaxYear => DateTime.UtcNow.Year;

    public IReadOnlyList<string> Validate(Book? book)
    {
        if (book is null)
        {
            return ["Book must not be null."];
        }

        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(book.Isbn))
        {
            errors.Add("ISBN is required.");
        }

        if (string.IsNullOrWhiteSpace(book.Title))
        {
            errors.Add("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(book.Author))
        {
            errors.Add("Author is required.");
        }

        if (book.Year < MinYear || book.Year > MaxYear)
        {
            errors.Add($"Year must be between {MinYear} and {MaxYear}.");
        }

        if (!Enum.IsDefined(book.Genre))
        {
            errors.Add("Genre is not a recognized value.");
        }

        return errors;
    }

    public bool IsValid(Book? book) => Validate(book).Count == 0;

    public void ThrowIfInvalid(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);

        var errors = Validate(book);
        if (errors.Count > 0)
        {
            throw new ArgumentException("Invalid book: " + string.Join(" ", errors), nameof(book));
        }
    }
}
