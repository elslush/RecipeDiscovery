using Google.Apis.Download;
using Microsoft.Extensions.Logging;
using RecipeLearning.Data;
using RecipeLearning.DataCollection.Data;
using RecipeLearning.Import;
using RecipeLearning.Import.Streams;
using System.Data.SqlClient;

namespace RecipeLearning.DataCollection;

public class SnapshotImporter : Importer<IngredientSnapshot>
{
    private static readonly FileRetriever fileRetriever = new("1A2fqnDs-B3uF8DOsF2S1vi4KdEEJudhn", "ingredients_snapshot.zip", "nyt-ingredients-snapshot-2015.csv");
    private readonly ILogger logger;

    public SnapshotImporter(RecipeContext db, ILogger logger) : base(db, fileRetriever)
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
