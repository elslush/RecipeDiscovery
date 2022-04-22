using Microsoft.EntityFrameworkCore;
using RecipeLearning.Data;
using Sylvan.Data;
using Sylvan.Data.Csv;

namespace RecipeLearning.DataParsing.Outputs;

public class OutputNutrition
{
    private readonly RecipeContext db;

    public OutputNutrition(RecipeContext db)
    {
        this.db = db;
    }

    public EventHandler<double>? OnPercentCompleted;

    public async Task Output(CancellationToken token = default)
    {
        var totalCount = await db.Nutritions.CountAsync(token);

        int i = 0;
        var recordReader = db.Nutritions.
            Select(i => new { Id = i.NutritionID, Text = i.CleanedInput.Replace("\"", "") })
            .AsNoTracking()
            .AsEnumerable()
            .Select(v =>
            {
                OnPercentCompleted?.Invoke(this, (double)i / totalCount);
                i++;
                return v;
            })
            .AsDataReader();

        using var csvWriter = CsvDataWriter.Create("nutrition_output.csv");
        await csvWriter.WriteAsync(recordReader, token);
    }
}