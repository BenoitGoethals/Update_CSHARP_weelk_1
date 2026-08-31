using Update_CSHARP_weelk_1.Domain;

namespace Update_CSHARP_weelk_1.Tests;

public class BookValidatorTests
{
    private readonly IBookValidator _validator = new BookValidator();

    private static Book NewBook(string isbn = "978-1", string title = "Clean Code",
        string author = "Robert Martin", int year = 2008, Genre genre = Genre.NonFiction)
        => new() { Isbn = isbn, Title = title, Author = author, Year = year, Genre = genre };

    [Fact]
    public void Validate_ValidBook_ReturnsNoErrors()
    {
        Assert.Empty(_validator.Validate(NewBook()));
        Assert.True(_validator.IsValid(NewBook()));
    }

    [Fact]
    public void Validate_NullBook_ReturnsError()
    {
        Assert.NotEmpty(_validator.Validate(null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankIsbn_ReturnsError(string isbn)
    {
        var errors = _validator.Validate(NewBook(isbn: isbn));
        Assert.Contains(errors, e => e.Contains("ISBN"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankTitle_ReturnsError(string title)
    {
        var errors = _validator.Validate(NewBook(title: title));
        Assert.Contains(errors, e => e.Contains("Title"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankAuthor_ReturnsError(string author)
    {
        var errors = _validator.Validate(NewBook(author: author));
        Assert.Contains(errors, e => e.Contains("Author"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(9999)]
    public void Validate_OutOfRangeYear_ReturnsError(int year)
    {
        var errors = _validator.Validate(NewBook(year: year));
        Assert.Contains(errors, e => e.Contains("Year"));
    }

    [Fact]
    public void Validate_UndefinedGenre_ReturnsError()
    {
        var errors = _validator.Validate(NewBook(genre: (Genre)999));
        Assert.Contains(errors, e => e.Contains("Genre"));
    }

    [Fact]
    public void ThrowIfInvalid_InvalidBook_Throws()
    {
        Assert.Throws<ArgumentException>(() => _validator.ThrowIfInvalid(NewBook(isbn: "")));
    }

    [Fact]
    public void ThrowIfInvalid_ValidBook_DoesNotThrow()
    {
        var ex = Record.Exception(() => _validator.ThrowIfInvalid(NewBook()));
        Assert.Null(ex);
    }
}
