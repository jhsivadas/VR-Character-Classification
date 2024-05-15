// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Google.Cloud.Storage.V1;
// using Google.Apis.Auth.OAuth2;
// using System;
// using System.IO;

// public class GoogleCloud : MonoBehaviour
// {
//     private string bucketName = "digits-vr";
//     private string uploadName = "7_Z.csv";
//     private string responseName = "response.txt";
//     private string uploadFilePath;
//     private string serviceAccountJsonPath;
//     private DateTime lastCheckedTime;

//     void Start()
//     {
//         serviceAccountJsonPath = Path.Combine(Application.dataPath, "googlecloud_credentials.json");  
//         uploadFilePath = Path.Combine(Application.dataPath, uploadName);
//         StartCoroutine(UploadAndReadFile(uploadFilePath));
//     }

//     IEnumerator UploadAndReadFile(string filePath)
//     {
//         // Upload file
//         try
//         {
//             var credential = GoogleCredential.FromFile(serviceAccountJsonPath);
//             var storageClient = StorageClient.Create(credential);

//             using (var fileStream = File.OpenRead(filePath))
//             {
//                 storageClient.UploadObject(bucketName, uploadName, null, fileStream);
//                 Debug.Log("Google File uploaded successfully.");
//                 lastCheckedTime = DateTime.UtcNow;
//             }
//         }
//         catch (Exception ex)
//         {
//             Debug.LogError($"An error occurred: {ex.Message}");
//             yield break; // Stop the coroutine if the upload fails
//         }

//         // Read response file
//         StartCoroutine(PollForFileUpdate());
//     }

//     IEnumerator PollForFileUpdate()
//     {
//         bool fileUpdated = false;

//         while (!fileUpdated)
//         {
//             yield return new WaitForSeconds(1); // Polling interval

//             try
//             {
//                 var credential = GoogleCredential.FromFile(serviceAccountJsonPath);
//                 var storageClient = StorageClient.Create(credential);
//                 var obj = storageClient.GetObject(bucketName, responseName);

//                 if (obj.Updated.HasValue && obj.Updated.Value.ToUniversalTime() > lastCheckedTime)
//                 {
//                     fileUpdated = true;
//                     Debug.Log("Response file has been updated.");
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Debug.LogError($"Failed to check file update status: {ex.Message}");
//                 yield break;
//             }
//         }

//         ReadFile();
//     }

//     void ReadFile()
//     {
//         Debug.Log("Reading...");
//         try
//         {
//             var credential = GoogleCredential.FromFile(serviceAccountJsonPath);
//             var storageClient = StorageClient.Create(credential);
//             MemoryStream memoryStream = new MemoryStream();

//             storageClient.DownloadObject(bucketName, responseName, memoryStream);
//             memoryStream.Position = 0;

//             StreamReader reader = new StreamReader(memoryStream);
//             string fileContents = reader.ReadToEnd();

//             Debug.Log($"Response File Contents: {fileContents}");
//         }
//         catch (Exception ex)
//         {
//             Debug.LogError($"Failed to download the response file: {ex.Message}");
//         }
//     }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using System;
using System.IO;
using System.Linq;
using System.Collections.Concurrent;

public class GoogleCloud : MonoBehaviour
{
    private string bucketName = "digits-vr";
    private string responseName = "response.txt";
    private string uploadFolderPath;
    private string serviceAccountJsonPath;
    private ConcurrentQueue<string> fileQueue = new ConcurrentQueue<string>();

    void Start()
    {
        serviceAccountJsonPath = Path.Combine(Application.dataPath, "googlecloud_credentials.json");
        uploadFolderPath = Path.Combine(Application.dataPath, "Data");

        foreach (string uploadFilePath in Directory.GetFiles(uploadFolderPath, "*.csv"))
        {
            fileQueue.Enqueue(uploadFilePath);
        }

        StartCoroutine(ProcessFiles());
    }

    IEnumerator ProcessFiles()
    {
        while (fileQueue.TryDequeue(out string uploadFilePath))
        {
            yield return StartCoroutine(UploadAndReadFile(uploadFilePath, Path.GetFileName(uploadFilePath)));
        }
    }

    IEnumerator UploadAndReadFile(string uploadFilePath, string uploadName)
    {
        try
        {
            var credential = GoogleCredential.FromFile(serviceAccountJsonPath);
            var storageClient = StorageClient.Create(credential);

            using (var fileStream = File.OpenRead(uploadFilePath))
            {
                storageClient.UploadObject(bucketName, uploadName, null, fileStream);
                Debug.Log($"{uploadName} uploaded successfully.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"An error occurred while uploading {uploadName}: {ex.Message}");
            yield break;
        }

        // Wait for and read response
        yield return StartCoroutine(WaitAndReadResponse());
    }

    IEnumerator WaitAndReadResponse()
    {
        ilovejayText.text = "trying to get response";
        bool responseReceived = false;
        DateTime lastCheckedTime = DateTime.UtcNow;
        string current = ilovejayText.text;

        while (!responseReceived)
        {
            yield return new WaitForSeconds(1);

            try
            {
                var credential = GoogleCredential.FromFile(serviceAccountJsonPath);
                var storageClient = StorageClient.Create(credential);
                var obj = storageClient.GetObject(bucketName, responseName);

                ilovejayText.text = $"{obj.Updated.Value.ToUniversalTime()}, {lastCheckTime}";
                
                if (obj.Updated.HasValue && obj.Updated.Value.ToUniversalTime() > lastCheckedTime)
                {
                    responseReceived = true;
                    MemoryStream memoryStream = new MemoryStream();
                    storageClient.DownloadObject(bucketName, responseName, memoryStream);
                    memoryStream.Position = 0;
                    StreamReader reader = new StreamReader(memoryStream);
                    string fileContents = reader.ReadToEnd();
                    ilovejayText.text = current + fileContents;
                }
            }
            catch (Exception ex)
            {
                ilovejayText.text = "Failed to check or read response file: " + ex.Message;
                yield break;
            }
        }
    }
}