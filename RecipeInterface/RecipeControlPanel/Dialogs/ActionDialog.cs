using DustInTheWind.ConsoleTools.Controls;
using DustInTheWind.ConsoleTools.Controls.Menus;
using DustInTheWind.ConsoleTools.Controls.Menus.MenuItems;
using RecipeControlPanel.Dialogs.DataCollection;
using RecipeControlPanel.Dialogs.DataMatching;
using RecipeControlPanel.Dialogs.DataParsing;
using RecipeControlPanel.Dialogs.RecipeSimilarities;
using RecipeLearning.Data;

namespace RecipeControlPanel.Dialogs;

internal class ActionDialog : IDialog
{
    private readonly ManualResetEvent oSignalEvent = new(false);
    private readonly ScrollMenu scrollMenu = new()
    {
        HorizontalAlignment = HorizontalAlignment.Left,
        EraseAfterClose = true,
    };
    private IDialog? dialog;

    internal ActionDialog(RecipeContext db)
    {
        scrollMenu = new()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            EraseAfterClose = true,
        };
        var dataCollectionDialog = new DataCollectionDialog(db, this);
        var dataMatchingDialog = new DataMatchingDialog(db, this);
        var dataParsingDialog = new DataParsingDialog(db, this);
        var recipeSimilaritiesDialog = new RecipeSimilaritiesDialog(db, this);

        scrollMenu.AddItem(new LabelMenuItem() { Text = "Data Collection", Command = new ActionCommand(() => SetDialog(dataCollectionDialog)) });
        scrollMenu.AddItem(new LabelMenuItem() { Text = "Data Parsing", Command = new ActionCommand(() => SetDialog(dataParsingDialog)) });
        scrollMenu.AddItem(new LabelMenuItem() { Text = "Data Matching", Command = new ActionCommand(() => SetDialog(dataMatchingDialog)) });
        scrollMenu.AddItem(new LabelMenuItem() { Text = "Recipe Similarities", Command = new ActionCommand(() => SetDialog(recipeSimilaritiesDialog)) });
        scrollMenu.AddItem(new LabelMenuItem() { Text = "Exit", Command = new ActionCommand(() => SetDialog(null)) });
    }

    private void SetDialog (IDialog? dialog)
    {
        this.dialog = dialog;
        oSignalEvent.Set();
    }

    public Task<IDialog?> Execute(CancellationToken token = default)
    {
        Console.Clear();
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
