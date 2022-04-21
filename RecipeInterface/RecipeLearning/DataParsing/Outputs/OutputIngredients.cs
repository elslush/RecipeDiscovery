using Microsoft.EntityFrameworkCore;
using RecipeLearning.Data;
using Sylvan.Data;
using Sylvan.Data.Csv;

namespace RecipeLearning.DataParsing.Outputs;

public class OutputIngredients
{
    internal const int ChunkSize = 128;

    private readonly RecipeContext db;

    public OutputIngredients(RecipeContext db)
    {
        this.db = db;
    }

    public EventHandler<double>? OnPercentCompleted;

    public async Task Output(CancellationToken token = default)
    {
        var totalCount = await db.Ingredients.CountAsync(token);

        int i = 0;
        var recordReader = db.Ingredients.
            Select(i => new { Id = i.IngredientID, Text = i.CleanedInput.Replace("\"", "") })
            .AsNoTracking()
            .AsEnumerable()
            .Select(v =>
            {
                OnPercentCompleted?.Invoke(this, (double)i / totalCount);
                i++;
                return v;
            })
            .AsDataReader();

        using var csvWriter = CsvDataWriter.Create("ingredient_output.csv");
        await csvWriter.WriteAsync(recordReader, token);
    }
}
