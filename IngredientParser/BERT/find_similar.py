import os
import pickle
import pandas as pd
from datasketch import  MinHash, MinHashLSH

CHUNK_SIZE = 1000

local_file_path = os.path.join(os.getcwd(), 'IngredientParser', 'BERT')
recipe_nutrition_name = os.path.join(local_file_path, 'recipe_nutrition.json')
recipe_nutrition_lines_name = os.path.join(local_file_path, 'recipe_nutrition_lines.json')
graph_name = os.path.join(local_file_path, 'lsh_graph.pickle')

if not os.path.isfile(recipe_nutrition_lines_name):
    df = pd.read_json(recipe_nutrition_name, encoding='utf-8')
    df.to_json(recipe_nutrition_lines_name, orient='records', lines=True)

def main():
    lsh = MinHashLSH(threshold=0.8, num_perm=128, storage_config={
            'type': 'redis',
            'basename': b'recipes_with_subs_61245',
            'redis': {'host': 'localhost', 'port': 6379},
        })

    i = 0
    records = pd.read_json(recipe_nutrition_lines_name, encoding='utf-8', orient='records', lines=True, chunksize=CHUNK_SIZE)
    for chunk in records:
        # if i < 1025000:
        #     i += CHUNK_SIZE
        #     continue
        chunk = chunk.to_numpy()
        with lsh.insertion_session() as session:
            for row in chunk:
                m = MinHash(num_perm=128)
                for d in row[1]:
                    m.update(str(d).encode('utf8'))
                session.insert(row[0], m)
                i += 1
                if (i % 1000 == 0):
                    print("Hashed: " + str(i))

    print("Saving...")
    with open(graph_name, 'wb') as handle:
        pickle.dump(lsh, handle, protocol=pickle.HIGHEST_PROTOCOL)

if __name__ == "__main__":
    main()

# # Check for membership using the key
# print("m2" in lshensemble)
# print("m3" in lshensemble)

# # Using m1 as the query, get an result iterator
# print("Sets with containment > 0.8:")
# print(m1.jaccard(m2))
# print(m1.jaccard(m3))
# for key in lshensemble.query(m1, len(set1)):
#     print(m1.jaccard(values[key][0]))