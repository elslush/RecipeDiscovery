using Google.Apis.Download;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipeLearning.DataParsing.Data;
using RecipeLearning.Import;
using RecipeLearning.Import.Streams;
using System.Data.SqlClient;

namespace RecipeLearning.DataParsing.Inputs;

public class ParsedSubstitutionImporter : Importer<ParsedSubstitution>
{
    private static readonly FileRetriever fileRetriever = new("1F2MzXDZXSyVY6lHekM4Co8OkJM7z3bR3", "ingredient_predictions.zip", "ingredient_predictions.csv");
    private static readonly SqlBulkCopyColumnMapping[] mappings = {
        new(0, "SubstitutionID"),
        new(1, "Substitution1Name"),
        new(2, "Substitution1Quantity"),
        new(3, "Substitution1Unit"),
        new(4, "Substitution1Comment"),
        new(5, "Substitution1Other"),
        new(6, "Substitution2Name"),
        new(7, "Substitution2Quantity"),
        new(8, "Substitution2Unit"),
        new(9, "Substitution2Comment"),
        new(10, "Substitution2Other"),
    };

    private readonly ILogger logger;

    public ParsedSubstitutionImporter(DbContext db, ILogger logger) : base(db, fileRetriever, mappings)
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
