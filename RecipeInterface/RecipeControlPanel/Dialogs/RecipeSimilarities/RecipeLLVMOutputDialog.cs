using DustInTheWind.ConsoleTools.Controls.Spinners;
using RecipeLearning.Data;
using RecipeLearning.RecipeSimilarities.Outputs;

namespace RecipeControlPanel.Dialogs.RecipeSimilarities;

internal class RecipeLLVMOutputDialog : IDialog
{
    private readonly OutputRecipeLLVM output;
    private readonly IDialog nextDialog;

    private readonly ProgressBar progressBar = new()
    {
        LabelText = "Computed",
    };

    internal RecipeLLVMOutputDialog(RecipeContext db, IDialog nextDialog)
    {
        this.nextDialog = nextDialog;
        output = new(db);

        output.OnPercentCompleted += OnPercentCompleted;
    }

    private void OnPercentCompleted(object? sender, double e)
    {
        progressBar.Value = Convert.ToInt32(e * 100);
    }

    public async Task<IDialog?> Execute(CancellationToken token = default)
    {
        Console.Clear();
        progressBar.Display();

        await output.Output(token);

        progressBar.Close();

        return nextDialog;
    }
}
