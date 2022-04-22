using Google.Apis.Download;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipeLearning.DataParsing.Data;
using RecipeLearning.Import.Streams;
using Sylvan.Data.Csv;
using System.Data.SqlClient;

namespace RecipeLearning.DataParsing.Inputs;

public class ParsedSubstitutionImporter : ParsedImporter<ParsedSubstitution>
{
    private static readonly FileRetriever fileRetriever = new("1Gf0y8nQ_DYvZZYuHWJYZ3EeGhiPxKDvw", "substitution_predictions.zip", "substitution_predictions.csv");
    private static readonly SqlBulkCopyColumnMapping[] mappings = {
        new("SubstitutionID", "SubstitutionID"),
        new("Substitution1Name", "Substitution1Name"),
        new("Substitution1Quantity", "Substitution1Quantity"),
        new("Substitution1Unit", "Substitution1Unit"),
        new("Substitution1Comment", "Substitution1Comment"),
        new("Substitution1Other", "Substitution1Other"),
        new("Substitution2Name", "Substitution2Name"),
        new("Substitution2Quantity", "Substitution2Quantity"),
        new("Substitution2Unit", "Substitution2Unit"),
        new("Substitution2Comment", "Substitution2Comment"),
        new("Substitution2Other", "Substitution2Other"),
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

    protected override IEnumerable<ParsedSubstitution> ParseReader(CsvDataReader csvDataReader)
    {
        while (csvDataReader.Read())
        {
            ParsedSubstitution parsedIngredient = new()
            {
                SubstitutionID = csvDataReader.GetInt32(0),
                Substitution1Name = csvDataReader.GetString(1),
                Substitution1Unit = csvDataReader.GetString(3),
                Substitution1Comment = csvDataReader.GetString(4),
                Substitution1Other = csvDataReader.GetString(6),
                Substitution2Name = csvDataReader.GetString(7),
                Substitution2Unit = csvDataReader.GetString(9),
                Substitution2Comment = csvDataReader.GetString(10),
                Substitution2Other = csvDataReader.GetString(11)
            };

            var quantity = csvDataReader.GetString(2);
            decimal? quantityDouble;
            if (!quantity.Equals(string.Empty) && (quantityDouble = quantity.ParseNumber()) is not null)
                parsedIngredient.Substitution1Quantity = (float)quantityDouble;

            quantity = csvDataReader.GetString(8);
            if (!quantity.Equals(string.Empty) && (quantityDouble = quantity.ParseNumber()) is not null)
                parsedIngredient.Substitution2Quantity = (float)quantityDouble;

            yield return parsedIngredient;
        }
    }
}
