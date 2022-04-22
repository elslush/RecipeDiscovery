using DustInTheWind.ConsoleTools.Controls.Spinners;
using RecipeLearning.Data;
using RecipeLearning.DataParsing.Outputs;

namespace RecipeControlPanel.Dialogs.DataParsing;

internal class OutputTrainingDialog : IDialog
{
    private readonly OutputBertTensors outputBertTensors;
    private readonly IDialog nextDialog;

    private readonly ProgressBar progressBar = new()
    {
        LabelText = "Saving training file",
    };

    internal OutputTrainingDialog(RecipeContext db, IDialog nextDialog)
    {
        this.nextDialog = nextDialog;
        outputBertTensors = new(db);

        outputBertTensors.OnPercentCompleted += OnPercentCompleted;
    }

    private void OnPercentCompleted(object? sender, double e)
    {
        progressBar.Value = Convert.ToInt32(e * 100);
    }

    public async Task<IDialog?> Execute(CancellationToken token = default)
    {
        Console.Clear();
        progressBar.Display();

        await outputBertTensors.CalculateEmbeddings(token);

        progressBar.Close();

        return nextDialog;
    }
}