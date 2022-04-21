import os
import numpy as np
from transformers import BertTokenizerFast, BertForTokenClassification
import torch
from torch import cuda
from tokenize_training_dataset import dataset

MAX_LEN = 128

device = 'cuda' if cuda.is_available() else 'cpu'

token_directory = os.path.join('.', 'IngredientParser', 'BERT', 'TokenData')
tokenizer = BertTokenizerFast.from_pretrained(token_directory)

token_data_path = os.path.join(token_directory, 'training.pt')
token_data = torch.load(token_data_path)
training_set = token_data['training_set']
testing_set = token_data['testing_set']
labels_to_ids = token_data['labels_to_ids']
ids_to_labels = token_data['ids_to_labels']

model_directory = os.path.join('.', 'IngredientParser', 'BERT', 'ModelData')
model = BertForTokenClassification.from_pretrained(model_directory, num_labels=len(labels_to_ids))
model.to(device)

def predict(sentences, split_by=' '):
    inputs = tokenizer([sentence.Description.split(split_by) for sentence in sentences],
                        is_split_into_words=True, 
                        return_offsets_mapping=True, 
                        padding='max_length', 
                        truncation=True, 
                        max_length=MAX_LEN,
                        return_tensors="pt")

    # move to gpu
    ids = inputs["input_ids"].to(device)
    mask = inputs["attention_mask"].to(device)
    # forward pass
    outputs = model(ids, attention_mask=mask)
    logits = outputs[0]

    # active_logits = logits.view(-1, model.num_labels) # shape (batch_size * seq_len, num_labels)
    # flattened_predictions = torch.argmax(active_logits, axis=1) # shape (batch_size*seq_len,) - predictions at the token level
    
    pred = logits.cpu().detach().numpy()

    predictions = []
    for i in range(pred.shape[0]):
        tokens = tokenizer.convert_ids_to_tokens(ids[i].squeeze().tolist())
        # token_predictions = [ids_to_labels[i] for i in pred]
        token_predictions = [ids_to_labels[j] for j in np.argmax(pred[i], axis=1)]
        wp_preds = list(zip(tokens, token_predictions)) # list of tuples. Each tuple = (wordpiece, prediction)

        prediction = []
        for token_pred, mapping in zip(wp_preds, inputs["offset_mapping"][i].squeeze().tolist()):
            #only predictions on first word pieces are important
            if mapping[0] == 0 and mapping[1] != 0:
                prediction.append(token_pred[1])
            else:
                continue
        predictions.append((sentences[i], prediction))
    
    return predictions
    # active_logits = logits.view(-1, model.num_labels) # shape (batch_size * seq_len, num_labels)
    # flattened_predictions = torch.argmax(active_logits, axis=1) # shape (batch_size*seq_len,) - predictions at the token level

    # tokens = tokenizer.convert_ids_to_tokens(ids.squeeze().tolist())
    # token_predictions = [ids_to_labels[i] for i in flattened_predictions.cpu().numpy()]
    # wp_preds = list(zip(tokens, token_predictions)) # list of tuples. Each tuple = (wordpiece, prediction)

    # prediction = []
    # for token_pred, mapping in zip(wp_preds, inputs["offset_mapping"].squeeze().tolist()):
    #     #only predictions on first word pieces are important
    #     if mapping[0] == 0 and mapping[1] != 0:
    #         prediction.append(token_pred[1])
    #     else:
    #         continue

    # return prediction

inputs = tokenizer("1/2 cucumber, seeded, chopped",
                        return_offsets_mapping=True, 
                        padding='max_length', 
                        truncation=True, 
                        max_length=MAX_LEN)

from collections import namedtuple
Ingredient = namedtuple('Ingredient', 'Description')
# sentences = "1 ripe tomato, chopped".split()
print(predict([Ingredient("1/2 cucumber, seeded, chopped")]))
# print(predict([Ingredient("CUCUMBER,PEELED,RAW")], ','))
# print(predict([Ingredient("PICKLES,CUCUMBER,DILL,RED NA")], ','))
# print(predict([Ingredient("CUCUMBER,WITH PEEL,RAW")], ','))
# print()
# print(predict([Ingredient("2 stalks celery, chopped coarse")]))