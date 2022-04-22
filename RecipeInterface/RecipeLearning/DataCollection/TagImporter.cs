using Google.Apis.Download;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipeLearning.Data;
using RecipeLearning.DataCollection.Data;
using RecipeLearning.Import;
using RecipeLearning.Import.Streams;
using System.Collections.ObjectModel;
using System.Data.SqlClient;

namespace RecipeLearning.DataCollection;

public class TagImporter : IImporter
{
    private static readonly string[] tagNames = { 
        nameof(IngredientSnapshot.Name).ToUpper(), 
        nameof(IngredientSnapshot.Unit).ToUpper(),
        nameof(IngredientSnapshot.Quantity).ToUpper(),
        nameof(IngredientSnapshot.Comment).ToUpper(),
        nameof(IngredientSnapshot.RangeEnd).ToUpper(),
        "OTHER",
    };
    private static readonly ReadOnlyCollection<IngredientTag> tags = new(GetIngredientTags(tagNames).ToList());
    private static readonly ReadOnlyDictionary<string, int> tagToId = new(tags.ToDictionary(x => x.Tag, x => x.Id));
    private static readonly ReadOnlyDictionary<int, string> idToTag = new(tags.ToDictionary(x => x.Id, x => x.Tag));

    private static IEnumerable<IngredientTag> GetIngredientTags(string[] tagNames)
    {
        int i = 1;
        foreach (var name in tagNames)
        {
            yield return new() { Id = i++, Tag = $"B-{name}" };
            yield return new() { Id = i++, Tag = $"I-{name}" };
        }
    }

    private readonly RecipeContext db;
    private readonly ILogger logger;

    public event EventHandler<ImportStatus>? OnStatusChanged;
    public event EventHandler<SqlRowsCopiedEventArgs>? OnSqlRowsCopied;
    public event EventHandler<ProgressChangedEventArgs>? OnStreamProgressChanged;
#pragma warning disable CS0067
    public event EventHandler<IDownloadProgress>? OnDownloadProgressChanged;
#pragma warning restore CS0067

    public TagImporter(RecipeContext db, ILogger logger)
    {
        this.db = db;
        this.logger = logger;
    }

    public static ReadOnlyDictionary<string, int> TagToId => tagToId;

    public static ReadOnlyDictionary<int, string> IdToTag => idToTag;

    public async Task Import(CancellationToken token = default)
    {
        if (!await db.IngredientTags.AnyAsync(token))
        {
            OnSqlRowsCopied?.Invoke(this, new(tags.Count));
            OnStreamProgressChanged?.Invoke(this, new(0));
            logger.LogInformation("Importing Tags...");
            OnStatusChanged?.Invoke(this, ImportStatus.Copying);

            await db.IngredientTags.AddRangeAsync(tags, token);
            await db.SaveChangesAsync(token);

            OnSqlRowsCopied?.Invoke(this, new(tags.Count));
            OnStreamProgressChanged?.Invoke(this, new(1));
            OnStatusChanged?.Invoke(this, ImportStatus.Finished);
            logger.LogInformation("Tags Imported...");
        }
    }
}
