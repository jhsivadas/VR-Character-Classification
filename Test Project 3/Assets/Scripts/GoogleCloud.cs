using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using System;
using System.IO;
using TMPro;

public class GoogleCloud : MonoBehaviour
{
    private string bucketName = "digits-vr";
    // private string uploadName = "sexyjay.csv";
    // private string uploadName; 
    private string responseName = "response.txt";
    private string uploadName = "positionalData.csv";
    private string uploadFilePath;
    private string serviceAccountJsonPath;
    private DateTime lastCheckedTime;
    public TMP_Text ilovejayTextGCP;


    void Start()
    {

        serviceAccountJsonPath = Path.Combine(Application.persistentDataPath, "googlecloud_credentials.json");  
        uploadFilePath = Path.Combine(Application.persistentDataPath, uploadName); //Path.Combine(Application.dataPath, uploadName);

        ilovejayTextGCP.text = "GCP";
        // StartCoroutine(UploadAndReadFile(uploadFilePath));
    }

    public IEnumerator UploadAndReadFile()
    {
        // ilovejayTextGCP.text = "trying to upload";
        // Upload file
        try
        {
            var credential = GoogleCredential.FromFile(serviceAccountJsonPath);
            var storageClient = StorageClient.Create(credential);

            using (var fileStream = File.OpenRead(uploadFilePath))
            {
                storageClient.UploadObject(bucketName, uploadName, null, fileStream);
                ilovejayTextGCP.text = "Google File uploaded successfully.";
                lastCheckedTime = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            ilovejayTextGCP.text = $"An error occurred: {ex.Message}";
            yield break; // Stop the coroutine if the upload fails
        }

        // Read response file
        StartCoroutine(PollForFileUpdate());
    }

    IEnumerator PollForFileUpdate()
    {
        bool fileUpdated = false;

        while (!fileUpdated)
        {
            yield return new WaitForSeconds(1); // Polling interval

            try
            {
                var credential = GoogleCredential.FromFile(serviceAccountJsonPath);
                var storageClient = StorageClient.Create(credential);
                var obj = storageClient.GetObject(bucketName, responseName);

                if (obj.Updated.HasValue && obj.Updated.Value.ToUniversalTime() > lastCheckedTime)
                {
                    fileUpdated = true;
                    Debug.Log("Response file has been updated.");
                }
            }
            catch (Exception ex)
            {
                ilovejayTextGCP.text = $"Failed to check file update status: {ex.Message}";
                yield break;
            }
        }

        ReadFile();
    }

    void ReadFile()
    {
        Debug.Log("Reading...");
        try
        {
            var credential = GoogleCredential.FromFile(serviceAccountJsonPath);
            var storageClient = StorageClient.Create(credential);
            MemoryStream memoryStream = new MemoryStream();

            storageClient.DownloadObject(bucketName, responseName, memoryStream);
            memoryStream.Position = 0;

            StreamReader reader = new StreamReader(memoryStream);
            string fileContents = reader.ReadToEnd();

            // Debug.Log($"Response File Contents: {fileContents}");
            ilovejayTextGCP.text = fileContents;
        }
        catch (Exception ex)
        {
            ilovejayTextGCP.text = $"Failed to download the response file: {ex.Message}";
        }
    }
}
