using Google.Apis.Download;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipeLearning.DataParsing.Data;
using RecipeLearning.Import;
using RecipeLearning.Import.Streams;
using Sylvan.Data.Csv;
using System.Data.SqlClient;

namespace RecipeLearning.DataParsing.Inputs;

public class ParsedIngredientImporter : ParsedImporter<ParsedIngredient>
{
    private static readonly FileRetriever fileRetriever = new("1F2MzXDZXSyVY6lHekM4Co8OkJM7z3bR3", "ingredient_predictions.zip", "ingredient_predictions.csv");
    private static readonly SqlBulkCopyColumnMapping[] mappings = { 
        new("IngredientID", "IngredientID"), 
        new("Name", "Name"),
        new("Unit", "Unit"),
        new("Quantity", "Quantity"),
        new("Comment", "Comment"),
        new("Other", "Other"),
    };

    private readonly ILogger logger;

    public ParsedIngredientImporter(DbContext db, ILogger logger) : base(db, fileRetriever, mappings)
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

    protected override IEnumerable<ParsedIngredient> ParseReader(CsvDataReader csvDataReader)
    {
        while (csvDataReader.Read())
        {
            ParsedIngredient parsedIngredient = new()
            {
                IngredientID = csvDataReader.GetInt32(0),
                Name = csvDataReader.GetString(1),
                Unit = csvDataReader.GetString(2),
                Comment = csvDataReader.GetString(4),
                Other = csvDataReader.GetString(6),
            };
            var quantity = csvDataReader.GetString(3);
            decimal? quantityDouble;
            if (!quantity.Equals(string.Empty) && (quantityDouble = quantity.ParseNumber()) is not null)
                parsedIngredient.Quantity = (float)quantityDouble;
            yield return parsedIngredient;
        }
    }
}
