namespace Update_CSHARP_weelk_1.Domain;

/// <summary>
/// Validates <see cref="Book"/> instances. Shared by the presentation (input)
/// layer and the repository layer so both enforce the same invariants.
/// </summary>
public interface IBookValidator
{
    /// <summary>Earliest acceptable publication year.</summary>
    int MinYear { get; }

    /// <summary>Latest acceptable publication year.</summary>
    int MaxYear { get; }

    /// <summary>
    /// Returns a list of human-readable validation errors. An empty list means the book is valid.
    /// </summary>
    IReadOnlyList<string> Validate(Book? book);

    bool IsValid(Book? book);

    /// <summary>Throws when the book violates any invariant.</summary>
    void ThrowIfInvalid(Book book);
}
