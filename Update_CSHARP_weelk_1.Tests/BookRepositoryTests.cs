using Update_CSHARP_weelk_1.Domain;
using Update_CSHARP_weelk_1.Repository;

namespace Update_CSHARP_weelk_1.Tests;

public class BookRepositoryTests
{
    /// <summary>Minimal in-memory fake so repository tests don't touch disk.</summary>
    private sealed class FakeDataStore : IBookDataStore
    {
        public readonly List<Book> Books = new();
        public int CreateCalls { get; private set; }

        public Task<IReadOnlyList<Book>> ReadAllAsync() => Task.FromResult((IReadOnlyList<Book>)Books.AsReadOnly());

        public Task<Book?> ReadAsync(string isbn) => Task.FromResult(Books.FirstOrDefault(b => b.Isbn == isbn));

        public Task CreateAsync(Book book)
        {
            CreateCalls++;
            Books.Add(book);
            return Task.CompletedTask;
        }

        public int UpdateCalls { get; private set; }

        public Task UpdateAsync(Book book)
        {
            UpdateCalls++;
            var index = Books.FindIndex(b => b.Isbn == book.Isbn);
            if (index >= 0)
            {
                Books[index] = book;
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string isbn)
        {
            Books.RemoveAll(b => b.Isbn == isbn);
            return Task.CompletedTask;
        }
    }

    private static Book NewBook(string isbn = "978-1", string title = "Clean Code",
        string author = "Robert Martin", int year = 2008, Genre genre = Genre.NonFiction)
        => new() { Isbn = isbn, Title = title, Author = author, Year = year, Genre = genre };

    [Fact]
    public void Ctor_NullDataStore_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new BookRepository(null!, new BookValidator()));
    }

    [Fact]
    public async Task AddBookAsync_ValidBook_DelegatesToDataStore()
    {
        var fake = new FakeDataStore();
        var repo = new BookRepository(fake, new BookValidator());

        await repo.AddBookAsync(NewBook("978-1"));

        Assert.Equal(1, fake.CreateCalls);
        Assert.Single(fake.Books);
    }

    [Fact]
    public async Task AddBookAsync_InvalidBook_ThrowsAndDoesNotReachDataStore()
    {
        var fake = new FakeDataStore();
        var repo = new BookRepository(fake, new BookValidator());

        await Assert.ThrowsAsync<ArgumentException>(() => repo.AddBookAsync(NewBook(isbn: "")));

        // Repository-level validation must stop invalid data before the store is touched.
        Assert.Equal(0, fake.CreateCalls);
        Assert.Empty(fake.Books);
    }

    [Fact]
    public async Task AddBookAsync_NullBook_Throws()
    {
        var repo = new BookRepository(new FakeDataStore(), new BookValidator());

        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddBookAsync(null!));
    }

    [Fact]
    public async Task UpdateBookAsync_ValidBook_DelegatesToDataStore()
    {
        var fake = new FakeDataStore();
        fake.Books.Add(NewBook("978-1", title: "Old"));
        var repo = new BookRepository(fake, new BookValidator());

        await repo.UpdateBookAsync(NewBook("978-1", title: "New"));

        Assert.Equal(1, fake.UpdateCalls);
        Assert.Equal("New", fake.Books.Single().Title);
    }

    [Fact]
    public async Task UpdateBookAsync_InvalidBook_ThrowsAndDoesNotReachDataStore()
    {
        var fake = new FakeDataStore();
        var repo = new BookRepository(fake, new BookValidator());

        await Assert.ThrowsAsync<ArgumentException>(() => repo.UpdateBookAsync(NewBook(year: 0)));

        Assert.Equal(0, fake.UpdateCalls);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RemoveBookAsync_BlankIsbn_Throws(string isbn)
    {
        var repo = new BookRepository(new FakeDataStore(), new BookValidator());

        await Assert.ThrowsAsync<ArgumentException>(() => repo.RemoveBookAsync(isbn));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetBookByIdAsync_BlankIsbn_Throws(string isbn)
    {
        var repo = new BookRepository(new FakeDataStore(), new BookValidator());

        await Assert.ThrowsAsync<ArgumentException>(() => repo.GetBookByIdAsync(isbn));
    }
}
