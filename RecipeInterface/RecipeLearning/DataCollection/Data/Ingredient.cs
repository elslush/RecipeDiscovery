namespace RecipeLearning.DataCollection.Data;

public class Ingredient
{
	public int IngredientID { get; set; }

	public string Description { get; set; } = string.Empty;

	public string CleanedInput { get; set; } = string.Empty;

	public Guid RecipeID { get; set; }

	public Recipe? Recipe { get; set; }
}
