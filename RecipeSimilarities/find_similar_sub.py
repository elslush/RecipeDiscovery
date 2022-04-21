import os
import pickle
from datasketch import  MinHashLSHForest

CHUNK_SIZE = 1000

def compute_similar_w_sub():
    local_file_path = os.path.join(os.getcwd(), 'RecipeSimilarities')
    graph_name = os.path.join(local_file_path, 'outputs', 'lsh_graph_sub.pickle')
    recipe_lookup_name = os.path.join(local_file_path, 'outputs', 'recipe_lookup_w_subs.pickle')

    lsh = MinHashLSHForest()

    with open(recipe_lookup_name, 'rb') as handle:
        recipe_lookup = pickle.load(handle)
    i = 0
    for recipe_id, nutrition in recipe_lookup.items():
        lsh.add(recipe_id, nutrition)
        i += 1
        if (i % 1000 == 0):
            print("Added: " + str(i))

    print("Indexing...")
    lsh.index()
    print("Saving...")
    with open(graph_name, 'wb') as handle:
        pickle.dump(lsh, handle, protocol=pickle.HIGHEST_PROTOCOL)
    print("Finished...")