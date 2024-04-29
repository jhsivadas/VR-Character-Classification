import pandas as pd
import numpy as np
from sklearn.neural_network import MLPClassifier
import pickle
import os

num_columns = 785

# Create column names
columns = ['label'] + list(range(1, num_columns))

parent_dir = os.path.abspath(os.path.join(os.getcwd(), os.path.pardir))

# Read the training and testing datasets
ds_train = pd.read_csv(parent_dir + "/Data/emnist-bymerge-train.csv", header=None)
ds_test = pd.read_csv(parent_dir + "/Data/emnist-bymerge-test.csv", header=None)

# Assign column names to the DataFrames
ds_train.columns = columns
ds_test.columns = columns

# Reset index
ds_train = ds_train.reset_index(drop=True)
ds_test = ds_test.reset_index(drop=True)


from sklearn.preprocessing import LabelEncoder

# Create a LabelEncoder object
le = LabelEncoder()
print(ds_test.shape)
# Extract features and labels
train_images = ds_train.drop(['label'], axis=1).to_numpy().reshape(-1, 28 * 28) / 255
train_labels = le.fit_transform(ds_train['label'].to_numpy())

print(train_images.shape)
print(train_labels.shape)

test_images = ds_test.drop(['label'], axis=1).to_numpy().reshape(-1, 28 * 28) / 255
test_labels =le.fit_transform(ds_test['label'].to_numpy())


mlp = MLPClassifier(hidden_layer_sizes=(50, ), max_iter=10, alpha=1e-4, 
                    solver='sgd', verbose=10, random_state=1, learning_rate_init=.1)


mlp.fit(train_images, train_labels)

print(f"Training set score: {mlp.score(train_images, train_labels):.3f}")
print(f"Test set score: {mlp.score(test_images, test_labels):.3f}")

with open('model.pkl','wb') as f:
    pickle.dump(mlp,f)

