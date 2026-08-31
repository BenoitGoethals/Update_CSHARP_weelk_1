using Update_CSHARP_weelk_1.Persistence;
using Update_CSHARP_weelk_1.Domain;

namespace Update_CSHARP_weelk_1.Presentation;

/// <summary>
/// Presentation concern: lets the user pick which persistence backend to use at startup.
/// </summary>
public static class DataStoreSelector
{
    public static IBookDataStore Select()
    {
        while (true)
        {
            Console.WriteLine("\n=== Select Data Store ===");
            Console.WriteLine("1. JSON file");
            Console.WriteLine("2. SQLite database");
            Console.Write("\nSelect a data store: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    Console.WriteLine("Using JSON file store.");
                    return new DataStoreJson();
                case "2":
                    Console.WriteLine("Using SQLite store.");
                    return new DataStoreSqlite();
                default:
                    Console.WriteLine("Invalid choice. Please enter 1 or 2.");
                    break;
            }
        }
    }
}
