namespace RecipeLearning.RecipeSimilarities.Data;

public class RecipeSimilarity
{
	public int RecipeSimilarityID { get; set; }

	public Guid RecipeID { get; set; }

	public Guid SimilarRecipeID { get; set; }

	public float Jaccard { get; set; }

	public bool UsingSubstitution { get; set; }
}
