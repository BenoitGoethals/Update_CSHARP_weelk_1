using Update_CSHARP_weelk_1.Domain;
using Update_CSHARP_weelk_1.Repository;

namespace Update_CSHARP_weelk_1.Presentation;

/// <summary>
/// Drives the interactive console session: builds the menu, registers commands,
/// and runs the main input loop. This is the top of the presentation layer.
/// </summary>
public sealed class ConsoleApp
{
    private readonly Menu _menu = new();

    public ConsoleApp(IBookRepository repository, IBookValidator validator)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(validator);

        _menu.AddCommand(1, new AddBookCommand(repository, validator));
        _menu.AddCommand(2, new UpdateBookCommand(repository, validator));
        _menu.AddCommand(3, new RemoveBookCommand(repository));
        _menu.AddCommand(4, new ListBooksCommand(repository));
        _menu.AddCommand(5, new ExitCommand());
    }

    public async Task RunAsync()
    {
        var running = true;
        while (running)
        {
            _menu.DisplayMenu();

            if (int.TryParse(Console.ReadLine(), out var choice))
            {
                running = !await _menu.ExecuteCommandAsync(choice);
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a number.");
            }
        }
    }
}
