using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using TMPro;

public class ResetScreen : MonoBehaviour
{
    private Color original; 
    private Material mat;

    public List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();

    public void RefreshDevices()
    {
        devices.Clear();
        InputDevices.GetDevices(devices);
    }

    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<Renderer>().material;
        original = mat.color;   
    }

    // Update is called once per frame
    void Update()
    {
        RefreshDevices();

        InputDeviceCharacteristics rightController = (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right);
        InputDeviceCharacteristics leftController = (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left);

        InputDevice right = default;
        InputDevice left = default;

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

            bool aPressed;
            right.TryGetFeatureValue(CommonUsages.primaryButton, out aPressed);

            if (aPressed)
            {
                mat.color = original;
            }
        }
    }
}
