using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ImgcodecsModule;
using System;
using System.Runtime.InteropServices;


public class Frame3DFacebook : MonoBehaviour
{
        
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();
    public Frame3D frame3D{get;set;}

    protected string filePath;
    protected string outpath;
    // Start is called before the first frame update
    void Start()
    {
        // establish the save directory and file path
        outpath = Application.persistentDataPath + "/3dImages/";
        Directory.CreateDirectory(outpath);
        filePath = outpath + "/image";
    }

    // Update is called once per frame
    void Update()
    {
        if(frame3D.snapped){
            Debug.Log("writing color to " + filePath + ".png");
            
            Mat colorFlipped = new Mat();
            Core.flip(frame3D.waterMarkedColor, colorFlipped, 0);
            Mat rgb = new Mat();
            Imgproc.cvtColor(colorFlipped, rgb, Imgproc.COLOR_RGB2BGR);
            Imgcodecs.imwrite(filePath + ".png", rgb);
            Mat depthFlipped = new Mat();
            Core.flip(frame3D.waterMarkedDepth, depthFlipped, 0);
            Debug.Log("writing depth to " + filePath + "_depth.png");
            Imgcodecs.imwrite(filePath + "_depth.png", depthFlipped); 
            
            // Load the directory where the images are saved 
            Debug.Log("explorer.exe"+" /n, /e, "+ outpath.Replace('/', '\\'));
            System.Diagnostics.Process.Start("explorer.exe","/n, /e, " + outpath.Replace('/', '\\'));
            ShowWindow(GetActiveWindow(), 2);

        }
    }
}
