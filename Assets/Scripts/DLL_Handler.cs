using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using TMPro;
using System;
using System.Text;


public class DLL_Handler : MonoBehaviour
{
    [DllImport("TEST_DLL_1.dll")]
    public static extern int getRandomNumber();
    // Start is called before the first frame update
    void Start()
    {
        int a = getRandomNumber();
        Debug.Log(a);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
