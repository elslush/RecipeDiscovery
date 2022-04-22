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
    DECLARE @temp TABLE (parentID uniqueidentifier, RecipeID uniqueidentifier, SugarCalculated float)

    INSERT INTO @temp
    SELECT parentID, ingredients.RecipeID, --ingredients.Name, 
        SUM(SugarCalculated) AS SugarCalculated
    FROM (
    SELECT TOP """ + str(i) + """ sr.RecipeID AS parentID, r.RecipeID, r.Name,
    --Nutrition Conversions
    i.Quantity *  n.Sugar as SugarCalculated
    FROM Data.Ingredients i
    INNER JOIN Data.Nutrition n ON n.NutritionID = i.NutritionID
    INNER JOIN Data.RecipeSimilarities sr ON sr.SimilarRecipeID = i.RecipeID
        AND sr.UsingSubstitution = 0
    INNER JOIN Data.Recipes r ON r.RecipeID = sr.SimilarRecipeID
    ) as ingredients
    GROUP BY parentID, ingredients.RecipeID, ingredients.Name
    --ORDER BY SugarCalculated

    SELECT AVG(SugarDiff)
    FROM (
    SELECT t.parentID, MIN(tt.SugarCalculated - t.SugarCalculated) AS SugarDiff
    FROM @temp t
    INNER JOIN @temp tt ON t.parentID = t.RecipeID
    GROUP BY t.parentID) ttt"""

    cursor.execute(query)

    value = cursor.fetchone()[0]
    values.append(-float(value))
    keys.append(str(i))

    print('samples: ' + str(i) + ', avg: ' + str(value))

plt.bar(keys, values)
 
plt.xlabel("Samples Taken")
plt.ylabel("Average Sugar (g) Decrease")
plt.title("Average Sugar Decrease")
plt.show()
# plt.savefig('sugar_bar.png')