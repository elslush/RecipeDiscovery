using RecipeLearning.RecipeSimilarities.Data;

namespace RecipeLearning.DataCollection.Data;

public class Recipe
{
	public Guid RecipeID { get; set; }

	public string Name { get; set; } = string.Empty;

	public string? Url { get; set; }

	public ICollection<Ingredient> Ingredients { get; set; } = default!;

	public ICollection<Instruction> Instructions { get; set; } = default!;

	public ICollection<CombinedIngredient> CombinedIngredients { get; set; } = default!;
}
