import pandas as pd
from sklearn.metrics import accuracy_score
import torch
from torch.utils.data import DataLoader
from transformers import BertForTokenClassification
import torch
from torch import cuda
import os
from tokenize_training_dataset import dataset

# inspiration taken from
# https://github.com/abhimishra91/transformers-tutorials
# https://colab.research.google.com/github/NielsRogge/Transformers-Tutorials/blob/master/BERT/Custom_Named_Entity_Recognition_with_BERT_only_first_wordpiece.ipynb#scrollTo=zPDla1mmZiax

TRAIN_BATCH_SIZE = 4
EPOCHS = 5
LEARNING_RATE = 1e-05
MAX_GRAD_NORM = 10

device = 'cuda' if cuda.is_available() else 'cpu'

token_data_path = os.path.join('.', 'IngredientParser', 'BERT', 'TokenData', 'training.pt')
token_data = torch.load(token_data_path)
training_set = token_data['training_set']
testing_set = token_data['testing_set']
labels_to_ids = token_data['labels_to_ids']
ids_to_labels = token_data['ids_to_labels']

train_params = {'batch_size': TRAIN_BATCH_SIZE,
                'shuffle': True,
                'num_workers': 0
                }

training_loader = DataLoader(training_set, **train_params)

model_directory = os.path.join('.', 'IngredientParser', 'BERT', 'ModelData')
model_metadata_path = os.path.join(model_directory, 'metadata.pt')

if not os.path.exists(model_directory):
    os.makedirs(model_directory)

    model = BertForTokenClassification.from_pretrained('bert-base-uncased', num_labels=len(labels_to_ids))
    model.to(device)

    optimizer = torch.optim.Adam(params=model.parameters(), lr=LEARNING_RATE)
    epoch_start = 0
else:
    model = BertForTokenClassification.from_pretrained(model_directory, num_labels=len(labels_to_ids))
    model.to(device)

    optimizer = torch.optim.Adam(params=model.parameters(), lr=LEARNING_RATE)

    model_metadata = torch.load(model_metadata_path)
    loss = model_metadata['loss']
    epoch_start = model_metadata['epoch']
    optimizer.load_state_dict(model_metadata['optimizer_state_dict'])

inputs = training_set[2]
input_ids = inputs["input_ids"].unsqueeze(0)
attention_mask = inputs["attention_mask"].unsqueeze(0)
labels = inputs["labels"].unsqueeze(0)

input_ids = input_ids.type(torch.LongTensor).to(device)
attention_mask = attention_mask.type(torch.LongTensor).to(device)
labels = labels.type(torch.LongTensor).to(device)

outputs = model(input_ids, attention_mask=attention_mask, labels=labels)

def train(epoch):
    tr_loss, tr_accuracy = 0, 0
    nb_tr_examples, nb_tr_steps = 0, 0
    tr_preds, tr_labels = [], []
    # put model in training mode
    model.train()
    
    for idx, batch in enumerate(training_loader):
        
        ids = batch['input_ids'].to(device, dtype = torch.long)
        mask = batch['attention_mask'].to(device, dtype = torch.long)
        labels = batch['labels'].to(device, dtype = torch.long)

        outputs = model(input_ids=ids, attention_mask=mask, labels=labels)
        loss = outputs.loss
        tr_logits = outputs.logits
        tr_loss += loss.item()

        nb_tr_steps += 1
        nb_tr_examples += labels.size(0)
        
        if idx % 100==0:
            loss_step = tr_loss/nb_tr_steps
            print(f"Training loss per 100 training steps: {loss_step}")
           
        # compute training accuracy
        flattened_targets = labels.view(-1) # shape (batch_size * seq_len,)
        active_logits = tr_logits.view(-1, model.num_labels) # shape (batch_size * seq_len, num_labels)
        flattened_predictions = torch.argmax(active_logits, axis=1) # shape (batch_size * seq_len,)
        
        # only compute accuracy at active labels
        active_accuracy = labels.view(-1) != -100 # shape (batch_size, seq_len)
        #active_labels = torch.where(active_accuracy, labels.view(-1), torch.tensor(-100).type_as(labels))
        
        labels = torch.masked_select(flattened_targets, active_accuracy)
        predictions = torch.masked_select(flattened_predictions, active_accuracy)
        
        tr_labels.extend(labels)
        tr_preds.extend(predictions)

        tmp_tr_accuracy = accuracy_score(labels.cpu().numpy(), predictions.cpu().numpy())
        tr_accuracy += tmp_tr_accuracy
    
        # gradient clipping
        torch.nn.utils.clip_grad_norm_(
            parameters=model.parameters(), max_norm=MAX_GRAD_NORM
        )
        
        # backward pass
        optimizer.zero_grad()
        loss.backward()
        optimizer.step()

    epoch_loss = tr_loss / nb_tr_steps
    tr_accuracy = tr_accuracy / nb_tr_steps
    print(f"Training loss epoch: {epoch_loss}")
    print(f"Training accuracy epoch: {tr_accuracy}")
    model.save_pretrained(model_directory)
    torch.save({
            'epoch': epoch,
            'optimizer_state_dict': optimizer.state_dict(),
            'loss': loss,
            }, model_metadata_path)

for epoch in range(epoch_start, EPOCHS):
    print(f"Training epoch: {epoch + 1}")
    train(epoch)