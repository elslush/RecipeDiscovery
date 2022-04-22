using DustInTheWind.ConsoleTools.Controls.Spinners;
using Microsoft.Extensions.Logging.Abstractions;
using RecipeLearning.Data;
using RecipeLearning.Import;
using RecipeLearning.Import.Streams;
using RecipeLearning.RecipeSimilarities.Inputs;
using System.Data.SqlClient;

namespace RecipeControlPanel.Dialogs.RecipeSimilarities;

internal class RecipeSimilarityImporterDialog : IDialog
{
    private readonly RecipeSimilarityImporter recipeSimilarityImporter;
    private readonly IDialog nextDialog;
    private readonly ManualResetEventSlim finishEvent = new();

    private readonly ProgressBar progressBar = new()
    {
        LabelText = "Copied",
    };
    private readonly Spinner spinner = new()
    {
        Label = "Downloading From Cloud ",
    };

    internal RecipeSimilarityImporterDialog(RecipeContext db, IDialog nextDialog)
    {
        this.nextDialog = nextDialog;
        recipeSimilarityImporter = new(db, NullLogger.Instance);

        recipeSimilarityImporter.OnStatusChanged += OnStatusChanged;
        recipeSimilarityImporter.OnStreamProgressChanged += OnStreamProgressChanged;
        recipeSimilarityImporter.OnSqlRowsCopied += OnSqlRowsCopied;
    }

    private void OnSqlRowsCopied(object? sender, SqlRowsCopiedEventArgs e)
    {
        progressBar.UnitOfMeasurement = $"% or {e.RowsCopied} rows to Database";
    }

    private void OnStreamProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        progressBar.Value = Convert.ToInt32(e.ProgressPercentage * 100);
    }

    private void OnStatusChanged(object? sender, ImportStatus importStatus)
    {
        switch (importStatus)
        {
            case ImportStatus.Downloading:
                Console.Clear();
                spinner.Display();
                break;
            case ImportStatus.Finished:
                finishEvent.Set();
                break;
            default:
                spinner.Close();
                progressBar.Display();
                break;
        }
    }

    public async Task<IDialog?> Execute(CancellationToken token = default)
    {
        Console.Clear();

        await recipeSimilarityImporter.Import(token);

        finishEvent.Wait(token);

        progressBar.Close();
        finishEvent.Reset();

        return nextDialog;
    }
}
