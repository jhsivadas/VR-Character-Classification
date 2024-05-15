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
    public List<UnityEngine.Vector3> positions = new List<UnityEngine.Vector3>();
    public List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();
   // public int letterNum = 0;
    public int waitcycle = 0;
    public TMP_Text ilovejayText;
    public GameObject marker;
    public Camera renderCamera;
    private string filePath; 
    private string folderPath;

    private string bucketName = "digits-vr";
    private string responseName = "response.txt";
    private string uploadFolderPath;
    private string serviceAccountJsonPath;
    private ConcurrentQueue<string> fileQueue = new ConcurrentQueue<string>();

    public Vector3 GetCurrentReading(UnityEngine.XR.InputDevice device)
    {
        Vector3 recordedValue;
        device.TryGetFeatureValue(CommonUsages.devicePosition, out recordedValue);
        return recordedValue;
    }

    public void RefreshDevices()
    {
        devices.Clear();
        InputDevices.GetDevices(devices);
    }

    int letterNum()
    {
        // ilovejayText.text = "LetterNum";
        string[] filepaths = Directory.GetFiles(folderPath);
        List<string> actualFiles = new List<string>();

        foreach (var filePath in filepaths)
        {
            actualFiles.Add(Path.GetFileName(filePath));
        }
        // ilovejayText.text = "Got through getting the files";

        // files will be called positionalData_#.csv
        // number will be 16th element (15 position)

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
                // ilovejayText.text = num;
                int number = int.Parse(num);
                // ilovejayText.text = "iloveJay";

                if (number > largest)
                {
                    largest = number;
                }           
            }
            // ilovejayText.text = "return value";
            return largest;
        }
    }

    void createCSV()
    {
        int numberOfFiles = letterNum() + 1;
        string filename = $"positionalData_" + numberOfFiles + ".csv";
        filePath = Path.Combine(folderPath, filename);
        // path = Path.Combine(Application.persistentDataPath, filename);
        string header = "controller_right_pos.x,controller_right_pos.y,controller_right_pos.z";

        using (StreamWriter writer = new StreamWriter(filePath)) 
        {
            writer.WriteLine(header);
        }
        
    }

    /*

    void save_Data()
    {
        Storage.instance.letterNum = letterNum;
        Storage.instance.path = path;
        Storage.instance.waitcycle = waitcycle;
        Storage.instance.ilovejayText = ilovejayText;
    }*/ 

/*
    void clear_Screen()
    {
        CameraClearFlags original = renderCamera.clearFlags;
        renderCamera.clearFlags = CameraClearFlags.Skybox;
        renderCamera.Render();
        renderCamera.clearFlags = original;
    }*/

    // Start is called before the first frame update
    void Start()
    {
        string foldername = "positionalData";
        folderPath = Path.Combine(Application.persistentDataPath, foldername);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        marker = GameObject.Find("Marker");
        GameObject rC = GameObject.Find("Camera");
        renderCamera = null;
        if (rC != null)
        {
            renderCamera = rC.GetComponent<Camera>();
        } 

        serviceAccountJsonPath = Path.Combine(Application.persistentDataPath, "googlecloud_credentials.json");
        uploadFolderPath = folderPath;

        // ilovejayText.text = Directory.GetFiles(folderPath, "*.csv").Length.ToString();

        /*
        letterNum = Storage.instance.letterNum;
        path = Storage.instance.path;
        waitcycle = Storage.instance.waitcycle;
        ilovejayText = Storage.instance.ilovejayText;

        */
        // transform.position = new Vector3(-32, 10, -1);
        // createCSV();
        // lol do I even need anything here
        // theoretically a check but I'll do that later
    }

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
        // letterNum += 1;
        // save_Data();
        // SceneManager.LoadScene(GetActiveScene().name);

    }

    // Update is called once per frame
    void Update()
    {
        RefreshDevices();

        InputDeviceCharacteristics rightController = (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right);
        InputDeviceCharacteristics leftController = (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left);

        InputDevice right = default; // devices[0];
        InputDevice left = default; //devices[0];

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

        if (right != default)
        { 
            float triggerValue = 0;

            right.TryGetFeatureValue(CommonUsages.trigger, out triggerValue);
            bool fTRpressed = triggerValue > 0.05;
            left.TryGetFeatureValue(CommonUsages.trigger, out triggerValue);
            bool fTLpressed = triggerValue > 0.05;

            // right.TryGetFeatureValue(CommonUsages.primaryButton, out triggerValue);
            bool aPressed;
            right.TryGetFeatureValue(CommonUsages.primaryButton, out aPressed);
            bool bPressed;
            right.TryGetFeatureValue(CommonUsages.secondaryButton, out bPressed);
            bool xPressed;
            left.TryGetFeatureValue(CommonUsages.primaryButton, out xPressed);
            bool yPressed;
            left.TryGetFeatureValue(CommonUsages.secondaryButton, out yPressed);

            Vector3 controllerPosition;
            right.TryGetFeatureValue(CommonUsages.devicePosition, out controllerPosition);

            // ilovejayText.text = controllerPosition.ToString();

            if (fTRpressed)
            {
                // ilovejayText.text = "I really love Jay";
                Vector3 currentPos = GetCurrentReading(right);
                // marker.transform.position = currentPos;
                positions.Add(currentPos);
                // drawing code
            }

            if (fTLpressed)
            {
                // ilovejayText.text = "I LOVE Jay";
                Vector3 currentPos = GetCurrentReading(left);
                // marker.transform.position = currentPos;
                positions.Add(currentPos);
            }
            if (aPressed && (waitcycle == 0))
            {
                // int letters = letterNum();
                // ilovejayText.text = $"A pressed?" + letters;
                LogAttributes();
                // clear_Screen();
                waitcycle = 50;
                // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            if (bPressed && (waitcycle == 0))
            {
                // ilovejayText.text = "B pressed?";
                LogAttributes();
                foreach (string uploadFilePath in Directory.GetFiles(uploadFolderPath, "*.csv"))
                {
                    // ilovejayText.text = uploadFilePath;
                    fileQueue.Enqueue(uploadFilePath);
                }
                StartCoroutine(ProcessFiles());
                // waitcycle = 50;
                // ilovejayText.text = "B pressed?";
            }

            // erased button
            if (xPressed)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            // new word
            if (yPressed)
            {
                Directory.Delete(folderPath, true);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

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
            // ilovejayText.text = "upload started";

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
        // ilovejayText.text = "trying to get response";
        bool responseReceived = false;
        DateTime lastCheckedTime = DateTime.UtcNow;
        string current = ilovejayText.text;

        while (!responseReceived)
        {
            yield return new WaitForSeconds(0.5f);

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
            }
            catch (Exception ex)
            {
                ilovejayText.text = "Failed to check or read response file: " + ex.Message;
                yield break;
            }
        }
    }
}




        /*
        InputDevice right;
        float triggerRValue = 0;
        InputDevice left;
        float triggerLValue = 0;

        foreach (var dev in devices)
        {
            if ((dev.characteristics & rightController) == rightController)
            {
                right = dev;
                right.TryGetFeatureValue(CommonUsages.trigger, out triggerRValue);
                bool fTRpressed = triggerLValue > 0.05;

                if (fTRpressed)
                {
                    ilovejayText.text = "I really love Jay";
                    positions.Add(GetCurrentReading(right));
                    // drawing code
                }
            }
            else if ((dev.characteristics & leftController) == leftController)
            {
                left = dev;
                left.TryGetFeatureValue(CommonUsages.trigger, out triggerLValue);
                bool fTLpressed = triggerLValue > 0.05;

                if (fTLpressed)
                {
                    ilovejayText.text = "I love Jay";
                    positions.Add(GetCurrentReading(left));
                    // drawing code
                }
            }
        }*/

        //bool fTRpressed = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        // bool fTLpressed = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
        
    // }

/*
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
                ilovejayText.text = "Google File uploaded successfully.";
                lastCheckedTime = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            ilovejayText.text = $"An error occurred: {ex.Message}";
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
                ilovejayText.text = $"Failed to check file update status: {ex.Message}";
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
            ilovejayText.text = fileContents;
        }
        catch (Exception ex)
        {
            ilovejayText.text = $"Failed to download the response file: {ex.Message}";
        }
    }
}*/
