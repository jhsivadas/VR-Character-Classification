using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using TMPro;

public class Storage : MonoBehaviour
{
    public int letterNum = 0;
    public int waitcycle = 0;
    public TMP_Text ilovejayText;
    public List<UnityEngine.Vector3> positions = new List<UnityEngine.Vector3>();
    public string path;
    
    public static Storage instance;

    void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}