import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import seaborn as sns
import cv2


# def project(df):
#     return_df = pd.DataFrame(columns=["x", "y", "z"])

#     for index, row in df.iterrows():
#         x = row["controller_right_pos.x"]
#         y = row["controller_right_pos.y"]
#         z = row["controller_right_pos.z"]
#         time  = row["time"]

#         p = Point3D(x,y,z)

#         plane = Plane(Point3D(1, 1, 0), normal_vector =(0, 0, 1)) 
#         projection = plane.projection(p) 
#         return_df.loc[index] = [projection.x, projection.y, projection.z]



#     return return_df

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

    for y in range(matrix.shape[1]):
        for x in range(matrix.shape[0]):

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

    for row in range(scaled_matrix.shape[1]):
        for col in range(scaled_matrix.shape[0]):
            if scaled_matrix[row, col] >= 1:
                scaled_matrix[row, col] = 1
            else:
                scaled_matrix[row, col] = scaled_matrix[row, col] / max


    plt.figure(figsize=(15, 8))
    sns.heatmap(scaled_matrix, annot=True, cmap='gray', cbar=False)
    plt.xticks(range(scaled_matrix.shape[1]))
    plt.yticks(range(scaled_matrix.shape[0]))
    plt.grid(True)
    plt.show()

    return scaled_matrix

def matrix(file_name):
    df = pd.read_csv(file_name, index_col=False)
    # df = project(df)
    print(df)



    y_values = df['controller_right_pos.y']
    x_values = df['controller_right_pos.x']

    min_x = x_values.min()
    max_x = x_values.max()
    x_diff = max_x - min_x

    min_y = y_values.min()
    max_y = y_values.max()
    y_diff = max_y - min_y


    matrix = np.zeros((28, 50))

    for i in range(len(y_values)):
        y = y_values[i]
        x = x_values[i]

        curr_diff_y = y - min_y
        scale_y = curr_diff_y / y_diff
        scaled_y = round(18*scale_y) + 5

        curr_diff_x = x - min_x
        scale_x = curr_diff_x / x_diff
        scaled_x = round(40*scale_x) + 5

        matrix[scaled_y, scaled_x] += 1

    max = np.max(matrix)
    half = 1


    # matrix[26, 47] = 1
    # matrix[26, 48] = 1
    # matrix[27, 47] = 1
    # matrix[27, 48] = 1 

    plt.figure(figsize=(15, 8))
    sns.heatmap(matrix, annot=True, cmap='gray', cbar=False)
    plt.xticks(range(matrix.shape[1]))
    plt.yticks(range(matrix.shape[0]))
    plt.grid(True)
    plt.show()

  

    for row in range(matrix.shape[0]):
        for col in range(matrix.shape[1]):
            if matrix[row, col] >= half:
                matrix[row, col] = 1
            else:
                matrix[row, col] = matrix[row, col] / max
    plt.figure(figsize=(15, 8))
    sns.heatmap(matrix, annot=True, cmap='gray', cbar=False)
    plt.xticks(range(matrix.shape[1]))
    plt.yticks(range(matrix.shape[0]))
    plt.grid(True)
    plt.show()

    return matrix


def bounding(A):
    _, markers = cv2.connectedComponents(np.expand_dims(A.astype(np.uint8),axis=2),connectivity=4)

    markers = np.squeeze(markers)

    bounding_boxes = []

    for label in range(1,int(np.max(markers))+1):

        locations = np.transpose(np.nonzero(markers==label))

        min_x, max_x, min_y, max_y = np.min(locations[:,1]), np.max(locations[:,1]), np.min(locations[:,0]), np.max(locations[:,0])

        bounding_boxes.append((min_x, max_x, min_y, max_y))

    print(bounding_boxes)
    return bounding_boxes

# import os
# print(os.getcwd())
# adjust = matrix('ARC_01.csv')
# boxes = bounding(adjust)

# adjusted_1 = adjust[boxes[0][2]:(boxes[0][3]+1), boxes[0][0]:(boxes[0][1]+1)]
# plt.figure(figsize=(15, 8))
# sns.heatmap(adjusted_1, annot=True, cmap='gray', cbar=False)
# plt.xticks(range(adjusted_1.shape[1]))
# plt.yticks(range(adjusted_1.shape[0]))
# plt.grid(True)
# plt.show()
# adjusted_2 = adjust[boxes[1][2]:(boxes[1][3]+1), boxes[1][0]:(boxes[1][1]+1)]
# adjusted_2.resize(28, 28)
# plt.figure(figsize=(15, 8))
# sns.heatmap(adjusted_2, annot=True, cmap='gray', cbar=False)
# plt.xticks(range(adjusted_2.shape[1]))
# plt.yticks(range(adjusted_2.shape[0]))
# plt.grid(True)
# plt.show()
# scale(adjusted_1)
# scale(adjusted_2)


# transformed_image = np.expand_dims(adjust, axis=0)  # Add batch dimension
# transformed_image = np.expand_dims(transformed_image, axis=-1)  # Add channel dimension

# print("Transformed shape:", transformed_image.shape)


def main():
    adjust = matrix('ARC_01.csv')
    boxes = bounding(adjust)
    predictions = []
    for i, bound in enumerate(boxes):
        adjusted = adjust[bound[2]:(bound[3] + 1),bound[0]:(bound[1] + 1)]
        scaled = scale(adjusted)

        # call model.predict 
        # val = model.predict(scaled)
        # predictions.append(val)

