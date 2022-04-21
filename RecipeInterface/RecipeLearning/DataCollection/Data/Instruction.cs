namespace RecipeLearning.DataCollection.Data;

public class Instruction
{
	public int InstructionId { get; set; }

	public string? Text { get; set; }

	public int Sequence { get; set; }

	public Guid? RecipeID { get; set; }

	public Recipe? Recipe { get; set; }
}
