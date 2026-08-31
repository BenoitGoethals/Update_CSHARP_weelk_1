namespace Update_CSHARP_weelk_1.Presentation;

public sealed class Menu
{
    private readonly Dictionary<int, ICommand> _commands = new();

    public void AddCommand(int key, ICommand command)
    {
        _commands[key] = command;
    }

    public void DisplayMenu()
    {
        Console.WriteLine("\n=== Book Repository Menu ===");
        foreach (var kvp in _commands)
        {
            Console.WriteLine($"{kvp.Key}. {kvp.Value.GetDescription()}");
        }
        Console.Write("\nSelect an option: ");
    }

    /// <summary>
    /// Executes the command bound to <paramref name="key"/> and reports whether
    /// it should terminate the session.
    /// </summary>
    public async Task<bool> ExecuteCommandAsync(int key)
    {
        if (!_commands.TryGetValue(key, out var command))
        {
            Console.WriteLine("Invalid option. Please try again.");
            return false;
        }

        await command.ExecuteAsync();
        return command.TerminatesSession;
    }
}
