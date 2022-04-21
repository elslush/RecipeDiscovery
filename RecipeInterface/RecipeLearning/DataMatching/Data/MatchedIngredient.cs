namespace RecipeLearning.DataMatching.Data;

public class MatchedIngredient
{
	public int MatchedIngredientID { get; set; }

	public int IngredientID { get; set; }

	public int NutritionID { get; set; }

	public double Probability { get; set; }
}
