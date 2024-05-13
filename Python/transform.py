import functions_framework
import numpy as np 
import sklearn
import pandas as pd 
from google.cloud import storage
from google.cloud import aiplatform
import joblib
import random

# Connect to trained model endpoint
endpoint = aiplatform.Endpoint(
    endpoint_name="projects/460284996720/locations/us-central1/endpoints/4371471315060654080")

# Triggered by a change in a storage bucket
@functions_framework.cloud_event
def hello_gcs(cloud_event):
    data = cloud_event.data

    # Connect to storage bucket that was just uploaded to
    storage_client = storage.Client()
    bucket = storage_client.get_bucket("digits-vr") # remember to change the bucket name
    blob = bucket.blob(data['name'])
    blob.download_to_filename(data['name'])

    # Check if changed file is csv
    if data['name'].split('.')[1] == 'csv':

        # Transform data to proper format
        df = pd.read_csv(data['name'], index_col=False)
        adjust = matrix(df).astype(np.float32).tolist()[0]

        # Model Inference
        prediction = np.array(endpoint.predict(instances=[adjust]).predictions)
        max_prob_index = prediction[0].argmax()
        index = {0: 48, 1: 49, 2: 50, 3: 51, 4: 52, 5: 53, 6: 54, 7: 55, 8: 56, 9: 57, 10: 65, 11: 66, 12: 67, 13: 68, 14: 69, 15: 70, 16: 71, 17: 72, 18: 73, 19: 74, 20: 75, 21: 76, 22: 77, 23: 78, 24: 79, 25: 80, 26: 81, 27: 82, 28: 83, 29: 84, 30: 85, 31: 86, 32: 87, 33: 88, 34: 89, 35: 90, 36: 97, 37: 98, 38: 100, 39: 101, 40: 102, 41: 103, 42: 104, 43: 110, 44: 113, 45: 114, 46: 116}
        ascii = index[max_prob_index]
        ch = chr(ascii)

        # Write model response out 
        with open('response.txt', 'w') as f:
            f.write(ch)
        client = storage.Client()
        bucket = client.get_bucket('digits-vr')
        blob = bucket.blob('response.txt')
        blob.upload_from_filename('response.txt')


# Function to transform data to EMNIST representation
def matrix(df):  
    y_values = df['controller_right_pos.y']
    x_values = df['controller_right_pos.x']

    min_x = x_values.min()
    max_x = x_values.max()
    x_diff = max_x - min_x

    min_y = y_values.min()
    max_y = y_values.max()
    y_diff = max_y - min_y


    matrix = np.zeros((28, 28))

    # Iterate over raw VR data values frequencies in relative scale
    for i in range(len(y_values)):
        y = y_values[i]
        x = x_values[i]

        curr_diff_y = y - min_y
        scale_y = curr_diff_y / y_diff
        scaled_y = round(18*scale_y) + 5

        curr_diff_x = x - min_x
        scale_x = curr_diff_x / x_diff
        scaled_x = round(18*scale_x) + 5

        matrix[scaled_y, scaled_x] = 255

    # Fill surrounding values using the fill function
    matrix = fill(matrix)

    # Reorient and transform data
    matrix = np.flip(matrix, 0)
    matrix = np.flip(matrix, 1)
    matrix = matrix.reshape(-1, 28, 28, 1).transpose()

    return matrix

# This function fills all data surrounding covered data with noise for proper inference
def fill(matrix):
    rows, cols = matrix.shape
    for i in range(rows):
        for j in range(cols):
            if matrix[i, j] == 255:
                for di in [-1, 0, 1]:
                    for dj in [-1, 0, 1]:
                        if (di != 0 or dj != 0) and 0 <= i + di < rows and 0 <= j + dj < cols and matrix[i + di, j + dj] == 0:
                            matrix[i + di, j + dj] = random.randint(50, 200)
    return matrix