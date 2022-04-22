import os
import pickle
import pandas as pd
from datasketch import  MinHashLSHForest, MinHash

CHUNK_SIZE = 1000

def evluate_sim():
    local_file_path = os.path.join(os.getcwd(), 'RecipeSimilarities')
    graph_name = os.path.join(local_file_path, 'outputs', 'lsh_graph.pickle')
    recipe_lookup_name = os.path.join(local_file_path, 'outputs', 'recipe_lookup.pickle')
    recipe_similarities = os.path.join(local_file_path, 'outputs', 'recipe_similarities.csv')

    records = pd.read_csv(recipe_similarities)
    records = records.to_numpy()
    i = 0
    for r in records:
        
        if not r[1] or r[1] == '':
            break
        i += 1
            
    with open(recipe_lookup_name, 'rb') as handle:
        recipe_lookup = pickle.load(handle)

    with open(graph_name, 'rb') as handle:
        lsh = pickle.load(handle)

    i = 0
    for recipe_id, nutrition in recipe_lookup.items():
        result = []
        for key in lsh.query(nutrition, 10):
            result.append([recipe_id, key, nutrition.jaccard(recipe_lookup[key])])
        df = pd.DataFrame(result)
        df.to_csv(recipe_similarities, mode='a', index=False, header=False)
        i += 1
        if (i % 1000 == 0):
            print("Computed: " + str(i))