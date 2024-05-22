using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using TMPro;
using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Concurrent;

// using Oculus;

public class Drawing : MonoBehaviour
{
    // Global variables used throughout the program

    // List that contains the position data
    public List<UnityEngine.Vector3> positions = new List<UnityEngine.Vector3>();

    // List that contains active devices
    public List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();

    // Certain functions need to wait a number of frames to go again
    // this variable is used to measure how many frames we should wait for
    public int waitcycle = 0;

    // The main TextMesh that receives the string to be displayed 
    public TMP_Text ilovejayText;

    // String that denotes the path to the current file
    private string filePath; 

    // String that denotes the path to the file containing the letter csvs
    private string folderPath;

    // Max?
    private DateTime lastCheckedTime;

    private string bucketName = "digits-vr";
    private string responseName = "response.txt";
    private string uploadFolderPath;
    private string serviceAccountJsonPath;
    private ConcurrentQueue<string> fileQueue = new ConcurrentQueue<string>();

    // Provides the current position of this device
    public Vector3 GetCurrentReading(UnityEngine.XR.InputDevice device)
    {
        Vector3 recordedValue;
        device.TryGetFeatureValue(CommonUsages.devicePosition, out recordedValue);
        return recordedValue;
    }

    // Refresh the list of devices currently connected
    public void RefreshDevices()
    {
        devices.Clear();
        InputDevices.GetDevices(devices);
    }

    // checks how many files are already in the folder
    int letterNum()
    {
        string[] filepaths = Directory.GetFiles(folderPath);
        List<string> actualFiles = new List<string>();

        foreach (var filePath in filepaths)
        {
            actualFiles.Add(Path.GetFileName(filePath));
        }

        int largest = 0;

        if (filepaths.Length == 0)
        {
            return -1;
        }
        else
        {
            foreach (var file in actualFiles)
            {
                string num = file.Substring(15);
                num = num.Substring(0, num.Length - 4);
                int number = int.Parse(num);

                if (number > largest)
                {
                    largest = number;
                }           
            }

            return largest;
        }
    }

    // Create the CSV file
    void createCSV()
    {
        int numberOfFiles = letterNum() + 1;
        string filename = $"positionalData_" + numberOfFiles + ".csv";
        filePath = Path.Combine(folderPath, filename);
        string header = "controller_right_pos.x,controller_right_pos.y,controller_right_pos.z";

        using (StreamWriter writer = new StreamWriter(filePath)) 
        {
            writer.WriteLine(header);
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        string foldername = "positionalData";
        folderPath = Path.Combine(Application.persistentDataPath, foldername);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        GameObject rC = GameObject.Find("Camera");

        serviceAccountJsonPath = Path.Combine(Application.persistentDataPath, "googlecloud_credentials.json");
        uploadFolderPath = folderPath;
    }

    // Log the positional data in a csv file
    void LogAttributes()
    {
        createCSV();
        using (StreamWriter writer = new StreamWriter(filePath, true)) 
        {
            foreach (var pos in positions)
            {
                string entry = $"{pos.x},{pos.y},{pos.z},";
                writer.WriteLine(entry);
            }
        }

        positions.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        // check to make sure the same devices are still connected
        RefreshDevices();

        // create a mask for identifying the right and left controllers
        InputDeviceCharacteristics rightController = (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right);
        InputDeviceCharacteristics leftController = (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left);

        // for now, set these to defaul so they can be accessed outside 
        // the following for loop
        InputDevice right = default; 
        InputDevice left = default; 

        // check all connected devices, finding the right and left controller
        foreach (var dev in devices)
        {
            if ((dev.characteristics & rightController) == rightController)
            {
                right = dev;
            }
            else if ((dev.characteristics & leftController) == leftController)
            {
                left = dev;
            }
        }

        // if they've been found
        if (right != default)
        { 
            // trigger value is the amount of pressure applied to that button
            float triggerValue = 0;

            // get the pressure applied to the right front trigger
            right.TryGetFeatureValue(CommonUsages.trigger, out triggerValue);
            // if its over 0.05, that's my threshold for it being pressed
            bool fTRpressed = triggerValue > 0.05;

            // similar for left front trigger
            left.TryGetFeatureValue(CommonUsages.trigger, out triggerValue);
            bool fTLpressed = triggerValue > 0.05;

            // slightly different code, but directly checks if each button has
            // been pressed
            bool aPressed;
            right.TryGetFeatureValue(CommonUsages.primaryButton, out aPressed);
            bool bPressed;
            right.TryGetFeatureValue(CommonUsages.secondaryButton, out bPressed);
            bool xPressed;
            left.TryGetFeatureValue(CommonUsages.primaryButton, out xPressed);
            bool yPressed;
            left.TryGetFeatureValue(CommonUsages.secondaryButton, out yPressed);

            // Has the right trigger been pressed?
            if (fTRpressed)
            {
                Vector3 currentPos = GetCurrentReading(right);
                positions.Add(currentPos);
            }

            // Has the left trigger been pressed?
            if (fTLpressed)
            {
                // ilovejayText.text = "I LOVE Jay";
                Vector3 currentPos = GetCurrentReading(left);
                positions.Add(currentPos);
            }

            // Has A been pressed
            // Waitcycle is put here so it waits a certain amount of frames
            // before checking A again, so a bunch of files are not produced
            // each second
            if (aPressed && (waitcycle == 0))
            {
                LogAttributes();
                waitcycle = 50;
            }

            // Has be been pressed
            // waitcycle here for the same reason
            if (bPressed && (waitcycle == 0))
            {
                LogAttributes();

                /*
                var notSortedFiles = Directory.GetFiles(uploadFolderPath, "*.csv");
                Array.Sort(notSortedFiles);
                var sortedFiles = notSortedFiles.ToList();

                           .OrderBy(filePath => 
                           {
                               string fileName = Path.GetFileNameWithoutExtension(filePath);
                               return int.Parse(new string(fileName.Where(char.IsDigit).ToArray()));
                           })
                           .ToList();
                */

                var sortedFiles = Directory.GetFiles(uploadFolderPath, "*.csv")
                           .OrderBy(filePath => 
                           {
                               string fileName = Path.GetFileNameWithoutExtension(filePath);
                               return int.Parse(new string(fileName.Where(char.IsDigit).ToArray()));
                           })
                           .ToList();

                foreach (string uploadFilePath in sortedFiles)
                {
                    fileQueue.Enqueue(uploadFilePath);
                }
                StartCoroutine(ProcessFiles());
            }

            // erase button
            if (xPressed)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            // new word, also deletes the directory
            if (yPressed)
            {
                Directory.Delete(folderPath, true);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            
            // decrements waitcycle
            if (waitcycle > 0)
            {
                waitcycle -= 1;
            }
        }
    }

    IEnumerator ProcessFiles()
    {
        // ilovejayText.text = fileQueue.Count.ToString();
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
            var obj = storageClient.GetObject(bucketName, responseName);
            // ilovejayText.text = "upload started";
            lastCheckedTime = obj.Updated.Value.ToUniversalTime();

            using (var fileStream = File.OpenRead(uploadFilePath))
            {
                storageClient.UploadObject(bucketName, uploadName, null, fileStream);
                // ilovejayText.text = $"{uploadName} uploaded successfully.";
            }
        }
        catch (Exception ex)
        {
            ilovejayText.text = $"An error occurred while uploading {uploadName}: {ex.Message}";
            yield break;
        }

        // Wait for and read response
        yield return StartCoroutine(WaitAndReadResponse());
    }

    IEnumerator WaitAndReadResponse()
    {
        bool responseReceived = false;
        string current = ilovejayText.text;
        float backoffTime = 0.5f;
        const float maxBackoffTime = 8.0f;

        while (!responseReceived)
        {
            yield return new WaitForSeconds(backoffTime);

            try
            {
                var credential = GoogleCredential.FromFile(serviceAccountJsonPath);
                var storageClient = StorageClient.Create(credential);
                var obj = storageClient.GetObject(bucketName, responseName);
                
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
                else 
                {
                    responseReceived = false;
                    backoffTime = Mathf.Min(backoffTime * 2, maxBackoffTime);
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

