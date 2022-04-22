import os
import pickle
import pandas as pd
from datasketch import  MinHashLSHForest, MinHash

CHUNK_SIZE = 1000

def evluate_sim_w_subs():
    local_file_path = os.path.join(os.getcwd(), 'RecipeSimilarities')
    graph_name = os.path.join(local_file_path, 'outputs', 'lsh_graph_sub.pickle')
    recipe_lookup_name = os.path.join(local_file_path, 'outputs', 'recipe_lookup_w_subs.pickle')
    recipe_similarities = os.path.join(local_file_path, 'outputs', 'recipe_similarities_sub.csv')
            
    with open(recipe_lookup_name, 'rb') as handle:
        recipe_lookup = pickle.load(handle)

    with open(graph_name, 'rb') as handle:
        lsh = pickle.load(handle)

    i = 0
    for recipe_id, nutrition in recipe_lookup.items():
        result = []
        for key in lsh.query(nutrition, 10):
            f = recipe_lookup[key]
            result.append([recipe_id, key, nutrition.jaccard(recipe_lookup[key])])
        df = pd.DataFrame(result)
        df.to_csv(recipe_similarities, mode='a', index=False, header=False)
        i += 1
        if (i % 1000 == 0):
            print("Computed: " + str(i))