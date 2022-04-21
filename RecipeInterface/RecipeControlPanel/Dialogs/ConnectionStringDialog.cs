using DustInTheWind.ConsoleTools.Controls;
using DustInTheWind.ConsoleTools.Controls.Menus;
using DustInTheWind.ConsoleTools.Controls.Menus.MenuItems;
using Microsoft.Extensions.Configuration;

namespace RecipeControlPanel.Dialogs;

internal class ConnectionStringDialog
{
    private readonly ManualResetEvent oSignalEvent = new(false);
    private readonly ScrollMenu scrollMenu = new()
    {
        HorizontalAlignment = HorizontalAlignment.Left,
        EraseAfterClose = true,
    };
    private string connectionString = string.Empty;

    internal ConnectionStringDialog()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var connectionStrings = config.GetRequiredSection("ConnectionStrings")
            .GetChildren()
            .Select(section => new LabelMenuItem() { Text = section.Key, Command = new ActionCommand(() => SetConnectionString(section.Value)) });
        
        scrollMenu.AddItems(connectionStrings);
    }

    public string GetConnectionString()
    {
        Console.WriteLine("Please Select a Database Connection.");
        Console.WriteLine(" Use the arrow keys to scroll.");
        Console.WriteLine(" Press Enter to select.");
        Console.WriteLine(string.Empty);
        scrollMenu.Display();

        oSignalEvent.WaitOne();
        oSignalEvent.Reset();

        return connectionString;
    }

    private void SetConnectionString(string connectionString)
    {
        this.connectionString = connectionString;
        oSignalEvent.Set();
    }
}
