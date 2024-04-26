using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.Networking;
using Amazon.S3;
using Amazon.S3.Model;
using System;

public class AWS : MonoBehaviour
{
    private string bucketName = "digits-vr";
    private string keyName = "HOLYCOW.csv"; // Key name under which the file will be saved in the bucket
    private string filePath = "/Users/max/Desktop/VR-Character-Classification/API Tests/Assets/HOLYCOW.csv"; // Local path to file

    void Start()
    {
        UploadFile();
    }

    public void onRefresh()
    {
        Start();
    }

    private void UploadFile()
    {
        AmazonS3Client s3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, Amazon.RegionEndpoint.USEast1);
        
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
}