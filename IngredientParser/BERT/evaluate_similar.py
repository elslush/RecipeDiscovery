import os
import pickle
import pandas as pd
from datasketch import  MinHash, MinHashLSH

local_file_path = os.path.join(os.getcwd(), 'IngredientParser', 'BERT')
graph_name = os.path.join(local_file_path, 'lsh_graph.pickle')

with open(graph_name, 'rb') as handle:
    lsh = pickle.load(handle)

set1 = set([18988,
1057,
1011,
2032,
2047,
2015,
11413,
16104,
1042])

m1 = MinHash(num_perm=128)
for d in set1:
    m1.update(str(d).encode('utf8'))

# Using m1 as the query, get an result iterator
print("Sets with containment > 0.8:")
# print(m1.jaccard(m2))
# print(m1.jaccard(m3))
i = 0
for key in lsh.query(m1, len(set1)):
    print(key)
    i += 1
    if i > 10:
        break