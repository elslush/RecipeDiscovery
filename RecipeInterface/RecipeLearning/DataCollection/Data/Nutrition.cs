namespace RecipeLearning.DataCollection.Data;

public class Nutrition
{
	public int NutritionID { get; set; }
	public string Name { get; set; } = string.Empty;
	public double Water { get; set; }
	public double Calories { get; set; }
	public double Protein { get; set; }
	public double Fat { get; set; }
	public double Ash { get; set; }
	public double Carbohydrate { get; set; }
	public double Fiber { get; set; }
	public double Sugar { get; set; }
	public double Calcium { get; set; }
	public double Iron { get; set; }
	public double Magnesium { get; set; }
	public double Phosphorus { get; set; }
	public double Potassium { get; set; }
	public double Sodium { get; set; }
	public double Zinc { get; set; }
	public double Copper { get; set; }
	public double Manganese { get; set; }
	public double Selenium { get; set; }
	public double VitaminC { get; set; }
	public double Thiamin { get; set; }
	public double Riboflavin { get; set; }
	public double Niacin { get; set; }
	public double PantothenicAcid { get; set; }
	public double VitaminB6 { get; set; }
	public double Folate { get; set; }
	public double FolicAcid { get; set; }
	public double FoodFolate { get; set; }
	public double DietaryFolateEquiv { get; set; }
	public double Choline { get; set; }
	public double VitaminB12 { get; set; }
	public double VitaminA { get; set; }
	public double VitaminARentinolEquiv { get; set; }
	public double Rentinol { get; set; }
	public double AlphaCarotene { get; set; }
	public double BetaCarotene { get; set; }
	public double BetaCryptoxanthin { get; set; }
	public double Lycopene { get; set; }
	public double LuteinZeazathin { get; set; }
	public double VitaminE { get; set; }
	public double VitaminD { get; set; }
	public double VitaminDIU { get; set; }
	public double VitaminK { get; set; }
	public double SaturatedFat { get; set; }
	public double MonounsaturatedFat { get; set; }
	public double PolyunsaturatedFat { get; set; }
	public double Cholesterol { get; set; }
	public double HouseholdWeight1 { get; set; }
	public string? HouseholdDesc1 { get; set; }
	public double HouseholdWeight2 { get; set; }
	public string? HouseholdDesc2 { get; set; }
	public int RefusePercentage { get; set; }
	public string CleanedInput { get; set; } = string.Empty;
}
