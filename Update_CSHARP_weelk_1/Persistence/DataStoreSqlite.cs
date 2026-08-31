using Microsoft.Data.Sqlite;
using Update_CSHARP_weelk_1.Domain;

namespace Update_CSHARP_weelk_1.Persistence;

/// <summary>
/// SQLite backed async CRUD store for <see cref="Book"/> records.
/// </summary>
public sealed class DataStoreSqlite : IBookDataStore
{
    private readonly string _connectionString;
    private bool _initialized;

    public DataStoreSqlite(string? filePath = null)
    {
        var path = filePath ?? Path.Combine(AppContext.BaseDirectory, "db", "books.db");

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = path
        }.ToString();
    }

    public async Task<IReadOnlyList<Book>> ReadAllAsync()
    {
        await using var connection = await OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Isbn, Title, Author, Year, Genre, IsRead FROM Books;";

        var books = new List<Book>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            books.Add(MapBook(reader));
        }

        return books.AsReadOnly();
    }

    public async Task<Book?> ReadAsync(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            throw new ArgumentException("ISBN must not be null or empty.", nameof(isbn));
        }

        await using var connection = await OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText =
            "SELECT Isbn, Title, Author, Year, Genre, IsRead FROM Books WHERE Isbn = $isbn;";
        command.Parameters.AddWithValue("$isbn", isbn);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapBook(reader) : null;
    }

    public async Task CreateAsync(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);

        if (string.IsNullOrWhiteSpace(book.Isbn))
        {
            throw new ArgumentException("Book ISBN must not be null or empty.", nameof(book));
        }

        await using var connection = await OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO Books (Isbn, Title, Author, Year, Genre, IsRead)
            VALUES ($isbn, $title, $author, $year, $genre, $isRead);
            """;
        BindBook(command, book);

        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // SQLITE_CONSTRAINT (PK violation)
        {
            throw new InvalidOperationException($"A book with ISBN '{book.Isbn}' already exists.");
        }
    }

    public async Task UpdateAsync(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);

        if (string.IsNullOrWhiteSpace(book.Isbn))
        {
            throw new ArgumentException("Book ISBN must not be null or empty.", nameof(book));
        }

        await using var connection = await OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE Books
            SET Title = $title, Author = $author, Year = $year, Genre = $genre, IsRead = $isRead
            WHERE Isbn = $isbn;
            """;
        BindBook(command, book);

        var affected = await command.ExecuteNonQueryAsync();
        if (affected == 0)
        {
            throw new InvalidOperationException($"No book with ISBN '{book.Isbn}' was found.");
        }
    }

    public async Task DeleteAsync(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            throw new ArgumentException("ISBN must not be null or empty.", nameof(isbn));
        }

        await using var connection = await OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Books WHERE Isbn = $isbn;";
        command.Parameters.AddWithValue("$isbn", isbn);

        var affected = await command.ExecuteNonQueryAsync();
        if (affected == 0)
        {
            throw new InvalidOperationException($"No book with ISBN '{isbn}' was found.");
        }
    }

    private async Task<SqliteConnection> OpenAsync()
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        if (!_initialized)
        {
            var command = connection.CreateCommand();
            command.CommandText =
                """
                CREATE TABLE IF NOT EXISTS Books (
                    Isbn   TEXT PRIMARY KEY,
                    Title  TEXT NOT NULL,
                    Author TEXT NOT NULL,
                    Year   INTEGER NOT NULL,
                    Genre  TEXT NOT NULL,
                    IsRead INTEGER NOT NULL
                );
                """;
            await command.ExecuteNonQueryAsync();
            _initialized = true;
        }

        return connection;
    }

    private static void BindBook(SqliteCommand command, Book book)
    {
        command.Parameters.AddWithValue("$isbn", book.Isbn);
        command.Parameters.AddWithValue("$title", book.Title);
        command.Parameters.AddWithValue("$author", book.Author);
        command.Parameters.AddWithValue("$year", book.Year);
        command.Parameters.AddWithValue("$genre", book.Genre.ToString());
        command.Parameters.AddWithValue("$isRead", book.IsRead ? 1 : 0);
    }

    private static Book MapBook(SqliteDataReader reader) => new()
    {
        Isbn = reader.GetString(0),
        Title = reader.GetString(1),
        Author = reader.GetString(2),
        Year = reader.GetInt32(3),
        Genre = Enum.TryParse(reader.GetString(4), out Genre genre) ? genre : Genre.Fiction,
        IsRead = reader.GetInt32(5) != 0
    };
}
