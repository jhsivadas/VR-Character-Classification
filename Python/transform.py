import functions_framework
import numpy as np 
import sklearn
import pandas as pd 
from google.cloud import storage
import joblib
from tensorflow.keras.models import load_model

# Load Keras Model from storage 
storage_client = storage.Client()
bucket = storage_client.get_bucket("sklearn-model-storage")
blob = bucket.blob("emnist.keras")
blob.download_to_filename("/tmp/emnist.keras")
model = load_model("/tmp/emnist.keras")


# Triggered by a change in a digit-svr storage bucket (when VR data is pushed)
@functions_framework.cloud_event
def hello_gcs(cloud_event):
    data = cloud_event.data

    # Set connection to storage bucket that VR data is pushed to 
    storage_client = storage.Client()
    bucket = storage_client.get_bucket("digits-vr") # remember to change the bucket name
    blob = bucket.blob(data['name'])
    blob.download_to_filename(data['name'])

    # Checks if data pushed is a csv file
    if data['name'].split('.')[1] == 'csv':

        # Reads in VR Data and converts it to MNIST Representation
        df = pd.read_csv(data['name'], index_col=False)
        adjust = matrix(df)

        # Model Inference 
        prediction = model.predict(adjust)
        max_prob_index = prediction[0].argmax()
        index = {0: 48, 1: 49, 2: 50, 3: 51, 4: 52, 5: 53, 6: 54, 7: 55, 8: 56, 9: 57, 10: 65, 11: 66, 12: 67, 13: 68, 14: 69, 15: 70, 16: 71, 17: 72, 18: 73, 19: 74, 20: 75, 21: 76, 22: 77, 23: 78, 24: 79, 25: 80, 26: 81, 27: 82, 28: 83, 29: 84, 30: 85, 31: 86, 32: 87, 33: 88, 34: 89, 35: 90, 36: 97, 37: 98, 38: 100, 39: 101, 40: 102, 41: 103, 42: 104, 43: 110, 44: 113, 45: 114, 46: 116}
        ascii = index[max_prob_index]
        ch = chr(ascii)
        
        # Output Inference to response file to be read by headset
        with open('response.txt', 'w') as f:
            f.write(ch)

        client = storage.Client()
        bucket = client.get_bucket('digits-vr')
        blob = bucket.blob('response.txt')
        blob.upload_from_filename('response.txt')


# Function that converts raw VR data to 28x28 model inferenceable format
def matrix(df):  
    y_values = df['controller_right_pos.y']
    x_values = df['controller_right_pos.x']

    # Get max x and y values
    min_x = x_values.min()
    max_x = x_values.max()
    x_diff = max_x - min_x

    min_y = y_values.min()
    max_y = y_values.max()
    y_diff = max_y - min_y

    # Create matrix storage
    matrix = np.zeros((28, 28))

    # Iterate over each coordinate
    for i in range(len(y_values)):
        y = y_values[i]
        x = x_values[i]

        # Calculate x and y scaled along 28x28 axis
        curr_diff_y = y - min_y
        scale_y = curr_diff_y / y_diff
        scaled_y = round(18*scale_y) + 5

        curr_diff_x = x - min_x
        scale_x = curr_diff_x / x_diff
        scaled_x = round(18*scale_x) + 5

        matrix[scaled_y, scaled_x] += 1

    max = np.max(matrix)

    # Reshape scale based on frequency [0,1]
    for row in range(matrix.shape[0]):
        for col in range(matrix.shape[1]):
            val = matrix[row, col]
            scale = val / max
            matrix[row,col] = scale

    # Reshape to fit formatting
    matrix = matrix.reshape(-1, 28, 28, 1).transpose()

    return matrix

    