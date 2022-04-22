namespace RecipeLearning.DataParsing.Data;

public class ParsedIngredient
{
	public int ParsedIngredientID { get; set; }
	public int IngredientID { get; set; }
	public string? Name { get; set; }
	public float Quantity { get; set; }
	public string? Unit { get; set; }
	public string? Comment { get; set; }
	public string? Other { get; set; }
}
