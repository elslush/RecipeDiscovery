using DustInTheWind.ConsoleTools.Controls;
using DustInTheWind.ConsoleTools.Controls.Menus;
using DustInTheWind.ConsoleTools.Controls.Menus.MenuItems;
using RecipeLearning.Data;

namespace RecipeControlPanel.Dialogs.RecipeSimilarities;

internal class RecipeSimilaritiesDialog : IDialog
{
    private readonly ManualResetEvent oSignalEvent = new(false);
    private readonly ScrollMenu scrollMenu = new()
    {
        HorizontalAlignment = HorizontalAlignment.Left,
        EraseAfterClose = true,
    };
    private IDialog? dialog;

    internal RecipeSimilaritiesDialog(RecipeContext db)
    {
        scrollMenu = new()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            EraseAfterClose = true,
        };
        var recipeLLVMOutputDialog = new RecipeLLVMOutputDialog(db, this);
        var recipeSimilarityImporterDialog = new RecipeSimilarityImporterDialog(db, this);
        var recipeSimilarityImporterWSubsDialog = new RecipeSimilarityImporterWSubsDialog(db, this);

        scrollMenu.AddItem(new LabelMenuItem() { Text = "Step 16: Output recipe ingredient vectors to .csv.", Command = new ActionCommand(() => SetDialog(recipeLLVMOutputDialog)) });
        scrollMenu.AddItem(new LabelMenuItem() { Text = "Step 18: Import top 10 similar recipes to database.", Command = new ActionCommand(() => SetDialog(recipeSimilarityImporterDialog)) });
        scrollMenu.AddItem(new LabelMenuItem() { Text = "Step 19: Import top 10 similar recipes (with substitutes accounted for) to database.", Command = new ActionCommand(() => SetDialog(recipeSimilarityImporterWSubsDialog)) });
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
        Console.WriteLine("Recipe Similarity");
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
