using DustInTheWind.ConsoleTools.Controls.Spinners;
using Microsoft.Extensions.Logging.Abstractions;
using RecipeLearning.Data;
using RecipeLearning.DataParsing.Analysis;
using System.Data.SqlClient;

namespace RecipeControlPanel.Dialogs.DataParsing;

internal class SnapshotTokenizerDialog : IDialog
{
    private readonly SnapshotTokenizer snapshotTokenizer;
    private readonly IDialog nextDialog;
    private readonly ManualResetEventSlim finishEvent = new();

    private readonly ProgressBar progressBar = new()
    {
        LabelText = "Copied",
    };

    internal SnapshotTokenizerDialog(RecipeContext db, IDialog nextDialog)
    {
        this.nextDialog = nextDialog;
        snapshotTokenizer = new(db, NullLogger.Instance);

        snapshotTokenizer.OnStatusChanged += OnStatusChanged;
        snapshotTokenizer.OnPercentCompleted += OnPercentCompleted;
        snapshotTokenizer.OnSqlRowsCopied += OnSqlRowsCopied;
    }

    private void OnSqlRowsCopied(object? sender, SqlRowsCopiedEventArgs e)
    {
        progressBar.UnitOfMeasurement = $"% or {e.RowsCopied} rows to Database";
    }

    private void OnPercentCompleted(object? sender, double e)
    {
        progressBar.Value = Convert.ToInt32(e * 100);
    }

    private void OnStatusChanged(object? sender, TokenizerStatus importStatus)
    {
        switch (importStatus)
        {
            case TokenizerStatus.Finished:
                finishEvent.Set();
                break;
            default:
                break;
        }
    }

    public async Task<IDialog?> Execute(CancellationToken token = default)
    {
        Console.Clear();
        progressBar.Display();

        await snapshotTokenizer.Tokenize(token);

        finishEvent.Wait(token);

        progressBar.Close();
        finishEvent.Reset();

        return nextDialog;
    }
}
