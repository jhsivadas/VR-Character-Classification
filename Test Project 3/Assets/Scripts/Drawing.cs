using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using TMPro;

// using Oculus;

public class Drawing : MonoBehaviour
{
    public List<UnityEngine.Vector3> positions = new List<UnityEngine.Vector3>();
    public string path;
    public List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();
    public int letterNum = 0;
    public int waitcycle = 0;
    public TMP_Text ilovejayText;
    public GameObject marker;
    public Camera renderCamera;
    // public TMP_Text responseText;

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

    void createCSV()
    {
        string filename = $"positionalData.csv";
        path = Path.Combine(Application.persistentDataPath, filename);
        string header = "controller_right_pos.x,controller_right_pos.y,controller_right_pos.z";

        using (StreamWriter writer = new StreamWriter(path)) 
        {
            writer.WriteLine(header);
        }
    }

    void save_Data()
    {
        Storage.instance.letterNum = letterNum;
        Storage.instance.path = path;
        Storage.instance.waitcycle = waitcycle;
        Storage.instance.ilovejayText = ilovejayText;
    }

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
        marker = GameObject.Find("Marker");
        GameObject rC = GameObject.Find("Camera");
        renderCamera = null;
        if (rC != null)
        {
            renderCamera = rC.GetComponent<Camera>();
        } 
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
        using (StreamWriter writer = new StreamWriter(path, true)) 
        {
            foreach (var pos in positions)
            {
                string entry = $"{pos.x},{pos.y},{pos.z},";
                writer.WriteLine(entry);
            }
        }

        positions.Clear();
        letterNum += 1;
        // save_Data();
        // SceneManager.LoadScene(GetActiveScene().name);

    }

    // Update is called once per frame
    void Update()
    {
        RefreshDevices();

        InputDeviceCharacteristics rightController = (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right);
        InputDeviceCharacteristics leftController = (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left);

        InputDevice right = devices[0];
        InputDevice left = devices[0];

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
            // drawing code
        }

        if (aPressed && (waitcycle == 0))
        {
            ilovejayText.text = "A pressed?";
            LogAttributes();
            // clear_Screen();
            waitcycle = 100;

        }

        if (bPressed)
        {
            ilovejayText.text = "B pressed?";
        }

        if (waitcycle > 0)
        {
            waitcycle -= 1;
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
        
    }
}
