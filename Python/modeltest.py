import numpy as np
import matplotlib.pyplot as plt
import seaborn as sns
from sklearn.metrics import confusion_matrix, classification_report, roc_curve, auc
from tensorflow.keras.utils import to_categorical
from tensorflow.keras.models import load_model
import os
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from sklearn.metrics import confusion_matrix

# Import Data
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

# Load the model from a .keras file
model = load_model('emnist.keras')
y_pred = np.argmax(model.predict(test_images), axis=-1)
y_true = np.argmax(test_labels, axis=1)
report = classification_report(y_true, y_pred)


# Compute the confusion matrix
cm = confusion_matrix(y_true, y_pred)

# Plot the confusion matrix
fig, ax = plt.subplots(figsize=(12, 12))
ax.imshow(cm, cmap='Blues')
ax.set_xlabel('Predicted')
ax.set_ylabel('True')
ax.set_title('Confusion Matrix')
target_names = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'd', 'e', 'f', 'g', 'h', 'n', 'q', 'r', 't']
tick_marks = np.arange(len(target_names))
ax.set_xticks(tick_marks)
ax.set_yticks(tick_marks)
ax.set_xticklabels(target_names, rotation=90)
ax.set_yticklabels(target_names)

# Display the values in the cells
for i in range(len(target_names)):
    for j in range(len(target_names)):
        ax.text(j, i, cm[i, j], ha="center", va="center", color="gray", fontsize=5, rotation=45)

plt.show()