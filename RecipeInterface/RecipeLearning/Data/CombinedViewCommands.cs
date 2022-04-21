using System.Data.SqlClient;

namespace RecipeLearning.Data;

internal static class CombinedViewCommands
{
    private const string ingredientCommand = @"
CREATE VIEW [RecipeSimilarity].[CombinedIngredients] WITH SCHEMABINDING
 AS 
SELECT i.IngredientID, i.Description, i.CleanedInput, i.RecipeID, pari.Name, pari.Quantity, pari.Unit, pari.Comment, pari.Other, mi.NutritionID, mi.Probability
FROM DataCollection.Ingredients i
INNER JOIN DataMatching.MatchedIngredients mi ON i.IngredientID = mi.IngredientID
INNER JOIN DataParsing.ParsedIngredients pari ON i.IngredientID = pari.IngredientID
",
		ingredientIndexCommand = @"CREATE UNIQUE CLUSTERED INDEX [IX_CombinedIngredients] ON [RecipeSimilarity].[CombinedIngredients] ( [IngredientID] )";

	public static async Task CreateCombinedViews(string? sqlConnectionString, CancellationToken token = default)
	{
		using SqlConnection sqlConnection = new(sqlConnectionString);
		await sqlConnection.OpenAsync(token);

		using SqlCommand ingredientView = new(ingredientCommand, sqlConnection);
		using SqlCommand ingredientIndex = new(ingredientIndexCommand, sqlConnection);

		await ingredientView.ExecuteNonQueryAsync(token);
		await ingredientIndex.ExecuteNonQueryAsync(token);
	}
}
