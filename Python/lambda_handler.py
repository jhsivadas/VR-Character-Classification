from transform import main
import boto3
from io import StringIO
import pandas as pd

s3_client = boto3.client('s3')

def lambda_handler(event, context):
    bucket = 'digits-vr'
    key= event["Records"][0]["s3"]["object"]["key"]
        
    object = s3_client.get_object(Bucket=bucket, Key=key)
    body = object['Body']
    csv_string = body.read().decode('utf-8')
    dataframe = pd.read_csv(StringIO(csv_string))
    print(dataframe)
    main(dataframe)


   
   # The return just needs to be some JSON data structure
    return {
        'message' : "hi"
    }
