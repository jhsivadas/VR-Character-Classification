import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import seaborn as sns
from sympy import Plane, Point, Point3D


def project(df):
    return_df = pd.DataFrame(columns=["x", "y", "z"])

    for index, row in df.iterrows():
        x = row["controller_right_pos.x"]
        y = row["controller_right_pos.y"]
        z = row["controller_right_pos.z"]
        time  = row["time"]

        p = Point3D(x,y,z)

        plane = Plane(Point3D(1, 1, 0), normal_vector =(0, 0, 1)) 
        projection = plane.projection(p) 
        return_df.loc[index] = [projection.x, projection.y, projection.z]



    return return_df



def matrix(file_name):
    df = pd.read_csv(file_name, index_col=False)
    df = project(df)
    print(df)



    y_values = df['y']
    x_values = df['x']

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

    max = np.max(matrix)
    half = .5 * max

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

import os
print(os.getcwd())
adjust = matrix('ARC_01.csv')
transformed_image = np.expand_dims(adjust, axis=0)  # Add batch dimension
transformed_image = np.expand_dims(transformed_image, axis=-1)  # Add channel dimension

print("Transformed shape:", transformed_image.shape)