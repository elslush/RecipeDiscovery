using DustInTheWind.ConsoleTools.Controls;
using DustInTheWind.ConsoleTools.Controls.Menus;
using DustInTheWind.ConsoleTools.Controls.Menus.MenuItems;
using RecipeLearning.Data;

namespace RecipeControlPanel.Dialogs.DataParsing;

internal class DataParsingDialog : IDialog
{
    private readonly ManualResetEvent oSignalEvent = new(false);
    private readonly ScrollMenu scrollMenu = new()
    {
        HorizontalAlignment = HorizontalAlignment.Left,
        EraseAfterClose = true,
    };
    private IDialog? dialog;

    internal DataParsingDialog(RecipeContext db)
    {
        scrollMenu = new()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            EraseAfterClose = true,
        };
        var outputTrainingDialog = new OutputTrainingDialog(db, this);
        var outputEvaluatesDialog = new OutputEvaluatesDialog(db, this);
        var importIngredientPredictionsDialog = new ImportParsedIngredientDialog(db, this);
        var importParsedSubstitutionDialog = new ImportParsedSubstitutionDialog(db, this);
        var snapshotTokenizerDialog = new SnapshotTokenizerDialog(db, this);

        scrollMenu.AddItem(new LabelMenuItem() { Text = "Step 8: Output tokenized training data to .csv.", Command = new ActionCommand(() => SetDialog(outputTrainingDialog)) });
        scrollMenu.AddItem(new LabelMenuItem() { Text = "Step 9: Output ingredients, nutrition, and substitutions to .csv to evaluate.", Command = new ActionCommand(() => SetDialog(outputEvaluatesDialog)) });
        scrollMenu.AddItem(new LabelMenuItem() { Text = "Step 13: Import ingredient predictions into database.", Command = new ActionCommand(() => SetDialog(importIngredientPredictionsDialog)) });
        scrollMenu.AddItem(new LabelMenuItem() { Text = "Step 14: Import substitution predictions into database.", Command = new ActionCommand(() => SetDialog(importParsedSubstitutionDialog)) });
        scrollMenu.AddItem(new LabelMenuItem() { Text = "Step (optional): Save training data tokens to database.", Command = new ActionCommand(() => SetDialog(snapshotTokenizerDialog)) });
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
        Console.WriteLine("Data Parsing");
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
