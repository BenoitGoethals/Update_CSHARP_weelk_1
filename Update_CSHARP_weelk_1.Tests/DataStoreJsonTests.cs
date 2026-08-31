using Update_CSHARP_weelk_1.Persistence;
using Update_CSHARP_weelk_1.Domain;

namespace Update_CSHARP_weelk_1.Tests;

public sealed class DataStoreJsonTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _filePath;

    public DataStoreJsonTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "datastore-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _filePath = Path.Combine(_tempDir, "books.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    private DataStoreJson CreateStore() => new(_filePath);

    private static Book NewBook(string isbn = "978-1", string title = "Clean Code",
        string author = "Robert Martin", int year = 2008, Genre genre = Genre.NonFiction, bool isRead = false)
        => new()
        {
            Isbn = isbn,
            Title = title,
            Author = author,
            Year = year,
            Genre = genre,
            IsRead = isRead
        };

    // ---------- ReadAll ----------

    [Fact]
    public async Task ReadAllAsync_WhenFileMissing_ReturnsEmpty()
    {
        var store = CreateStore();

        var books = await store.ReadAllAsync();

        Assert.Empty(books);
    }

    [Fact]
    public async Task ReadAllAsync_ReturnsCreatedBooks()
    {
        var store = CreateStore();
        await store.CreateAsync(NewBook("978-1"));
        await store.CreateAsync(NewBook("978-2", title: "The Pragmatic Programmer"));

        var books = await store.ReadAllAsync();

        Assert.Equal(2, books.Count);
        Assert.Contains(books, b => b.Isbn == "978-1");
        Assert.Contains(books, b => b.Isbn == "978-2");
    }

    // ---------- Create ----------

    [Fact]
    public async Task CreateAsync_PersistsToDisk()
    {
        var store = CreateStore();

        await store.CreateAsync(NewBook("978-1"));

        Assert.True(File.Exists(_filePath));
        var contents = await File.ReadAllTextAsync(_filePath);
        Assert.Contains("978-1", contents);
    }

    [Fact]
    public async Task CreateAsync_NullBook_Throws()
    {
        var store = CreateStore();

        await Assert.ThrowsAsync<ArgumentNullException>(() => store.CreateAsync(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_BlankIsbn_Throws(string isbn)
    {
        var store = CreateStore();

        await Assert.ThrowsAsync<ArgumentException>(() => store.CreateAsync(NewBook(isbn)));
    }

    [Fact]
    public async Task CreateAsync_DuplicateIsbn_Throws()
    {
        var store = CreateStore();
        await store.CreateAsync(NewBook("978-1"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => store.CreateAsync(NewBook("978-1")));
    }

    // ---------- Read ----------

    [Fact]
    public async Task ReadAsync_ExistingIsbn_ReturnsBook()
    {
        var store = CreateStore();
        await store.CreateAsync(NewBook("978-1", title: "Refactoring"));

        var book = await store.ReadAsync("978-1");

        Assert.NotNull(book);
        Assert.Equal("Refactoring", book!.Title);
    }

    [Fact]
    public async Task ReadAsync_UnknownIsbn_ReturnsNull()
    {
        var store = CreateStore();
        await store.CreateAsync(NewBook("978-1"));

        var book = await store.ReadAsync("does-not-exist");

        Assert.Null(book);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ReadAsync_BlankIsbn_Throws(string isbn)
    {
        var store = CreateStore();

        await Assert.ThrowsAsync<ArgumentException>(() => store.ReadAsync(isbn));
    }

    // ---------- Update ----------

    [Fact]
    public async Task UpdateAsync_ExistingBook_ReplacesIt()
    {
        var store = CreateStore();
        await store.CreateAsync(NewBook("978-1", title: "Old Title", isRead: false));

        await store.UpdateAsync(NewBook("978-1", title: "New Title", isRead: true));

        var book = await store.ReadAsync("978-1");
        Assert.NotNull(book);
        Assert.Equal("New Title", book!.Title);
        Assert.True(book.IsRead);
        Assert.Single(await store.ReadAllAsync());
    }

    [Fact]
    public async Task UpdateAsync_UnknownIsbn_Throws()
    {
        var store = CreateStore();

        await Assert.ThrowsAsync<InvalidOperationException>(() => store.UpdateAsync(NewBook("978-missing")));
    }

    [Fact]
    public async Task UpdateAsync_NullBook_Throws()
    {
        var store = CreateStore();

        await Assert.ThrowsAsync<ArgumentNullException>(() => store.UpdateAsync(null!));
    }

    // ---------- Delete ----------

    [Fact]
    public async Task DeleteAsync_ExistingBook_RemovesIt()
    {
        var store = CreateStore();
        await store.CreateAsync(NewBook("978-1"));

        await store.DeleteAsync("978-1");

        Assert.Empty(await store.ReadAllAsync());
    }

    [Fact]
    public async Task DeleteAsync_UnknownIsbn_Throws()
    {
        var store = CreateStore();

        await Assert.ThrowsAsync<InvalidOperationException>(() => store.DeleteAsync("978-missing"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DeleteAsync_BlankIsbn_Throws(string isbn)
    {
        var store = CreateStore();

        await Assert.ThrowsAsync<ArgumentException>(() => store.DeleteAsync(isbn));
    }

    // ---------- Persistence / serialization ----------

    [Fact]
    public async Task Data_PersistsAcrossStoreInstances()
    {
        var first = CreateStore();
        await first.CreateAsync(NewBook("978-1", title: "Domain-Driven Design"));

        var second = CreateStore();
        var book = await second.ReadAsync("978-1");

        Assert.NotNull(book);
        Assert.Equal("Domain-Driven Design", book!.Title);
    }

    [Fact]
    public async Task Genre_IsSerializedAsString()
    {
        var store = CreateStore();
        await store.CreateAsync(NewBook("978-1", genre: Genre.ScienceFiction));

        var contents = await File.ReadAllTextAsync(_filePath);

        Assert.Contains("\"ScienceFiction\"", contents);
        Assert.DoesNotContain("\"Genre\": 3", contents);
    }

    [Fact]
    public async Task Description_IsNotPersisted()
    {
        var store = CreateStore();
        await store.CreateAsync(NewBook("978-1"));

        var contents = await File.ReadAllTextAsync(_filePath);

        Assert.DoesNotContain("Description", contents);
    }

    [Fact]
    public async Task Store_RoundTripsAllBookFields()
    {
        var store = CreateStore();
        await store.CreateAsync(NewBook("978-1", title: "T", author: "A", year: 1999,
            genre: Genre.Mystery, isRead: true));

        var reloaded = await CreateStore().ReadAsync("978-1");

        Assert.NotNull(reloaded);
        Assert.Equal("978-1", reloaded!.Isbn);
        Assert.Equal("T", reloaded.Title);
        Assert.Equal("A", reloaded.Author);
        Assert.Equal(1999, reloaded.Year);
        Assert.Equal(Genre.Mystery, reloaded.Genre);
        Assert.True(reloaded.IsRead);
    }
}
