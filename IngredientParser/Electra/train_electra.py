import os
import pandas as pd
import numpy as np
import tensorflow as tf
from tensorflow import keras
from transformers import TFElectraModel
import tf2onnx

num_classes = 13
batch_size = 32

def train():
    tf.get_logger().setLevel('ERROR')
    strategy = tf.distribute.MirroredStrategy()
    os.environ["TRANSFORMERS_OFFLINE"]="1"

    local_file_path = os.path.join(os.getcwd(), 'Electra', 'BERT')
    electra_model_path = os.path.join(local_file_path, 'inputs', 'electra-small-discriminator')
    electra_trained_model_path = os.path.join(local_file_path, 'outputs', 'trained_model')

    with strategy.scope():
        loss_object = tf.keras.losses.SparseCategoricalCrossentropy(
            from_logits=False, reduction=tf.keras.losses.Reduction.NONE
        )

        def masked_ce_loss(real, pred):
            mask = tf.math.logical_not(tf.math.equal(real, 17))
            loss_ = loss_object(real, pred)

            mask = tf.cast(mask, dtype=loss_.dtype)
            loss_ *= mask

            return tf.reduce_mean(loss_)

        encoder = TFElectraModel.from_pretrained(electra_model_path)
        input_ids = tf.keras.layers.Input(shape=(128,), dtype=tf.int32)
        token_type_ids = tf.keras.layers.Input(shape=(128,), dtype=tf.int32)
        attention_mask = tf.keras.layers.Input(shape=(128,), dtype=tf.int32)
        embedding = encoder(
            input_ids, token_type_ids=token_type_ids, attention_mask=attention_mask
        )[0]
        embedding = tf.keras.layers.Dropout(0.3)(embedding)
        tag_logits = tf.keras.layers.Dense(13, activation='softmax')(embedding)

        model = keras.Model(
            inputs=[input_ids, token_type_ids, attention_mask],
            outputs=[tag_logits],
        )
        optimizer = keras.optimizers.Adam(lr=3e-5)
        model.compile(optimizer=optimizer, loss=masked_ce_loss, metrics=['accuracy'])
        # model.summary()

        input_masks = pd.read_csv(os.path.join(local_file_path,'inputs', 'InputMask.csv'), dtype=np.int32).to_numpy()
        input_word_ids = pd.read_csv(os.path.join(local_file_path, 'inputs', 'InputWordIds.csv'), dtype=np.int32).to_numpy()
        input_type_ids = pd.read_csv(os.path.join(local_file_path, 'inputs', 'InputTypeIds.csv'), dtype=np.int32).to_numpy()
        labels = pd.read_csv(os.path.join(local_file_path, 'inputs', 'Labels.csv'), dtype=np.int32).to_numpy()

        input = [
            input_word_ids,
            input_type_ids,
            input_masks,
        ]

        checkpoint_prefix = os.path.join(local_file_path, 'training_checkpoints', "ckpt_{epoch}")
        callbacks = [
            tf.keras.callbacks.ModelCheckpoint(filepath=checkpoint_prefix, save_weights_only=True),
        ]

        model.fit(
            input,
            labels,
            epochs=3,
            verbose=1,
            batch_size=batch_size,
            validation_split=0.1
        )

        model.save(electra_trained_model_path)
        (onnx_model_proto, storage) = tf2onnx.convert.from_keras(model)
        with open(os.path.join(local_file_path, 'model.onnx'), "wb") as f:
            f.write(onnx_model_proto.SerializeToString())