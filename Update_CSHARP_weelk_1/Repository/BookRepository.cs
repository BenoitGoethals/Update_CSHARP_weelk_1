using Update_CSHARP_weelk_1.Domain;

namespace Update_CSHARP_weelk_1.Repository;

public sealed class BookRepository : IBookRepository
{
    private readonly IBookDataStore _dataStore;
    private readonly IBookValidator _validator;

    public BookRepository(IBookDataStore dataStore, IBookValidator validator)
    {
        ArgumentNullException.ThrowIfNull(dataStore);
        ArgumentNullException.ThrowIfNull(validator);
        _dataStore = dataStore;
        _validator = validator;
    }

    public async Task<List<Book>> GetAllBooksAsync()
    {
        var books = await _dataStore.ReadAllAsync();
        return books.ToList();
    }

    public Task<Book?> GetBookByIdAsync(string isbn)
    {
        RequireIsbn(isbn);
        return _dataStore.ReadAsync(isbn);
    }

    public Task AddBookAsync(Book book)
    {
        // Repository-level validation: never persist an invalid book, regardless of caller.
        _validator.ThrowIfInvalid(book);
        return _dataStore.CreateAsync(book);
    }

    public Task UpdateBookAsync(Book book)
    {
        // Repository-level validation: never persist an invalid book, regardless of caller.
        _validator.ThrowIfInvalid(book);
        return _dataStore.UpdateAsync(book);
    }

    public Task RemoveBookAsync(string isbn)
    {
        RequireIsbn(isbn);
        return _dataStore.DeleteAsync(isbn);
    }

    private static void RequireIsbn(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            throw new ArgumentException("ISBN must not be null or empty.", nameof(isbn));
        }
    }
}
