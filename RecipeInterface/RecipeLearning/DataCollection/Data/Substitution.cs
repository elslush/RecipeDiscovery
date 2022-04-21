namespace RecipeLearning.DataCollection.Data;

public class Substitution
{
	public int SubstitutionID { get; set; }
	public string Substitution1 { get; set; } = string.Empty;
	public string CleanedSubstitution1 { get; set; } = string.Empty;
	public string Substitution2 { get; set; } = string.Empty;
	public string CleanedSubstitution2 { get; set; } = string.Empty;
	public string? Webpage { get; set; }
}
