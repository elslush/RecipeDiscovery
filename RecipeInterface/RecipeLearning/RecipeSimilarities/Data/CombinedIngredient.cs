﻿namespace RecipeLearning.RecipeSimilarities.Data;

internal class CombinedIngredient
{
	public int IngredientID { get; set; }

	public string Description { get; set; } = string.Empty;

	public string CleanedInput { get; set; } = string.Empty;

	public string? Name { get; set; }

	public float Quantity { get; set; }

	public string? Unit { get; set; }

	public string? Comment { get; set; }

	public string? Other { get; set; }

	public int NutritionID { get; set; }

	public double Probability { get; set; }
}