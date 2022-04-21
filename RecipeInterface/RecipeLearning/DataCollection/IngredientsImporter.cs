using Google.Apis.Download;
using Microsoft.Extensions.Logging;
using RecipeLearning.Data;
using RecipeLearning.DataCollection.Data;
using RecipeLearning.Import;
using RecipeLearning.Import.Streams;
using System.Data.SqlClient;

namespace RecipeLearning.DataCollection;

public class IngredientsImporter : Importer<Ingredient>
{
    private static readonly FileRetriever fileRetriever = new("1vbxsLqk-CkVsNek5NqX8Cujza1OuxdyG", "Ingredients.zip", "Ingredients.csv");
    private static readonly SqlBulkCopyColumnMapping[] mappings = { new("RecipeID", "RecipeID"), new("Description", "Description") };
    private readonly ILogger logger;

    public IngredientsImporter(RecipeContext db, ILogger logger) : base(db, fileRetriever, mappings)
    {
        this.logger = logger;
        OnSqlRowsCopied += SqlRowsCopied;
        OnStreamProgressChanged += StreamProgressChanged;
        OnDownloadProgressChanged += DownloadProgressChanged;
    }

    private void SqlRowsCopied(object? sender, SqlRowsCopiedEventArgs e)
    {
        logger.LogInformation("Copied {RowsCopied} rows from csv so far...", e.RowsCopied);
    }

    private void StreamProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        logger.LogInformation("Read {ProgressPercentage} from csv so far...", e.ProgressPercentage.ToString("P"));
    }

    private void DownloadProgressChanged(object? sender, IDownloadProgress progress)
    {
        logger.LogInformation("Read {BytesDownloaded} from Google Drive Storage so far...", progress.BytesDownloaded);
    }
}
