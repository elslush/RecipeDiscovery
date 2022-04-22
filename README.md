# Discovering Healthier Recipes

With the rise of the internet, sharing recipes has never been easier. Sifting through the  billions of recipes online can be tedious and troublesome task, especially if your goal is to compare nutritional content. This project aims to fix this issue by providing an easy and robust method for consuming and comparing recipes at a large scale. 

This system can be broken up into 5 distinct sections: 

1. data collection: we utilize SQL bulk loading techniques and SQL CLR-computed columns to quickly gigabytes of recipes, nutrition, and ingredient substitution data.
2/3. Data parsing/ingredient matching: We use state-of-the-art machine learning techniques to parse and match this data together, allowing for easy access to every recipes' nutritional content.
4. Similarity measurement: we employs probabilistic similarity metrics to efficiently determine any number of similar recipes on a large scale, utilizing ingredient substitutions to boost our similarity measurements.
5. Recipe comparison: We utilize the flexibility of SQL to efficiently compare the nutrition between similar recipes.

Our results find that our system can ultimately discover healthier recipe alternatives in regards to many different nutritional measurements such as less sugar, more protein, and a higher NRF index ([https://pubmed.ncbi.nlm.nih.gov/20368382/](https://pubmed.ncbi.nlm.nih.gov/20368382/)). More information can be found in the attached report.

## How to run
This program is split between C# and Python. Therefore the steps to replicate are split between two command lines.
An SQL Server instance that allows for unsafe assemblies is also needed and can be specified in the appsettings.json file.

C# steps are accessed after you build RecipeControlPanel.csproj. Then you just run the executable (ex. RecipeControlPanel.exe).
Python steps are access using the capstone.py file in the form of "capstone.py <Step #>"

Steps to replicate are below:

1. Step 1 (C#): Import Recipes into database
2. Step 2 (C#): Import Ingredients into database
3. Step 3 (C#): Import Instructions into database
4. Step 4 (C#): Import Nutrition into database
5. Step 5 (C#): Import Snapshots into database
6. Step 6 (C#): Import Tags into database
7. Step 7 (C#): Import Substitutions into database
8. Step 8 (C#): Output tokenized training data to .csv data.
9. Step 9 (C#): Output ingredients, nutrition, and substitutions to .csv to evaluate.
10. Step 10 (Python): Train Electra
11. Step 11 (Python): Evaluate Ingredients with Electra
12. Step 12 (Python): Evaluate Nutrition with Electra
13. Step 13 (Python): Evaluate Substitutions with Electra
14. Step 14 (C#): Import ingredient predictions into database
15. Step 15 (C#): Import substitution predictions into database
16. Step 16 (Python): Match Ingredients with Nutrition
17. Step 17 (Python): Match Substitutions with Nutrition
18. Step 18 (C#): Import Matched Ingredients
19. Step 19 (C#): Output recipe ingredient vectors to .csv
20. Step 20 (Python): Create MinHashes from Ingredient Vectors (without substitutions)
21. Step 21 (Python): Create MinHashes from Ingredient Vectors (with substitutions)
22. Step 22 (Python): Create LSH Forest from MinHashes (without substitutions)
23. Step 23 (Python): Create LSH Forest from MinHashes (with substitutions)
24. Step 24 (Python): Compute top 10 similar recipes from LSH forest (without substitutions)
25. Step 25 (Python): Compute top 10 similar recipes from LSH forest (with substitutions)
26. Step 26 (C#): Import top 10 similar recipes to database
27. Step 27 (C#): Import top 10 similar recipes (with substitutes accounted for) to database
