import os
import pandas as pd
import numpy as np
import torch
from torch.utils.data import Dataset
from transformers import BertTokenizerFast

MAX_LEN = 128

class dataset(Dataset):
      def __init__(self, dataframe, tokenizer, max_len, labels_to_ids):
            self.len = len(dataframe)
            self.data = dataframe
            self.tokenizer = tokenizer
            self.max_len = max_len
            self.labels_to_ids = labels_to_ids

      def __getitem__(self, index):
            # step 1: get the sentence and word labels 
            sentence = self.data.sentences[index]
            word_labels = self.data.word_labels[index]

            # step 2: use tokenizer to encode sentence (includes padding/truncation up to max length)
            # BertTokenizerFast provides a handy "return_offsets_mapping" functionality for individual tokens
            encoding = self.tokenizer(sentence,
                              is_split_into_words =True, 
                              return_offsets_mapping=True, 
                              padding='max_length', 
                              truncation=True, 
                              max_length=self.max_len)
            
            # step 3: create token labels only for first word pieces of each tokenized word
            labels = [self.labels_to_ids[label] for label in word_labels] 
            # code based on https://huggingface.co/transformers/custom_datasets.html#tok-ner
            # create an empty array of -100 of length max_length
            encoded_labels = np.ones(len(encoding["offset_mapping"]), dtype=int) * -100
            
            # set only labels whose first offset position is 0 and the second is not 0
            i = 0
            for idx, mapping in enumerate(encoding["offset_mapping"]):
                  if mapping[0] == 0 and mapping[1] != 0:
                        # overwrite label
                        encoded_labels[idx] = labels[i]
                        i += 1

            # step 4: turn everything into PyTorch tensors
            item = {key: torch.as_tensor(val) for key, val in encoding.items()}
            item['labels'] = torch.as_tensor(encoded_labels)
            
            return item

      def __len__(self):
            return self.len

def main():
      tokenizer = BertTokenizerFast.from_pretrained('bert-base-uncased')

      data_path = os.path.join('.', 'IngredientParser', 'training.csv')
      data = pd.read_csv(data_path, names=['Value', 'Tag', 'IngredientSnapshotId' ],
            # nrows=1000,
      )

      labels_to_ids = {k: v for v, k in enumerate(data.Tag.unique())}
      ids_to_labels = {v: k for v, k in enumerate(data.Tag.unique())}

      sentences = data.groupby('IngredientSnapshotId')['Value'].apply(list)
      word_labels = data.groupby('IngredientSnapshotId')['Tag'].apply(list)
      data = pd.DataFrame()
      data['sentences'] = sentences
      data['word_labels'] = word_labels

      train_size = 0.8
      train_dataset = data.sample(frac=train_size,random_state=200)
      test_dataset = data.drop(train_dataset.index).reset_index(drop=True)
      train_dataset = train_dataset.reset_index(drop=True)

      print("FULL Dataset: {}".format(data.shape))
      print("TRAIN Dataset: {}".format(train_dataset.shape))
      print("TEST Dataset: {}".format(test_dataset.shape))

      training_set = dataset(train_dataset, tokenizer, MAX_LEN, labels_to_ids)
      testing_set = dataset(test_dataset, tokenizer, MAX_LEN, labels_to_ids)

      token_directory = os.path.join('.', 'IngredientParser', 'BERT', 'TokenData')
      if not os.path.exists(token_directory):
            os.makedirs(token_directory)

      tokenizer.save_vocabulary(token_directory)

      training_path = os.path.join(token_directory, 'training.pt')
      torch.save({
                  'training_set': training_set,
                  'testing_set': testing_set,
                  'labels_to_ids': labels_to_ids,
                  'ids_to_labels': ids_to_labels,
                  }, training_path)

if __name__ == "__main__":
      main()