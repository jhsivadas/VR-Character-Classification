import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import seaborn as sns


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

    half = np.percentile(matrix, 90)
    max = np.max(matrix)

    for row in range(matrix.shape[0]):
        for col in range(matrix.shape[1]):
            if matrix[row, col] >= half:
                matrix[row, col] = 1
            else:
                matrix[row, col] = matrix[row, col] / max
    # plt.figure(figsize=(15, 8))
    # sns.heatmap(matrix, annot=True, cmap='ocean', cbar=False)
    # plt.xticks(range(matrix.shape[1]))
    # plt.yticks(range(matrix.shape[0]))
    # plt.grid(True)
    # plt.show()

    return matrix

adjust = matrix('DRI_01.csv')
transformed_image = np.expand_dims(adjust, axis=0)  # Add batch dimension
transformed_image = np.expand_dims(transformed_image, axis=-1)  # Add channel dimension

print("Transformed shape:", transformed_image.shape)