import sys
from IngredientParser.Electra.train_electra import train
from IngredientParser.Electra.evaluate_electra import evaluate_ingredients
from IngredientParser.Electra.evaluate_nutrition_electra import evaluate_nutrition
from IngredientParser.Electra.evaluate_substitutions_electra import evaluate_substitutions
from DataMatching.match_ingredients_nutrition import match_ingredients
from DataMatching.match_substitutions_nutrition import match_substitutions
from RecipeSimilarities.hash_recipes import hash
from RecipeSimilarities.hash_recipes_w_subs import hash_w_subs
from RecipeSimilarities.find_similar import compute_similar
from RecipeSimilarities.find_similar_sub import compute_similar_w_sub
from RecipeSimilarities.evaluate_similar import evluate_sim
from RecipeSimilarities.evaluate_similar_subs import evluate_sim_w_subs

if len(sys.argv) != 2:
    print("Command Line Error")
    print("Must be of form:")
    print("capstone.py <Step #>")
    print("")
    print("Possible Steps include:")
    print("Step 1: Train Electra")
    print("Step 2: Evaluate Ingredients with Electra")
    print("Step 3: Evaluate Nutrition with Electra")
    print("Step 4: Evaluate Substitutions with Electra")
    print("Step 5: Match Ingredients with Nutrition")
    print("Step 6: Match Substitutions with Nutrition")
    print("Step 7: Create MinHashes from Ingredient Vectors (without substitutions)")
    print("Step 8: Create MinHashes from Ingredient Vectors (with substitutions)")
    print("Step 9: Create LSH Forest from MinHashes (without substitutions)")
    print("Step 10: Create LSH Forest from MinHashes (with substitutions)")
    print("Step 11: Compute top 10 similar recipes from LSH forest (without substitutions)")
    print("Step 12: Compute top 10 similar recipes from LSH forest (with substitutions)")
else:
    if sys.argv[1] == '1':
        print("Training Electra..")
        train()
    elif sys.argv[1] == '2':
        print("Evaluating Ingredients with Electra..")
        evaluate_ingredients()
    elif sys.argv[1] == '3':
        print("Evaluating Nutrition with Electra..")
        evaluate_nutrition()
    elif sys.argv[1] == '4':
        print("Evaluating Substitutions with Electra..")
        evaluate_substitutions()
    elif sys.argv[1] == '5':
        print("Matching Ingredients with Nutrition..")
        match_ingredients()
    elif sys.argv[1] == '6':
        print("Matching Substitutions with Nutrition..")
        match_substitutions()
    elif sys.argv[1] == '7':
        print("Creating MinHashes from Ingredient Vectors (without substitutions)n..")
        hash()
    elif sys.argv[1] == '8':
        print("Create MinHashing from Ingredient Vectors (with substitutions)..")
        hash_w_subs()
    elif sys.argv[1] == '9':
        print("Creating LSH Forest from MinHashes (without substitutions)..")
        compute_similar()
    elif sys.argv[1] == '10':
        print("Creating LSH Forest from MinHashes (with substitutions)..")
        compute_similar_w_sub()
    elif sys.argv[1] == '11':
        print("Computing top 10 similar recipes from LSH forest (without substitutions)..")
        evluate_sim()
    elif sys.argv[1] == '12':
        print("Computing top 10 similar recipes from LSH forest (with substitutions)..")
        evluate_sim_w_subs()