using Microsoft.EntityFrameworkCore;
using RecipeLearning.Data;
using Sylvan.Data;
using Sylvan.Data.Csv;

namespace RecipeLearning.DataParsing.Outputs;

public class OutputSubstitutions
{
    private readonly RecipeContext db;

    public OutputSubstitutions(RecipeContext db)
    {
        this.db = db;
    }

    public EventHandler<double>? OnPercentCompleted;

    public async Task Output(CancellationToken token = default)
    {
        var totalCount = await db.Substitutions.CountAsync(token);

        int i = 0;
        var recordReader = db.Substitutions.
            Select(i => new { Id = i.SubstitutionID, Substitution1 = i.CleanedSubstitution1.Replace("\"", ""), Substitution2 = i.CleanedSubstitution2.Replace("\"", "") })
            .AsNoTracking()
            .AsEnumerable()
            .Select(v =>
            {
                OnPercentCompleted?.Invoke(this, (double)i / totalCount);
                i++;
                return v;
            })
            .AsDataReader();

        using var csvWriter = CsvDataWriter.Create("substitution_output.csv");
        await csvWriter.WriteAsync(recordReader, token);
    }
}
