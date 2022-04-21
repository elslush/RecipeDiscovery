using Microsoft.EntityFrameworkCore;
using RecipeLearning.Data;
using Sylvan.Data;
using System.Text.Json;

namespace RecipeLearning.RecipeSimilarities.Outputs;

public class OutputRecipeLLVM
{
    private static readonly string fileName = Path.Combine(Environment.CurrentDirectory, "recipe_nutrition.json");
    private readonly RecipeContext db;

    public OutputRecipeLLVM(RecipeContext db)
    {
        this.db = db;
    }

    public EventHandler<double>? OnPercentCompleted;

    public async Task Output(CancellationToken token = default)
    {
        var count = await db.Nutritions.CountAsync(token);
        var format = string.Join(' ', Enumerable.Range(0, count).Select(i => $"{i+1}:{{{i}}}").Prepend("0"));

        IReadOnlyDictionary<int, int> nutritionToIndex = db.Nutritions
                .AsEnumerable()
                .Select((nutrition, i) => new { nutrition.NutritionID, i })
                .ToDictionary(val => val.NutritionID, val => val.i);
        IReadOnlyDictionary<int, int> indexToNutrition = nutritionToIndex.ToDictionary(x => x.Value, x => x.Key);

        var totalCount = await db.Recipes.CountAsync(token);
        int i = 0;
        var recipes = db.Recipes
            .Include(recipe => recipe.Ingredients)
            .AsNoTracking()
            .AsEnumerable()
            .Select(recipe =>
            {
                OnPercentCompleted?.Invoke(this, (double)i++ / totalCount);
                return new KeyValuePair<Guid, int[]>(recipe.RecipeID, recipe.Ingredients.Select(ingredient => 0/*(int)ingredient.NutritionID!*/).ToArray());
            });

        JsonSerializerOptions options = new()
        {
            WriteIndented = false,
        };
        using FileStream createStream = File.Create(fileName);
        await JsonSerializer.SerializeAsync(createStream, recipes, options: options, cancellationToken: token);
        await createStream.DisposeAsync();
    }
}
