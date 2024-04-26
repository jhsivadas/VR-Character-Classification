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
    private StreamWriter writer;
    private List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();
    public TMP_Text ilovejayText;

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
        string path = Path.Combine(Application.persistentDataPath, filename);
        string header = "x,y,z";

        writer = new StreamWriter(path);
        writer.WriteLine(header);
    }

    // Start is called before the first frame update
    void Start()
    {
        // transform.position = new Vector3(-32, 10, -1);
        createCSV();
        // lol do I even need anything here
        // theoretically a check but I'll do that later
    }

    void LogAttributes()
    {
        foreach (var pos in positions)
        {
            string entry = $"{pos.x},{pos.y},{pos.z},";
            writer.WriteLine(entry);
        }

        positions.Clear();
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

        if (fTRpressed)
        {
            ilovejayText.text = "I really love Jay";
            positions.Add(GetCurrentReading(right));
            // drawing code
        }

        if (fTLpressed)
        {
            ilovejayText.text = "I LOVE Jay";
            positions.Add(GetCurrentReading(left));
            // drawing code
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
