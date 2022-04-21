import os
import pandas as pd
import numpy as np
import onnxruntime as rt
from transformers import BertTokenizerFast
from sklearn.metrics.pairwise import cosine_similarity
import re
import os.path

CHUNK_SIZE = 256

def match_ingredients():
    local_file_path = os.path.join(os.getcwd(), 'DataMatching')

    token_directory = os.path.join(os.getcwd(), 'IngredientParsing', 'Electra', 'inputs', 'TokenData')
    tokenizer = BertTokenizerFast.from_pretrained(token_directory)

    onnx_model_path = os.path.join(local_file_path, 'inputs', 'embeddings.onnx')

    sess_options = rt.SessionOptions()
    sess_options.graph_optimization_level = rt.GraphOptimizationLevel.ORT_ENABLE_ALL
    sess_options.enable_profiling = True

    providers = [('CUDAExecutionProvider', {"cudnn_conv_use_max_workspace": '1','do_copy_in_default_stream': True})]
    session = rt.InferenceSession(onnx_model_path, providers=providers, sess_options=sess_options)

    def get_correct_tokens1(input):
        tokens1 = tokenizer([re.split('[ ,]',str(sentence)) for sentence in input if sentence and not str.isspace(str(sentence))], 
                                is_split_into_words=True, 
                                padding='max_length', 
                                truncation=True, 
                                max_length=128)
        tokens1['input_ids'] = np.array(tokens1.pop('input_ids'), dtype=np.int64)
        tokens1['token_type_ids'] = np.array(tokens1.pop('token_type_ids'), dtype=np.int64)
        tokens1['attention_mask'] = np.array(tokens1.pop('attention_mask'), dtype=np.int64)
        return tokens1

    sentence_embeddings = pd.read_csv(os.path.join(local_file_path, 'inputs', 'nutrition_sentence_embeddings.csv')).to_numpy()
    sentences = pd.read_csv(os.path.join(local_file_path, 'inputs', 'nutrition_sentences.csv')).to_numpy()
    ids = pd.read_csv(os.path.join(local_file_path, 'inputs', 'nutrition_ids.csv')).to_numpy()

    ingredient_csv_name = os.path.join(os.getcwd(), 'IngredientParsing', 'Electra', 'outputs', 'ingredient_predictions.csv')
    ingredient_matches_csv_name = os.path.join(local_file_path, 'outputs', 'ingredient_matches_output.csv')

    amount_matched = -1
    with open(ingredient_matches_csv_name,'r') as f:
        for line in f:
            amount_matched += 1

    print("Already Matched: " + str(amount_matched))

    reader = pd.read_csv(ingredient_csv_name, chunksize=CHUNK_SIZE, skiprows=amount_matched)
    for chunk in reader:
        chunk = chunk.to_numpy()
        sub_ids = chunk[:,0]
        
        sub_values1 = chunk[:,1]
        tokens1 = get_correct_tokens1(sub_values1)
        embeddings = session.run(None, dict(tokens1))[0]
        similarity = cosine_similarity(embeddings, sentence_embeddings)
        match_args = np.argmax(similarity, axis=1)
        match1 = np.atleast_1d(np.squeeze(sentences[match_args]))
        match_ids1 = np.atleast_1d(np.squeeze(ids[match_args]))
        probabilities1 = similarity[np.array(range(len(sub_values1))), match_args]

        result = np.stack((sub_ids.T, sub_values1.T, match1.T, match_ids1.T, probabilities1.T), axis=1)
        df = pd.DataFrame(result)
        df.to_csv(ingredient_matches_csv_name, mode='a', index=False, header=False)

        amount_matched += CHUNK_SIZE
        print("Matched: " + str(amount_matched))