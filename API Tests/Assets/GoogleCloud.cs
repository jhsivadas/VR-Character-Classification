using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using System;
using System.IO;

public class GoogleCloud : MonoBehaviour
{
    private string bucketName = "max_test_123";
    private string objectName = "sexyjay.csv";
    private string filePath;
    private string serviceAccountJsonPath;

    void Start()
    {
        filePath = Path.Combine(Application.dataPath, objectName);
        serviceAccountJsonPath = Path.Combine(Application.dataPath, "googlecloud_credentials.json");   

        UploadFile();
    }

    void UploadFile()
    {
        try
        {
            var credential = GoogleCredential.FromFile(serviceAccountJsonPath);
            var storageClient = StorageClient.Create(credential);

            using (var fileStream = File.OpenRead(filePath))
            {
                storageClient.UploadObject(bucketName, objectName, null, fileStream);
                Debug.Log("Google File uploaded successfully.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"An error occurred: {ex.Message}");
        }
    }
}
