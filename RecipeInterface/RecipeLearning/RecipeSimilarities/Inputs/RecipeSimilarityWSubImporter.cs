using Google.Apis.Download;
using Microsoft.Extensions.Logging;
using RecipeLearning.Data;
using RecipeLearning.Import;
using RecipeLearning.Import.Streams;
using RecipeLearning.RecipeSimilarities.Data;
using System.Data.SqlClient;

namespace RecipeLearning.RecipeSimilarities.Inputs;

public class RecipeSimilarityWSubImporter : Importer<RecipeSimilarity>
{
    private static readonly FileRetriever fileRetriever = new("1HzSTg2BTQ7_fz5gJyNjtknM6aMKEkUqS", "recipe_similarities_sub.zip", "recipe_similarities_sub.csv");
    private static readonly SqlBulkCopyColumnMapping[] mappings = { new(0, "RecipeID"), new(1, "SimilarRecipeID"), new(2, "Jaccard") };
    private readonly ILogger logger;

    public RecipeSimilarityWSubImporter(RecipeContext db, ILogger logger) : base(db, fileRetriever, mappings)
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

