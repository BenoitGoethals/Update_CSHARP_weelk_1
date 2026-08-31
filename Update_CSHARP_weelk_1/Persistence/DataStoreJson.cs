using System.Text.Json;
using System.Text.Json.Serialization;
using Update_CSHARP_weelk_1.Domain;

namespace Update_CSHARP_weelk_1.Persistence;

/// <summary>
/// JSON-file backed async CRUD store for <see cref="Book"/> records.
/// The collection is read from and written to disk on every operation.
/// </summary>
public sealed class DataStoreJson : IBookDataStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _filePath;

    public DataStoreJson(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(AppContext.BaseDirectory, "db", "books.json");
    }

    public async Task<IReadOnlyList<Book>> ReadAllAsync()
    {
        var books = await LoadAsync();
        return books.AsReadOnly();
    }

    public async Task<Book?> ReadAsync(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            throw new ArgumentException("ISBN must not be null or empty.", nameof(isbn));
        }

        var books = await LoadAsync();
        return books.FirstOrDefault(b => b.Isbn == isbn);
    }

    public async Task CreateAsync(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);

        if (string.IsNullOrWhiteSpace(book.Isbn))
        {
            throw new ArgumentException("Book ISBN must not be null or empty.", nameof(book));
        }

        var books = await LoadAsync();
        if (books.Any(b => b.Isbn == book.Isbn))
        {
            throw new InvalidOperationException($"A book with ISBN '{book.Isbn}' already exists.");
        }

        books.Add(book);
        await SaveAsync(books);
    }

    public async Task UpdateAsync(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);

        if (string.IsNullOrWhiteSpace(book.Isbn))
        {
            throw new ArgumentException("Book ISBN must not be null or empty.", nameof(book));
        }

        var books = await LoadAsync();
        var index = books.FindIndex(b => b.Isbn == book.Isbn);
        if (index < 0)
        {
            throw new InvalidOperationException($"No book with ISBN '{book.Isbn}' was found.");
        }

        books[index] = book;
        await SaveAsync(books);
    }

    public async Task DeleteAsync(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            throw new ArgumentException("ISBN must not be null or empty.", nameof(isbn));
        }

        var books = await LoadAsync();
        var removed = books.RemoveAll(b => b.Isbn == isbn);
        if (removed == 0)
        {
            throw new InvalidOperationException($"No book with ISBN '{isbn}' was found.");
        }

        await SaveAsync(books);
    }

    private async Task<List<Book>> LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(_filePath);
        if (stream.Length == 0)
        {
            return [];
        }

        return await JsonSerializer.DeserializeAsync<List<Book>>(stream, SerializerOptions)
               ?? [];
    }

    private async Task SaveAsync(List<Book> books)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, books, SerializerOptions);
    }
}
