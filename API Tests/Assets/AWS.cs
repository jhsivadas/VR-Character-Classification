using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.Networking;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.IO;

public class AWS : MonoBehaviour
{
    private string bucketName = "digits-vr";
    private string keyName = "HOLYCOW.csv";
    private string filePath = "/Users/max/Desktop/VR-Character-Classification/API Tests/Assets/HOLYCOW.csv";
    private string awsCredentialsPath = "/Users/max/Desktop/VR-Character-Classification/API Tests/Assets/aws_credentials.json"; // Path to your aws_credentials.json file

    void Start()
    {
        UploadFile();
    }

    private void UploadFile()
    {
        var credentials = ReadAWSCredentials(awsCredentialsPath);
        if(credentials == null) return;

        AmazonS3Client s3Client = new AmazonS3Client(credentials.AWSAccessKeyId, credentials.AWSSecretKey, Amazon.RegionEndpoint.USEast2);

        try
        {
            PutObjectRequest putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = keyName,
                FilePath = filePath
            };

            PutObjectResponse response = s3Client.PutObjectAsync(putRequest).Result; // Use async in real applications

            Debug.Log("File uploaded successfully.");
        }
        catch (AmazonS3Exception e)
        {
            Debug.LogError("Error encountered on server. Message:'" + e.Message + "' when writing an object");
        }
        catch (Exception e)
        {
            Debug.LogError("Unknown encountered on server. Message:'" + e.Message + "'");
        }
    }

    private AWSCredentials ReadAWSCredentials(string credentialsPath)
    {
        try
        {
            string jsonText = File.ReadAllText(credentialsPath);
            return JsonConvert.DeserializeObject<AWSCredentials>(jsonText);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to read AWS credentials: " + e.Message);
            return null;
        }
    }
}

[Serializable]
public class AWSCredentials
{
    public string AWSAccessKeyId;
    public string AWSSecretKey;
}
