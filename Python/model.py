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




# # Load the dataset
# (train_images, train_labels), (test_images, test_labels) = mnist.load_data()
# # images, labels = extract_training_samples('digits')

# # Preprocess the data
# # train_images = train_images.reshape((60000, 28, 28, 1)).astype('float32') / 255
# # test_images = test_images.reshape((10000, 28, 28, 1)).astype('float32') / 255

# train_labels = to_categorical(train_labels)
# print(train_labels.shape)
# # test_labels = to_categorical(test_labels)


num_columns = 785

# Create column names
columns = ['label'] + list(range(1, num_columns))

parent_dir = os.path.abspath(os.path.join(os.getcwd(), os.path.pardir))

# Read the training and testing datasets
ds_train = pd.read_csv(parent_dir + "/Data/emnist/emnist-byclass-train.csv", header=None)
ds_test = pd.read_csv(parent_dir + "/Data/emnist/emnist-byclass-test.csv", header=None)

# Assign column names to the DataFrames
ds_train.columns = columns
ds_test.columns = columns

# Reset index
ds_train = ds_train.reset_index(drop=True)
ds_test = ds_test.reset_index(drop=True)


print(ds_test.shape)
# Extract features and labels
train_images = ds_train.drop(['label'], axis=1).to_numpy().reshape(-1, 28, 28, 1)
train_labels = to_categorical(ds_train['label'].to_numpy())

print(train_images.shape)
print(train_labels.shape)

test_images = ds_test.drop(['label'], axis=1).to_numpy().reshape(-1, 28, 28, 1)  
test_labels = to_categorical(ds_test['label'].to_numpy())

model = Sequential()
model.add(Conv2D(28, (5, 5), padding='same', input_shape=train_images.shape[1:]))
model.add(LeakyReLU(alpha=0.2))
model.add(BatchNormalization())
model.add(Conv2D(28, (5, 5)))
model.add(LeakyReLU(alpha=0.2))
model.add(MaxPool2D(pool_size=(2, 2)))
model.add(Dropout(0.25))

model.add(Conv2D(32, (5, 5), padding='same', input_shape=train_images.shape[1:]))
model.add(LeakyReLU(alpha=0.2))
model.add(BatchNormalization())
model.add(Conv2D(32, (5, 5)))
model.add(LeakyReLU(alpha=0.2))
model.add(MaxPool2D(pool_size=(2, 2)))
model.add(Dropout(0.25))

model.add(Flatten())
model.add(Dense(512))
model.add(LeakyReLU(alpha=0.2))
model.add(Dropout(0.25))
model.add(Dense(62))
model.add(Activation('softmax'))




# Compile the model
model.compile(optimizer='adam',
            loss='categorical_crossentropy',
            metrics=['accuracy'])

# Train the model
model.fit(train_images, train_labels, epochs=5, batch_size=64)
# print(model.predict(transformed_image))

# # Evaluate the model
test_loss, test_acc = model.evaluate(test_images, test_labels)
print(f"Test accuracy: {test_acc}")


model.save("emnist.keras")

# # convert 
# model_json = model.to_json()
# with open("model.json", "w") as json_file:
#     json_file.write(model_json)

# # Save the model weights to a HDF5 file
# model.save_weights("model.h5")


# with open("model.pkl", "wb") as f:
#     pickle.dump(model, f)
