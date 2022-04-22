using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipeLearning.Data;
using RecipeLearning.DataCollection.Data;
using Sylvan.Data;
using System.Data.SqlClient;

namespace RecipeLearning.DataParsing.Analysis;

public enum TokenizerStatus
{
    Tokenizing,
    Finished,
}

public class SnapshotTokenizer
{
    private readonly RecipeContext db;
    private readonly ILogger logger;
    private int total, completed;

    public SnapshotTokenizer(RecipeContext db, ILogger logger)
    {
        this.db = db;
        this.logger = logger;
    }

    public event EventHandler<TokenizerStatus>? OnStatusChanged;

    public event EventHandler<SqlRowsCopiedEventArgs>? OnSqlRowsCopied;

    public event EventHandler<double>? OnPercentCompleted;

    public async Task Tokenize(CancellationToken token = default)
    {
        OnStatusChanged?.Invoke(this, TokenizerStatus.Tokenizing);

        var entityType = db.Model.FindEntityType(typeof(IngredientSnapshot));
        var tableName = entityType?.GetSchemaQualifiedTableName();

        using SqlConnection sqlConnection = new(db.Database.GetConnectionString());
        await sqlConnection.OpenAsync(token);

        using var bcp = new SqlBulkCopy(sqlConnection)
        {
            BulkCopyTimeout = 0,
            DestinationTableName = tableName,
            NotifyAfter = 1000,
        };
        bcp.SqlRowsCopied += SqlRowsCopied;

        total = await db.IngredientSnapshots.CountAsync(token);

        var embeddings = db.IngredientSnapshots
            .AsNoTracking()
            .AsEnumerable()
            .SelectMany(ingredientSnapshot =>
            {
                var labels = SnapshotLabeler.Label(ingredientSnapshot);
                OnPercentCompleted?.Invoke(this, (double)completed++ / total);
                return labels;
            })
            .AsDataReader();
        await bcp.WriteToServerAsync(embeddings, token);

        OnStatusChanged?.Invoke(this, TokenizerStatus.Finished);
    }

    private void SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
    {
        logger.LogInformation("Created {RowsCopied} Ingredient Tokens...", e.RowsCopied);
        OnSqlRowsCopied?.Invoke(sender, e);
    }
}
