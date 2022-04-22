import os
import pickle
import pandas as pd
from datasketch import  MinHash

CHUNK_SIZE = 1000

def hash_w_subs():
    local_file_path = os.path.join(os.getcwd(), 'RecipeSimilarities')
    recipe_nutrition_name = os.path.join(local_file_path, 'inputs', 'recipe_nutrition.json')
    recipe_nutrition_lines_name = os.path.join(local_file_path, 'inputs', 'recipe_nutrition_lines.json')
    recipe_lookup_name = os.path.join(local_file_path, 'outputs', 'recipe_lookup_w_subs.pickle')

    if not os.path.isfile(recipe_nutrition_lines_name):
        df = pd.read_json(recipe_nutrition_name, encoding='utf-8')
        df.to_json(recipe_nutrition_lines_name, orient='records', lines=True)

    substitutions = {}
    substitutions_name = os.path.join(local_file_path, 'substitution_matches_output.csv')
    substitution_matches = pd.read_csv(substitutions_name).to_numpy()
    substitutions_dep = {}
    total_explored = []
    for row in substitution_matches:
        sub_1 = row[3]
        sub_2 = row[7]
        if sub_1 in substitutions_dep:
            substitutions_dep[sub_1].append(sub_2)
        else:
            substitutions_dep[sub_1] = [ sub_2 ]
        if sub_2 in substitutions_dep:
            substitutions_dep[sub_2].append(sub_1)
        else:
            substitutions_dep[sub_2] = [ sub_1 ]

    for row in substitution_matches:
        if row[3] in total_explored:
            continue
        unexplored_subs = [ row[3], row[7] ]
        total_subs = []

        while len(unexplored_subs) > 0:
            sub = unexplored_subs.pop()
            total_subs.append(sub)
            if sub in substitutions_dep:
                for new_sub in substitutions_dep[sub]:
                    if new_sub not in total_subs:
                        unexplored_subs.append(new_sub)

        total_explored.extend(total_subs)
        min_sub = min(total_subs)
        for sub in total_subs:
            substitutions[sub] = min_sub

    recipe_lookup = {}

    records = pd.read_json(recipe_nutrition_lines_name, encoding='utf-8', orient='records', lines=True, chunksize=CHUNK_SIZE)
    i = 0
    for chunk in records:
        chunk = chunk.to_numpy()
        for row in chunk:
            m = MinHash(num_perm=128)
            for d in row[1]:
                if d in substitutions:
                    d = substitutions[d]
                m.update(str(d).encode('utf8'))
            recipe_lookup[row[0]] = m
            i += 1
            if (i % 1000 == 0):
                print("Hashed: " + str(i))

    with open(recipe_lookup_name, 'wb') as handle:
        pickle.dump(recipe_lookup, handle, protocol=pickle.HIGHEST_PROTOCOL)