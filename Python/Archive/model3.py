import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import seaborn as sns
import torch
import torch.nn as nn
import torch.optim as optim
from torch.utils.data import Dataset, DataLoader
import os

# Load the dataset
parent_dir = os.path.abspath(os.path.join(os.getcwd(), os.path.pardir))
ds_train = pd.read_csv(parent_dir + "/Data/emnist-bymerge-train.csv", header=None)
ds_test = pd.read_csv(parent_dir + "/Data/emnist-bymerge-test.csv", header=None)

# Assign column names to the DataFrames
num_columns = 785
columns = ['label'] + list(range(1, num_columns))
ds_train.columns = columns
ds_test.columns = columns

# Reset index
ds_train = ds_train.reset_index(drop=True)
ds_test = ds_test.reset_index(drop=True)

# Extract features and labels
train_images = ds_train.drop(['label'], axis=1).to_numpy().reshape(-1, 1, 28, 28).astype(np.float32) 
train_labels = ds_train['label'].to_numpy().astype(np.int64)

test_images = ds_test.drop(['label'], axis=1).to_numpy().reshape(-1, 1, 28, 28).astype(np.float32) 
test_labels = ds_test['label'].to_numpy().astype(np.int64)

# Create PyTorch dataset and dataloader
class EMNISTDataset(Dataset):
    def __init__(self, images, labels):
        self.images = images
        self.labels = labels

    def __len__(self):
        return len(self.images)

    def __getitem__(self, idx):
        return self.images[idx], self.labels[idx]

train_dataset = EMNISTDataset(train_images, train_labels)
test_dataset = EMNISTDataset(test_images, test_labels)

train_loader = DataLoader(train_dataset, batch_size=64, shuffle=True)
test_loader = DataLoader(test_dataset, batch_size=64, shuffle=False)

# Define the model
class EMNISTModel(nn.Module):
    def __init__(self):
        super(EMNISTModel, self).__init__()
        self.conv1 = nn.Conv2d(1, 28, kernel_size=5, padding=2)
        self.leaky_relu1 = nn.LeakyReLU(negative_slope=0.2)
        self.batch_norm1 = nn.BatchNorm2d(28)
        self.conv2 = nn.Conv2d(28, 28, kernel_size=5)
        self.leaky_relu2 = nn.LeakyReLU(negative_slope=0.2)
        self.max_pool1 = nn.MaxPool2d(kernel_size=2)
        self.dropout1 = nn.Dropout(0.25)

        self.conv3 = nn.Conv2d(28, 32, kernel_size=5, padding=2)
        self.leaky_relu3 = nn.LeakyReLU(negative_slope=0.2)
        self.batch_norm2 = nn.BatchNorm2d(32)
        self.conv4 = nn.Conv2d(32, 32, kernel_size=5)
        self.leaky_relu4 = nn.LeakyReLU(negative_slope=0.2)
        self.max_pool2 = nn.MaxPool2d(kernel_size=2)
        self.dropout2 = nn.Dropout(0.25)

        self.flatten = nn.Flatten()
        self.fc1 = nn.Linear(32 * 7 * 7, 512)
        self.leaky_relu5 = nn.LeakyReLU(negative_slope=0.2)
        self.dropout3 = nn.Dropout(0.25)
        self.fc2 = nn.Linear(512, 62)
        self.softmax = nn.Softmax(dim=1)

    def forward(self, x):
        x = self.conv1(x)
        x = self.leaky_relu1(x)
        x = self.batch_norm1(x)
        x = self.conv2(x)
        x = self.leaky_relu2(x)
        x = self.max_pool1(x)
        x = self.dropout1(x)

        x = self.conv3(x)
        x = self.leaky_relu3(x)
        x = self.batch_norm2(x)
        x = self.conv4(x)
        x = self.leaky_relu4(x)
        x = self.max_pool2(x)
        x = self.dropout2(x)

        x = x.view(x.size(0), -1)  # Flatten the input
        x = self.fc1(x)
        x = self.leaky_relu5(x)
        x = self.dropout3(x)
        x = self.fc2(x)
        x = self.softmax(x)
        return x

# Train the model
model = EMNISTModel()
criterion = nn.CrossEntropyLoss()
optimizer = optim.Adam(model.parameters(), lr=0.001)

num_epochs = 10
for epoch in range(num_epochs):
    running_loss = 0.0
    for i, (images, labels) in enumerate(train_loader):
        optimizer.zero_grad()
        outputs = model(images)
        loss = criterion(outputs, labels)
        loss.backward()
        optimizer.step()
        running_loss += loss.item()
    print(f'Epoch [{epoch+1}/{num_epochs}], Loss: {running_loss/len(train_loader)}')

# Evaluate the model
correct = 0
total = 0
with torch.no_grad():
    for images, labels in test_loader:
        outputs = model(images)
        _, predicted = torch.max(outputs.data, 1)
        total += labels.size(0)
        correct += (predicted == labels).sum().item()
print(f'Accuracy of the model on the test images: {100 * correct / total}%')
