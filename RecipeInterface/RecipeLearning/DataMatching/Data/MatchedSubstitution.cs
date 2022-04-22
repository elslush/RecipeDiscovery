namespace RecipeLearning.DataMatching.Data;

public class MatchedSubstitution
{
	public int MatchedSubstitutionID { get; set; }

	public int Substitution1ID { get; set; }

	public int Nutrition1ID { get; set; }

	public double Probability1 { get; set; }

	public int Substitution2ID { get; set; }

	public int Nutrition2ID { get; set; }

	public double Probability2 { get; set; }
}
