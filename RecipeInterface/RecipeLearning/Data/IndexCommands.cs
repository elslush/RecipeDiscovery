using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeLearning.Data;

internal static class IndexCommands
{
	private const string

		createIngredientPredictionIndex = @"
CREATE NONCLUSTERED COLUMNSTORE INDEX [_dta_index_Ingredients_13_2005582183__col__] ON [RecipeSimilarity].[CombinedIngredients]
(
	[IngredientID],
	[RecipeID],
	[Description],
	[Name],
	[Quantity],
	[Unit],
	[Comment],
	[Other],
	[NutritionID]
)WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0) ON [PRIMARY]
",
		createIngredientPredictionStats = @"
CREATE STATISTICS [_dta_stat_2005582183_10_4] ON [RecipeSimilarity].[CombinedIngredients]([NutritionID], [Name])
",
		createIngredientMatchIndex = @"
CREATE NONCLUSTERED COLUMNSTORE INDEX [_dta_index_Ingredients_13_2005582183__col__] ON [RecipeSimilarity].[CombinedIngredients]
(
	[IngredientID],
	[RecipeID],
	[Description],
	[Name],
	[Quantity],
	[Unit],
	[Comment],
	[Other],
	[NutritionID],
	[Probability]
)WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0) ON [PRIMARY]

",
		createRecipeSimilarityIndex = @"
CREATE NONCLUSTERED COLUMNSTORE INDEX [_dta_index_RecipeSimilarities_13_2062630391__col__] ON [RecipeSimilarity].[RecipeSimilarities]
(
	[RecipeSimilarityID],
	[RecipeID],
	[SimilarRecipeID],
	[Jaccard],
	[UsingSubstitution]
)WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0) ON [PRIMARY]
",
		createRecipeSimilarityNonClustered = @"
CREATE NONCLUSTERED INDEX [_dta_index_RecipeSimilarities_13_2062630391__K2_K5_K3_K4_1] ON [RecipeSimilarity].[RecipeSimilarities]
(
	[RecipeID] ASC,
	[UsingSubstitution] ASC,
	[SimilarRecipeID] ASC,
	[Jaccard] ASC
)
INCLUDE([RecipeSimilarityID]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
",
		createRecipeSimilarityNonClusteredStats1 = "CREATE STATISTICS [_dta_stat_2062630391_3_2_5_4] ON [RecipeSimilarity].[RecipeSimilarities]([SimilarRecipeID], [RecipeID], [UsingSubstitution], [Jaccard])",
		createRecipeSimilarityNonClusteredStats2 = "CREATE STATISTICS [_dta_stat_2062630391_4_5_2] ON [RecipeSimilarity].[RecipeSimilarities]([Jaccard], [UsingSubstitution], [RecipeID])";

	public static async Task CreateIndicies(string? sqlConnectionString, CancellationToken token = default)
	{
		using SqlConnection sqlConnection = new(sqlConnectionString);
		await sqlConnection.OpenAsync(token);

		//using (SqlCommand ingredientPredictionIndexCommand = new(createIngredientPredictionIndex, sqlConnection))
		//	await ingredientPredictionIndexCommand.ExecuteNonQueryAsync(token);

		using (SqlCommand ingredientPredictionStatsCommand = new(createIngredientPredictionStats, sqlConnection))
			await ingredientPredictionStatsCommand.ExecuteNonQueryAsync(token);

		//using (SqlCommand ingredientMatchIndexCommand = new(createIngredientMatchIndex, sqlConnection))
		//	await ingredientMatchIndexCommand.ExecuteNonQueryAsync(token);

		using (SqlCommand recipeSimilarityIndexCommand = new(createRecipeSimilarityIndex, sqlConnection))
			await recipeSimilarityIndexCommand.ExecuteNonQueryAsync(token);

		using (SqlCommand createRecipeSimilarityNonClusteredCommand = new(createRecipeSimilarityNonClustered, sqlConnection))
			await createRecipeSimilarityNonClusteredCommand.ExecuteNonQueryAsync(token);

		using (SqlCommand createRecipeSimilarityNonClusteredStats1Command = new(createRecipeSimilarityNonClusteredStats1, sqlConnection))
			await createRecipeSimilarityNonClusteredStats1Command.ExecuteNonQueryAsync(token);

		using (SqlCommand createRecipeSimilarityNonClusteredStats1Command = new(createRecipeSimilarityNonClusteredStats2, sqlConnection))
			await createRecipeSimilarityNonClusteredStats1Command.ExecuteNonQueryAsync(token);
	}
}
