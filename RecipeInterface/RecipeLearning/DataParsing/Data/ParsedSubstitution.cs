namespace RecipeLearning.DataParsing.Data;

public class ParsedSubstitution
{
	public int ParsedSubstitutionID { get; set; }
	public int SubstitutionID { get; set; }
	public string Substitution1Name { get; set; } = string.Empty;
	public float Substitution1Quantity { get; set; }
	public string Substitution1Unit { get; set; } = string.Empty;
	public string Substitution1Comment { get; set; } = string.Empty;
	public string Substitution1Other { get; set; } = string.Empty;
	public string Substitution2Name { get; set; } = string.Empty;
	public float Substitution2Quantity { get; set; }
	public string Substitution2Unit { get; set; } = string.Empty;
	public string Substitution2Comment { get; set; } = string.Empty;
	public string Substitution2Other { get; set; } = string.Empty;
}
