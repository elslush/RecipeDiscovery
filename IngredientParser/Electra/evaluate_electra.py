import os
import pandas as pd
import numpy as np
from transformers import BertTokenizerFast
import onnxruntime as rt

CHUNK_SIZE = 1024

def evaluate_ingredients():
    local_file_path = os.path.join(os.getcwd(), 'IngredientParsing', 'Electra')

    predictions_to_ind = [-1, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6 ]
    prediction_csv_name = os.path.join(local_file_path, 'outputs', 'ingredient_predictions.csv')

    already_processed = -1
    with open(prediction_csv_name,'r') as f:
        for line in f:
            already_processed += 1

    token_directory = os.path.join(local_file_path, 'inputs', 'TokenData')
    tokenizer = BertTokenizerFast.from_pretrained(token_directory)

    onnx_model_path = os.path.join(local_file_path, 'outputs', 'model.onnx')

    sess_options = rt.SessionOptions()
    sess_options.graph_optimization_level = rt.GraphOptimizationLevel.ORT_ENABLE_ALL
    sess_options.enable_profiling = True

    providers = [('CUDAExecutionProvider', {"cudnn_conv_use_max_workspace": '1','do_copy_in_default_stream': True}), 'CPUExecutionProvider']
    m = rt.InferenceSession(onnx_model_path, providers=providers, sess_options=sess_options)

    print("Already Processed: " + str(already_processed))

    filename = os.path.join(local_file_path, 'inputs', "ingredient_output.csv")
    reader = pd.read_csv(filename, chunksize=CHUNK_SIZE, skiprows=already_processed)
    for chunk in reader:
        chunk = chunk.to_numpy()
        texts = [str(sentence).split(' ') for sentence in chunk[:,1]]
        ids = chunk[:,0]

        tokens = tokenizer(texts,
                                is_split_into_words=True, 
                                return_offsets_mapping=True, 
                                padding='max_length', 
                                truncation=True, 
                                max_length=128)

        tokens['input_1'] = np.array(tokens.pop('input_ids'), dtype=np.int32)
        tokens['input_2'] = np.array(tokens.pop('token_type_ids'), dtype=np.int32)
        tokens['input_3'] = np.array(tokens.pop('attention_mask'), dtype=np.int32)
        offset_mapping = np.array(tokens.pop('offset_mapping'))

        onnx_pred = np.array(m.run(["dense"], dict(tokens)))[0]

        pred  = np.argmax(onnx_pred, axis=2)

        predictions = np.full((len(texts), 7), '', dtype="U150")
        for i in range(pred.shape[0]):
            predictions[i,0] = ids[i]
            j = 0
            for token_pred, mapping in zip(pred[i], offset_mapping[i].squeeze().tolist()):
                prediction = predictions_to_ind[token_pred]
                if mapping[0] == 0 and mapping[1] != 0:
                    if (len(predictions[i, prediction]) > 0):
                        predictions[i, prediction] = str(predictions[i, prediction]) + ' ' + texts[i][j]
                    else:
                        predictions[i, prediction] = texts[i][j]
                    j += 1
                else:
                    continue

        df = pd.DataFrame(predictions)
        df.to_csv(prediction_csv_name, mode='a', index=False, header=False)

        already_processed += CHUNK_SIZE
        print("Processed Total: " + str(already_processed))