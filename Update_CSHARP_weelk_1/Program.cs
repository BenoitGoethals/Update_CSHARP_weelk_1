using Update_CSHARP_weelk_1.Domain;
using Update_CSHARP_weelk_1.Presentation;
using Update_CSHARP_weelk_1.Repository;

namespace Update_CSHARP_weelk_1;

public static class Program
{
    public static async Task Main(string[] args)
    {
        // Composition root: wire the layers together.
        IBookValidator validator = new BookValidator();
        IBookDataStore dataStore = DataStoreSelector.Select();
        IBookRepository repository = new BookRepository(dataStore, validator);
        var app = new ConsoleApp(repository, validator);

        await app.RunAsync();
    }
}
