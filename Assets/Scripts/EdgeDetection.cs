using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.PhotoModule;
using System.Runtime.InteropServices;
using OpenCVForUnity.XimgprocModule;
public class EdgeDetection : MonoBehaviour
{
    public Texture IRtexture{get;set;}

    [System.Serializable]
    public class TextureEvent : UnityEvent<Texture> { }
    public TextureEvent textureBinding;

    [Range(0, 255)]
    public int threshold1 = 50;
    [Range(0, 255)] 
    public int threshold2 = 110;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Mat IRMat = Util.toMat((Texture2D)IRtexture, CvType.CV_8UC3);

        Mat canneyRslt = new Mat();
        Imgproc.Canny(IRMat, canneyRslt, threshold1, threshold2);

        Texture2D cannyTexture =  (Texture2D)Util.toTexture(canneyRslt, TextureFormat.R8);
        textureBinding.Invoke(cannyTexture);
    }
}
