using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Intel.RealSense;

public class ContextManager : MonoBehaviour
{
    public GameObject RSDevicePrefab;
    
    public event System.Action<Texture> Action2;
    public GameObject LogText;

    // Start is called before the first frame update
    void Start()
    {
        int d435Count = 0;
        Context context = new Context();
        foreach(var device in context.Devices){
            string serial = device.Info[CameraInfo.SerialNumber];
            Debug.Log( device.Info[CameraInfo.Name]);
            if(!device.Info[CameraInfo.Name].Contains("D435"))
                continue;
            d435Count++;
            var RScamera = Instantiate(RSDevicePrefab);
            //RScamera.GetComponentInChildren<RsDevice>().DeviceConfiguration.RequestedSerialNumber = serial;
            RScamera.GetComponentInChildren<RsDevice>().enabled = true;
            //var mat = new Material(Shader.Find("Custom/PointCloudGeom"));
            //RScamera.GetComponentInChildren<TextureBinder>().materialToUpdate = mat;
            //RScamera.GetComponentInChildren<RsPointCloudRenderer>().GetComponent<MeshRenderer>().material = mat;
        }

        if(d435Count > 1)
            LogText.GetComponentInChildren<Text>().text = "Please ensure only a single RealSense depth camera is connected";
    }

    // Update is called once per frame
    void Update()
    {
    
    }
}
