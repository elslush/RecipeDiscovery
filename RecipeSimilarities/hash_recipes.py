import os
import pickle
import pandas as pd
from datasketch import  MinHash

CHUNK_SIZE = 1000

def hash():
    local_file_path = os.path.join(os.getcwd(), 'RecipeSimilarities')
    recipe_nutrition_name = os.path.join(local_file_path, 'inputs', 'recipe_nutrition.json')
    recipe_nutrition_lines_name = os.path.join(local_file_path, 'inputs', 'recipe_nutrition_lines.json')
    recipe_lookup_name = os.path.join(local_file_path, 'outputs', 'recipe_lookup.pickle')

    if not os.path.isfile(recipe_nutrition_lines_name):
        df = pd.read_json(recipe_nutrition_name, encoding='utf-8')
        df.to_json(recipe_nutrition_lines_name, orient='records', lines=True)

    recipe_lookup = {}

    records = pd.read_json(recipe_nutrition_lines_name, encoding='utf-8', orient='records', lines=True, chunksize=CHUNK_SIZE)
    i = 0
    for chunk in records:
        chunk = chunk.to_numpy()
        for row in chunk:
            m = MinHash(num_perm=128)
            for d in row[1]:
                m.update(str(d).encode('utf8'))
            recipe_lookup[row[0]] = m
            i += 1
            if (i % 1000 == 0):
                print("Hashed: " + str(i))

    with open(recipe_lookup_name, 'wb') as handle:
            pickle.dump(recipe_lookup, handle, protocol=pickle.HIGHEST_PROTOCOL)