import matplotlib.pyplot as plt
import pyodbc
import pandas as pd

cnxn = pyodbc.connect('DRIVER={ODBC Driver 17 for SQL Server};SERVER=(local);DATABASE=FoodRecipesTest;Trusted_Connection=yes;')
cursor = cnxn.cursor()

fig = plt.figure()

keys = []
values = []

for i in range(5000, 100000, 5000):
    query = """
    SET NOCOUNT ON
    DECLARE @temp TABLE (parentID uniqueidentifier, RecipeID uniqueidentifier, ProteinCalculated float)

    INSERT INTO @temp
    SELECT parentID, ingredients.RecipeID, --ingredients.Name, 
        SUM(ProteinCalculated) AS ProteinCalculated
    FROM (
    SELECT TOP """ + str(i) + """ sr.RecipeID AS parentID, r.RecipeID, r.Name,
    --Nutrition Conversions
    i.Quantity *  n.Protein as ProteinCalculated
    FROM Data.Ingredients i
    INNER JOIN Data.Nutrition n ON n.NutritionID = i.NutritionID
    INNER JOIN Data.RecipeSimilarities sr ON sr.SimilarRecipeID = i.RecipeID
        AND sr.UsingSubstitution = 0
    INNER JOIN Data.Recipes r ON r.RecipeID = sr.SimilarRecipeID
    ) as ingredients
    GROUP BY parentID, ingredients.RecipeID, ingredients.Name
    --ORDER BY ProteinCalculated

    SELECT AVG(ProteinDiff)
    FROM (
    SELECT t.parentID, MAX(tt.ProteinCalculated - t.ProteinCalculated) AS ProteinDiff
    FROM @temp t
    INNER JOIN @temp tt ON t.parentID = t.RecipeID
    GROUP BY t.parentID) ttt"""

    cursor.execute(query)

    value = cursor.fetchone()[0]
    values.append(float(value))
    keys.append(str(i))

    print('samples: ' + str(i) + ', avg: ' + str(value))

plt.bar(keys, values)
 
plt.xlabel("Samples Taken")
plt.ylabel("Average Protein (g) Increase")
plt.title("Average Protein Increase")
plt.show()
# plt.savefig('sugar_bar.png')