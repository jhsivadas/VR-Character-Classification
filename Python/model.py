import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import seaborn as sns
import tensorflow as tf
from tensorflow.keras import layers, models
from tensorflow.keras.datasets import mnist
from tensorflow.keras.utils import to_categorical
from tensorflow.keras.layers import Dropout, BatchNormalization, Conv2D, Activation, MaxPool2D, Flatten, Dense, LeakyReLU
from tensorflow.keras.models import Sequential
import os
import pickle
from tensorflow.keras.models import model_from_json

# Read in EMNIST Data
num_columns = 785
columns = ['label'] + list(range(1, num_columns))
parent_dir = os.path.abspath(os.path.join(os.getcwd(), os.path.pardir))
ds_train = pd.read_csv(parent_dir + "/Data/emnist-bymerge-train.csv", header=None)
ds_test = pd.read_csv(parent_dir + "/Data/emnist-bymerge-test.csv", header=None)
ds_train.columns = columns
ds_test.columns = columns
ds_train = ds_train.reset_index(drop=True)
ds_test = ds_test.reset_index(drop=True)
train_images = ds_train.drop(['label'], axis=1).to_numpy().reshape(-1, 28, 28, 1)
train_labels = to_categorical(ds_train['label'].to_numpy())
test_images = ds_test.drop(['label'], axis=1).to_numpy().reshape(-1, 28, 28, 1)  
test_labels = to_categorical(ds_test['label'].to_numpy())

# Build and compile CNN
model = Sequential()
model.add(Conv2D(32, (3, 3), padding='same', input_shape=train_images.shape[1:]))
model.add(LeakyReLU(alpha=0.3))
model.add(BatchNormalization())
model.add(Conv2D(32, (3, 3)))
model.add(LeakyReLU(alpha=0.3))
model.add(MaxPool2D(pool_size=(2, 2)))
model.add(Dropout(0.3))

model.add(Conv2D(64, (3, 3), padding='same'))
model.add(LeakyReLU(alpha=0.3))
model.add(BatchNormalization())
model.add(Conv2D(64, (3, 3)))
model.add(LeakyReLU(alpha=0.3))
model.add(MaxPool2D(pool_size=(2, 2)))
model.add(Dropout(0.3))

model.add(Flatten())
model.add(Dense(512))
model.add(LeakyReLU(alpha=0.3))
model.add(Dropout(0.4))
model.add(Dense(47))
model.add(Activation('softmax'))




# Compile the model

# Train the model
history = model.fit(train_images, train_labels, epochs=5, batch_size=64)
# print(model.predict(transformed_image))


# # Evaluate the model
test_loss, test_acc = model.evaluate(test_images, test_labels)
print(f"Test accuracy: {test_acc}")