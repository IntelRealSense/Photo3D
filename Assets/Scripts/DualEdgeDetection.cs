using UnityEngine;
using UnityEngine.Events;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;


using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.PhotoModule;
using OpenCVForUnity.XimgprocModule;
using OpenCVForUnity.VideoModule;
// using OpenCVForUnity.OptflowModule;
public class DualEdgeDetection : MonoBehaviour
{
    public Texture LeftIrTexture{get;set;}
    public Texture RightIrTexture{get;set;}

    [System.Serializable]
    public class TextureEvent : UnityEvent<Texture> { }
   
    public TextureEvent xTextureEvent;
    public TextureEvent flowTextureEvent;
    
    public TextureEvent yTextureEvent;
    
    [Range(0, 255)]
    public int threshold1 = 50;
    [Range(0, 255)] 
    public int threshold2 = 110;

    public enum KernalSize 
     {
         one = 1,
         Three = 3,
         Five = 5,
         Seven = 7,
         Nine = 9
    }
     public KernalSize ksize;

    public enum FlowPlane
     {
         x = 0,
         y = 1,
         z = 2
    }

    public FlowPlane flowPlane = 0;

    [Range(0, 3)]     
    public double scale = 1.0;

    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        Mat LeftIRMat = Util.toMat((Texture2D)LeftIrTexture, CvType.CV_8UC1);
        Mat RightIRMat = Util.toMat((Texture2D)LeftIrTexture, CvType.CV_8UC1);

        Mat XLeft = new Mat();
        Mat YLeft = new Mat();
        Mat XRight = new Mat();
        Mat YRight = new Mat();
        Imgproc.Sobel(LeftIRMat, XLeft, CvType.CV_32F, 1, 0, (int)ksize, scale);
        Imgproc.Sobel(LeftIRMat, YLeft, CvType.CV_32F, 0, 1, (int)ksize, scale);
        Imgproc.Sobel(RightIRMat, XRight, CvType.CV_32F, 1, 0, (int)ksize, scale);
        Imgproc.Sobel(RightIRMat, YRight, CvType.CV_32F, 0, 1, (int)ksize, scale);
        Mat x = XLeft + XRight;
        Mat y = XRight + (YRight);

        DenseOpticalFlow opticalFlow = DISOpticalFlow.create(DISOpticalFlow.PRESET_MEDIUM);

        //OpenCVForUnity.VideoModule.DenseOpticalFlow opticalFlow = new OpenCVForUnity.VideoModule.DenseOpticalFlow(IntPtr.Zero);
        

        Texture2D xTexture = (Texture2D)Util.toTexture(x, TextureFormat.RFloat);
        xTextureEvent.Invoke(xTexture);
        
        Texture2D yTexture =  (Texture2D)Util.toTexture(y, TextureFormat.RFloat);
        yTextureEvent.Invoke(yTexture);

        Mat x8Bit = new Mat();
        Mat y8Bit = new Mat();
        x.convertTo(x8Bit, CvType.CV_8UC1, 0.1f);
        y.convertTo(y8Bit, CvType.CV_8UC1, 0.1f);

        Mat flow = new Mat();
        opticalFlow.calc(y8Bit,x8Bit, flow);
        List<Mat> planes = new List<Mat> ();
         // split
        Core.split (flow, planes);

        Texture2D flowTexture = new Texture2D(LeftIrTexture.width, LeftIrTexture.height, TextureFormat.RFloat, false);
        Utils.fastMatToTexture2D(planes[(int)flowPlane], flowTexture);
        flowTextureEvent.Invoke(flowTexture);


        //Mat canneyRslt = new Mat();
        //Imgproc.Canny(,,IRMat, canneyRslt, threshold1, threshold2);

        
        //Imgproc.Canny(IRMat, canneyRslt, threshold1, threshold2);

    }
}
