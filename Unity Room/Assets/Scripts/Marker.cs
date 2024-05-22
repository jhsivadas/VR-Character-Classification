using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using TMPro;


// This is the file for the marker
public class Marker : MonoBehaviour
{
    // variable for all connected devices
    public List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();
    // vector3 that gives the marker's original center
    private static Vector3 markerCenter;

    // Refresh the list of devices currently connected 
    public void RefreshDevices()
    {
        devices.Clear();
        InputDevices.GetDevices(devices);
    }

    // set markercenter, so it knows how to adjust things
    void Start()
    {
        markerCenter = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // refresh the devices connected
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

            // If the fTR has been pressed, move the marker to that location
            if (fTRpressed)
            {
                Vector3 controllerPosition;
                right.TryGetFeatureValue(CommonUsages.devicePosition, out controllerPosition);

                Vector3 adjustment = new Vector3(0.0375f, 0, 0);
                Vector3 change = controllerPosition;
                Vector3 newPos = change + markerCenter + adjustment;
                transform.position = newPos;
            }
            // If the fTL has been pressed, move the marker to that location
            else if (fTLpressed)
            {
                Vector3 controllerPosition;
                left.TryGetFeatureValue(CommonUsages.devicePosition, out controllerPosition);

                Vector3 adjustment = new Vector3(0.0375f, 0, 0);
                Vector3 change = controllerPosition;
                Vector3 newPos = change + markerCenter + adjustment;
                transform.position = newPos;
            }
            // reset its position if no buttons are being pressed
            else
            {
                transform.position = markerCenter;
            }
        }
    }
}
