namespace RecipeLearning.DataCollection.Data;

public class IngredientSnapshot
{
	public int Id { get; set; }
	public string Input { get; set; } = string.Empty;
	public string? Name { get; set; }
	public string? Quantity { get; set; }
	public string? RangeEnd { get; set; }
	public string? Unit { get; set; }
	public string? Comment { get; set; }
	public string CleanedInput { get; set; } = string.Empty;
}
