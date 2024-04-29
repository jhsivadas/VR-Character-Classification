import numpy as np
import matplotlib.pyplot as plt
import seaborn as sns
from sklearn.metrics import confusion_matrix, classification_report, roc_curve, auc
from tensorflow.keras.utils import to_categorical
from tensorflow.keras.models import load_model
import os
import pandas as pd

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


print(ds_test.shape)
# Extract features and labels
train_images = ds_train.drop(['label'], axis=1).to_numpy().reshape(-1, 28, 28, 1)
train_labels = to_categorical(ds_train['label'].to_numpy())

print(train_images.shape)
print(train_labels.shape)

test_images = ds_test.drop(['label'], axis=1).to_numpy().reshape(-1, 28, 28, 1)  
test_labels = to_categorical(ds_test['label'].to_numpy())
# Load the model from a .keras file
model = load_model('emnist.keras')

history = model.fit(train_images, train_labels, epochs=5, batch_size=64)
plt.plot(history.history['accuracy'])
plt.plot(history.history['val_accuracy'])
plt.title('model accuracy')
plt.ylabel('accuracy')
plt.xlabel('epoch')
plt.legend(['train', 'val'], loc='upper left')
plt.show()