using System.Text.Json.Serialization;

namespace Update_CSHARP_weelk_1.Domain;

public sealed class Book : IEquatable<Book>
{
    public required string Isbn { get; init; }
    public required string Title { get; init; }
    public required string Author { get; init; }

    public int Year { get; set; }

    public Genre Genre { get; set; }

    public bool IsRead { get; set; }

    [JsonIgnore]
    public string Description =>
        $"{Title} by {Author} ({Year})";

    public override string ToString() =>
        $"ISBN: {Isbn}, Title: {Title}, Author: {Author}, Year: {Year}, Genre: {Genre}, IsRead: {IsRead}";

    public bool Equals(Book? other) =>
        other is not null && string.Equals(Isbn, other.Isbn, StringComparison.Ordinal);

    public override bool Equals(object? obj) => Equals(obj as Book);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Isbn);
}
