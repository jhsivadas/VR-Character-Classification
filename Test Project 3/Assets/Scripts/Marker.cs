using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using TMPro;


public class Marker : MonoBehaviour
{
    public List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();
    private static Vector3 markerCenter;

    public void RefreshDevices()
    {
        devices.Clear();
        InputDevices.GetDevices(devices);
    }

    void Start()
    {
        markerCenter = transform.position;
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
            Vector3 controllerPosition;
            right.TryGetFeatureValue(CommonUsages.devicePosition, out controllerPosition);

            Vector3 adjustment = new Vector3(0.0375f, 0, 0);
            Vector3 change = controllerPosition;
            Vector3 newPos = change + markerCenter + adjustment;
            transform.position = newPos;
        }
        else if (fTLpressed)
        {
            Vector3 controllerPosition;
            left.TryGetFeatureValue(CommonUsages.devicePosition, out controllerPosition);

            Vector3 adjustment = new Vector3(0.0375f, 0, 0);
            Vector3 change = controllerPosition;
            Vector3 newPos = change + markerCenter + adjustment;
            transform.position = newPos;
        }
        else
        {
            transform.position = markerCenter;
        }

        // transform.position = controllerPosition;

        
    }
}
