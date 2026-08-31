using Update_CSHARP_weelk_1.Domain;

namespace Update_CSHARP_weelk_1.Repository;

public interface IBookRepository
{
    Task<List<Book>> GetAllBooksAsync();
    Task<Book?> GetBookByIdAsync(string isbn);
    Task AddBookAsync(Book book);
    Task UpdateBookAsync(Book book);
    Task RemoveBookAsync(string isbn);
}
