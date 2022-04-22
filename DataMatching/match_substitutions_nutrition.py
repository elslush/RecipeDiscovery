import os
import pandas as pd
import numpy as np
import onnxruntime as rt
from transformers import BertTokenizerFast
from sklearn.metrics.pairwise import cosine_similarity
import re
import os.path

CHUNK_SIZE = 256

def match_substitutions():
    local_file_path = os.path.join(os.getcwd(), 'DataMatching', 'BERT')

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


    nutrition_sentence_embeddings_csv_name = os.path.join(local_file_path, 'inputs', 'nutrition_sentence_embeddings.csv')
    prediction_csv_name = os.path.join(os.getcwd(), 'IngredientParsing', 'Electra', 'outputs', 'nutrition_predictions.csv')
    nutrition_sentences_csv_name = os.path.join(local_file_path, 'inputs', 'nutrition_sentences.csv')
    nutrition_ids_csv_name = os.path.join(local_file_path, 'inputs', 'nutrition_ids.csv')
    if not os.path.isfile(nutrition_sentence_embeddings_csv_name):
        f = open(nutrition_sentence_embeddings_csv_name, "w")
        f.close()

        f = open(nutrition_sentences_csv_name, "w")
        f.close()

        f = open(nutrition_ids_csv_name, "w")
        f.close()

        reader = pd.read_csv(prediction_csv_name, chunksize=CHUNK_SIZE, index_col=False)

        loaded_into_memory = 0
        for chunk in reader:
            chunk = chunk.to_numpy()
            chunk = np.array(list(filter(lambda v: v[1]==v[1], chunk)))
            ids = chunk[:,0]
            sentences = chunk[:,1]
            tokens1 = get_correct_tokens1(sentences)

            embeddings = session.run(None, dict(tokens1))[0]

            df = pd.DataFrame(embeddings)
            df.to_csv(nutrition_sentence_embeddings_csv_name, mode='a', index=False, header=False)

            df = pd.DataFrame(sentences)
            df.to_csv(nutrition_sentences_csv_name, mode='a', index=False, header=False)

            df = pd.DataFrame(ids)
            df.to_csv(nutrition_ids_csv_name, mode='a', index=False, header=False)

            loaded_into_memory += CHUNK_SIZE
            print('Loaded ' + str(loaded_into_memory) + ' Nutrition Sentence Embeddings')

    sentence_embeddings = pd.read_csv(nutrition_sentence_embeddings_csv_name).to_numpy()
    sentences = pd.read_csv(nutrition_sentences_csv_name).to_numpy()
    ids = pd.read_csv(nutrition_ids_csv_name).to_numpy()

    substitutions_csv_name = os.path.join(os.getcwd(), 'IngredientParsing', 'Electra', 'outputs', 'substitution_output.csv')
    substitution_matches_csv_name = os.path.join(local_file_path, 'outputs', 'substitution_matches_output.csv')

    amount_matched = 0
    reader = pd.read_csv(substitutions_csv_name, chunksize=CHUNK_SIZE)
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

        sub_values2 = chunk[:,2]
        tokens2 = get_correct_tokens1(sub_values2)
        embeddings = session.run(None, dict(tokens2))[0]
        similarity = cosine_similarity(embeddings, sentence_embeddings)
        match_args = np.argmax(similarity, axis=1)
        match2 = np.atleast_1d(np.squeeze(sentences[match_args]))
        match_ids2 = np.atleast_1d(np.squeeze(ids[match_args]))
        probabilities2 = similarity[np.array(range(len(sub_values2))), match_args]

        result = np.stack((sub_ids.T, sub_values1.T, match1.T, match_ids1.T, probabilities1.T, sub_values2.T, match2.T, match_ids2.T, probabilities2.T), axis=1)
        result_filtered = result[np.where((result[:,4] > 0.65) & (result[:,8] > 0.65) & (result[:,1] != result[:,5]))]
        df = pd.DataFrame(result_filtered)
        df.to_csv(substitution_matches_csv_name, mode='a', index=False, header=False)

        amount_matched += CHUNK_SIZE
        print("Matched: " + str(amount_matched))