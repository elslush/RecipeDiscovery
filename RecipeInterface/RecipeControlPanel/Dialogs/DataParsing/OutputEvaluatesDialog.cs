using DustInTheWind.ConsoleTools.Controls.Spinners;
using RecipeLearning.Data;
using RecipeLearning.DataParsing.Outputs;

namespace RecipeControlPanel.Dialogs.DataParsing;

internal class OutputEvaluatesDialog : IDialog
{
    private readonly OutputIngredients outputIngredients;
    private readonly OutputNutrition outputNutrition;
    private readonly OutputSubstitutions outputSubstitutions;
    private readonly IDialog nextDialog;

    private readonly ProgressBar progressBar = new()
    {
        LabelText = "Saving ",
    };

    internal OutputEvaluatesDialog(RecipeContext db, IDialog nextDialog)
    {
        this.nextDialog = nextDialog;
        outputIngredients = new(db);
        outputNutrition = new(db);
        outputSubstitutions = new(db);

        outputIngredients.OnPercentCompleted += OnPercentCompleted;
        outputNutrition.OnPercentCompleted += OnPercentCompleted;
        outputSubstitutions.OnPercentCompleted += OnPercentCompleted;
    }

    private void OnPercentCompleted(object? sender, double e)
    {
        progressBar.Value = Convert.ToInt32(e * 100);
    }

    public async Task<IDialog?> Execute(CancellationToken token = default)
    {
        Console.Clear();
        progressBar.Display();

        progressBar.UnitOfMeasurement = "Ingredient File";
        await outputIngredients.Output(token);

        progressBar.UnitOfMeasurement = "Nutrition File";
        await outputNutrition.Output(token);

        progressBar.UnitOfMeasurement = "Substitutions File";
        await outputSubstitutions.Output(token);

        progressBar.Close();

        return nextDialog;
    }
}
