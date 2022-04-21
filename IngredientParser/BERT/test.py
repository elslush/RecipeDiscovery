import pandas as pd
import os
import numpy as np
from sklearn.metrics import accuracy_score
from transformers import BertTokenizerFast, BertForTokenClassification
import torch
from torch import cuda
from torch.utils.data import DataLoader
from tokenize_training_dataset import dataset

VALID_BATCH_SIZE = 2

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

test_params = {'batch_size': VALID_BATCH_SIZE,
                'shuffle': True,
                'num_workers': 0
                }

testing_loader = DataLoader(testing_set, **test_params)

def valid(model, testing_loader):
    # put model in evaluation mode
    model.eval()
    
    eval_loss, eval_accuracy = 0, 0
    nb_eval_examples, nb_eval_steps = 0, 0
    eval_preds, eval_labels = [], []
    
    with torch.no_grad():
        for idx, batch in enumerate(testing_loader):
            
            ids = batch['input_ids'].to(device, dtype = torch.long)
            mask = batch['attention_mask'].to(device, dtype = torch.long)
            labels = batch['labels'].to(device, dtype = torch.long)
            
            outputs = model(input_ids=ids, attention_mask=mask, labels=labels)
            loss = outputs.loss
            eval_logits = outputs.logits
            eval_loss += loss.item()

            nb_eval_steps += 1
            nb_eval_examples += labels.size(0)
        
            if idx % 100==0:
                loss_step = eval_loss/nb_eval_steps
                print(f"Validation loss per 100 evaluation steps: {loss_step}")
              
            # compute evaluation accuracy
            flattened_targets = labels.view(-1) # shape (batch_size * seq_len,)
            active_logits = eval_logits.view(-1, model.num_labels) # shape (batch_size * seq_len, num_labels)
            flattened_predictions = torch.argmax(active_logits, axis=1) # shape (batch_size * seq_len,)
            
            # only compute accuracy at active labels
            active_accuracy = labels.view(-1) != -100 # shape (batch_size, seq_len)
        
            labels = torch.masked_select(flattened_targets, active_accuracy)
            predictions = torch.masked_select(flattened_predictions, active_accuracy)
            
            eval_labels.extend(labels)
            eval_preds.extend(predictions)
            
            tmp_eval_accuracy = accuracy_score(labels.cpu().numpy(), predictions.cpu().numpy())
            eval_accuracy += tmp_eval_accuracy

    labels = [ids_to_labels[id.item()] for id in eval_labels]
    predictions = [ids_to_labels[id.item()] for id in eval_preds]
    
    eval_loss = eval_loss / nb_eval_steps
    eval_accuracy = eval_accuracy / nb_eval_steps
    print(f"Validation Loss: {eval_loss}")
    print(f"Validation Accuracy: {eval_accuracy}")

    validation_data_path = os.path.join(model_directory, 'validation_data.json')
    df = pd.DataFrame({'labels': labels,
                   'predictions': predictions,
                   'eval_loss': eval_loss,
                   'eval_accuracy': eval_accuracy})
    df.to_json(validation_data_path, indent=4)

    return labels, predictions

labels, predictions = valid(model, testing_loader)

from seqeval.metrics import classification_report

classification_report_path = os.path.join(model_directory, 'classification_report.csv')
df = pd.DataFrame(classification_report([labels], [predictions], output_dict=True))
df.to_csv(classification_report_path)