using DustInTheWind.ConsoleTools.Controls;
using DustInTheWind.ConsoleTools.Controls.Menus;
using DustInTheWind.ConsoleTools.Controls.Menus.MenuItems;
using RecipeLearning.Data;
namespace RecipeControlPanel.Dialogs.DataMatching;

internal class DataMatchingDialog : IDialog
{
    private readonly ManualResetEvent oSignalEvent = new(false);
    private readonly ScrollMenu scrollMenu = new()
    {
        HorizontalAlignment = HorizontalAlignment.Left,
        EraseAfterClose = true,
    };
    private IDialog? dialog;

    internal DataMatchingDialog(RecipeContext db)
    {
        scrollMenu = new()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            EraseAfterClose = true,
        };
        var ingredientMatchDialog = new IngredientMatchDialog(db, this);

        scrollMenu.AddItem(new LabelMenuItem() { Text = "Step 15: Output tokenized training data to .csv.", Command = new ActionCommand(() => SetDialog(ingredientMatchDialog)) });
        scrollMenu.AddItem(new LabelMenuItem() { Text = "Return", Command = new ActionCommand(() => SetDialog(null)) });
    }

    private void SetDialog(IDialog? dialog)
    {
        this.dialog = dialog;
        oSignalEvent.Set();
    }

    public Task<IDialog?> Execute(CancellationToken token = default)
    {
        Console.Clear();
        Console.WriteLine("Data Matching");
        Console.WriteLine("------------");
        Console.WriteLine("Please Select an action.");
        Console.WriteLine(" Use the arrow keys to scroll.");
        Console.WriteLine(" Press Enter to select.");

        Console.WriteLine(string.Empty);
        scrollMenu.Display();

        oSignalEvent.WaitOne();
        oSignalEvent.Reset();

        return Task.FromResult(dialog);
    }
}
