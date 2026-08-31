namespace Update_CSHARP_weelk_1.Domain;

/// <summary>
/// Async CRUD contract for persisting <see cref="Book"/> records.
/// </summary>
public interface IBookDataStore
{
    Task<IReadOnlyList<Book>> ReadAllAsync();

    Task<Book?> ReadAsync(string isbn);

    Task CreateAsync(Book book);

    Task UpdateAsync(Book book);

    Task DeleteAsync(string isbn);
}
