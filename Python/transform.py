import pandas as pd
import numpy as np
# import matplotlib.pyplot as plt
# import seaborn as sns
# import cv2


def scale(matrix):
    # Get the minimum and maximum values along each axis
    min_x = 0
    max_x = matrix.shape[0]
    print(max_x)
    x_diff = max_x - min_x

    min_y = 0
    max_y = matrix.shape[1]
    y_diff = max_y - min_y

    max = 20

    # Create a new matrix to store the scaled values
    scaled_matrix = np.zeros((28, 28))

    for y in range(matrix.shape[0]):
        for x in range(matrix.shape[1]):

            if (matrix[x, y] > 0):
                print(y, x)
                
                curr_diff_y = y - min_y
                scale_y = curr_diff_y / y_diff
                scaled_y = round(18 * scale_y) + 4
                print(scaled_y)

                # Scale the x-coordinate
                curr_diff_x = x - min_x
                scale_x = curr_diff_x / x_diff
                print(curr_diff_x)
                print(x_diff)
                print(scale_x)
                scaled_x = round(18 * scale_x) + 4
                print(scaled_x)

                # Increment the value in the scaled matrix
                scaled_matrix[scaled_x, scaled_y] += 1

    for row in range(scaled_matrix.shape[0]):
        for col in range(scaled_matrix.shape[1]):
            if scaled_matrix[row, col] >= 1:
                scaled_matrix[row, col] = 1
            else:
                scaled_matrix[row, col] = scaled_matrix[row, col] / max


    # plt.figure(figsize=(15, 8))
    # sns.heatmap(scaled_matrix, annot=True, cmap='gray', cbar=False)
    # plt.xticks(range(scaled_matrix.shape[1]))
    # plt.yticks(range(scaled_matrix.shape[0]))
    # plt.grid(True)
    # plt.show()

    return scaled_matrix



# def matrix(df):
#     # df = pd.read_csv(file_name, index_col=False)
#     # df = project(df)
#     # print(df)



#     y_values = df['controller_right_pos.y']
#     print(y_values)
#     x_values = df['controller_right_pos.x']

#     min_x = x_values.min()
#     max_x = x_values.max()
#     x_diff = max_x - min_x

#     min_y = y_values.min()
#     max_y = y_values.max()
#     y_diff = max_y - min_y


#     matrix = np.zeros((28, 28))

#     for i in range(len(y_values)):
#         y = y_values.iloc[i]
#         x = x_values.iloc[i]

#         curr_diff_y = y - min_y
#         scale_y = curr_diff_y / y_diff
#         scaled_y = round(18*scale_y) + 5

#         curr_diff_x = x - min_x
#         scale_x = curr_diff_x / x_diff
#         scaled_x = round(18*scale_x) + 5

#         matrix[scaled_y, scaled_x] += 1

#     max = np.max(matrix)
#     half = 1


#     # matrix[26, 47] = 1
#     # matrix[26, 48] = 1
#     # matrix[27, 47] = 1
#     # matrix[27, 48] = 1 

#     plt.figure(figsize=(15, 8))
#     sns.heatmap(matrix, annot=True, cmap='gray', cbar=False)
#     plt.xticks(range(matrix.shape[1]))
#     plt.yticks(range(matrix.shape[0]))
#     plt.grid(True)
#     plt.show()

  

#     for row in range(matrix.shape[0]):
#         for col in range(matrix.shape[1]):
#             if matrix[row, col] >= half:
#                 matrix[row, col] = 1
#             else:
#                 matrix[row, col] = matrix[row, col] / max
#     plt.figure(figsize=(15, 8))
#     sns.heatmap(matrix, annot=True, cmap='gray', cbar=False)
#     plt.xticks(range(matrix.shape[1]))
#     plt.yticks(range(matrix.shape[0]))
#     plt.grid(True)
#     plt.show()

#     return matrix


# def bounding(A):
#     _, markers = cv2.connectedComponents(np.expand_dims(A.astype(np.uint8),axis=2),connectivity=4)

#     markers = np.squeeze(markers)

#     bounding_boxes = []

#     for label in range(1,int(np.max(markers))+1):

#         locations = np.transpose(np.nonzero(markers==label))

#         min_x, max_x, min_y, max_y = np.min(locations[:,1]), np.max(locations[:,1]), np.min(locations[:,0]), np.max(locations[:,0])

#         bounding_boxes.append((min_x, max_x, min_y, max_y))

#     print(bounding_boxes)
#     return bounding_boxes



def bounding(A):
    markers = np.zeros_like(A, dtype=int)
    current_label = 1
    bounding_boxes = []

    def dfs(row, col, label):
        if 0 <= row < A.shape[0] and 0 <= col < A.shape[1] and A[row, col] and markers[row, col] == 0:
            markers[row, col] = label
            dfs(row-1, col, label)
            dfs(row+1, col, label)
            dfs(row, col-1, label)
            dfs(row, col+1, label)

    for row in range(A.shape[0]):
        for col in range(A.shape[1]):
            if A[row, col] and markers[row, col] == 0:
                dfs(row, col, current_label)
                locations = np.transpose(np.nonzero(markers == current_label))
                min_x, max_x, min_y, max_y = np.min(locations[:, 1]), np.max(locations[:, 1]), np.min(locations[:, 0]), np.max(locations[:, 0])
                bounding_boxes.append((min_x, max_x, min_y, max_y))
                current_label += 1

    # Remove duplicates and merge overlapping boxes
    unique_boxes = []
    for box in bounding_boxes:
        is_duplicate = False
        for unique_box in unique_boxes:
            if (box[0] <= unique_box[1] and box[1] >= unique_box[0] and
                box[2] <= unique_box[3] and box[3] >= unique_box[2]):
                is_duplicate = True
                break
        if not is_duplicate:
            unique_boxes.append(box)

    print(unique_boxes)
    return unique_boxes




# def main(df):
#     adjust = matrix(df)
#     print(adjust)
#     boxes = bounding(adjust)
#     print(boxes)
    predictions = []
    # for i, bound in enumerate(boxes):
    #     adjusted = adjust[bound[2]:(bound[3] + 1),bound[0]:(bound[1] + 1)]
    #     scaled = scale(adjusted)

# df = pd.read_csv('DRI_01.csv')
# main(df)


def matrix(file_name):
    df = pd.read_csv(file_name, index_col=False)
    y_values = df['controller_right_pos.y']
    x_values = df['controller_right_pos.x']

    min_x = x_values.min()
    max_x = x_values.max()
    x_diff = max_x - min_x

    min_y = y_values.min()
    max_y = y_values.max()
    y_diff = max_y - min_y


    matrix = np.zeros((28, 28))

    for i in range(len(y_values)):
        y = y_values[i]
        x = x_values[i]

        curr_diff_y = y - min_y
        scale_y = curr_diff_y / y_diff
        scaled_y = round(18*scale_y) + 5

        curr_diff_x = x - min_x
        scale_x = curr_diff_x / x_diff
        scaled_x = round(18*scale_x) + 5

        matrix[scaled_y, scaled_x] += 1

    # half = np.percentile(matrix, 90)
    half = 1
    max = np.max(matrix)

    # plt.figure(figsize=(15, 8))
    # sns.heatmap(matrix, annot=True, cmap='ocean', cbar=False)
    # plt.xticks(range(matrix.shape[1]))
    # plt.yticks(range(matrix.shape[0]))
    # plt.grid(True)
    # plt.show()

    for row in range(matrix.shape[0]):
        for col in range(matrix.shape[1]):
            val = matrix[row, col]
            scale = val / max
            matrix[row,col] = 255 * scale
            # if matrix[row, col] >= half:
            #     matrix[row, col] = 1
            # else:
            #     matrix[row, col] = matrix[row, col] / max

    # plt.figure(figsize=(15, 8))
    # sns.heatmap(matrix, annot=True, cmap='ocean', cbar=False)
    # plt.xticks(range(matrix.shape[1]))
    # plt.yticks(range(matrix.shape[0]))
    # plt.grid(True)
    # plt.show()

    matrix = matrix.reshape(-1, 28, 28, 1).transpose()

    return matrix
import os

adjust = matrix('DRI_01.csv')














from tensorflow.keras.utils import to_categorical

num_columns = 785

# Create column names
columns = ['label'] + list(range(1, num_columns))

parent_dir = os.path.abspath(os.path.join(os.getcwd(), os.path.pardir))

ds_test = pd.read_csv(parent_dir + "/Data/emnist-bymerge-test.csv", header=None)

# Assign column names to the DataFrames
ds_test.columns = columns



# Reset ind
ds_test = ds_test.reset_index(drop=True)



test_images = ds_test.drop(['label'], axis=1).to_numpy().reshape(-1, 28, 28, 1)  
test_labels = to_categorical(ds_test['label'].to_numpy())




from keras.models import load_model

# Load the saved model
model = load_model('emnist.keras')

# test_loss, test_acc = model.evaluate(test_images, test_labels)
# print(f"Test accuracy: {test_acc}")

# Make a prediction
output = model.predict(adjust)

# Find the index of the maximum probability
max_prob_index = output[0].argmax()

# Print the maximum probability
print(f"Maximum probability: {output[0][max_prob_index]}")
print(max_prob_index)
print(output)