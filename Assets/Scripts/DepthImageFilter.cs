using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.XimgprocModule;
using OpenCVForUnity.UnityUtils;


public class DepthImageFilter : MonoBehaviour
{
    // input images
    public Mat color{get;set;}
    public Texture depthTexture{get;set;}

    // output result
    [System.Serializable]
    public class MatEvent : UnityEvent<Mat> { }
    public MatEvent cleanedDepth;

    // intermediery results
    [System.Serializable]
    public class TextureEvent : UnityEvent<Texture> { }
    public TextureEvent sobelTextureEvent;
    public TextureEvent canneyTextureEvent;
    public TextureEvent improvedDepth;

    [Range(0, 255)]
    public int cannyThreshold1 = 50;

    [Range(0, 255)]
    public int cannyThreshold2 = 100;

    public enum KernalSize 
     {
         one = 1,
         Three = 3,
         Five = 5,
         Seven = 7,
         Nine = 9
    }
    public KernalSize ksize;

    [Range(0,255)]
    public int sigmaSpacial = 40;
    [Range(0,255)]
    public int sigmaColor = 40;

    [Range(1,5)]
    public int dtIter = 3;
    public float sobelScale = 0.01f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {      
        Debug.Log("Applying EdgeCleanup to Depth");
        Mat gray = new Mat();
        Imgproc.cvtColor(color, gray, Imgproc.COLOR_RGBA2GRAY);
        Mat sobelX = new Mat();
        Mat sobelY = new Mat();
        Imgproc.Sobel(gray, sobelX,  CvType.CV_16S, 1,0, (int)ksize, sobelScale, 0, Core.BORDER_DEFAULT);
        Imgproc.Sobel(gray, sobelY,  CvType.CV_16S, 0,1, (int)ksize, sobelScale, 0,  Core.BORDER_DEFAULT);
        
        Mat depth = Util.toMat((Texture2D)depthTexture, CvType.CV_8UC3);
        
        Mat depthFlipped = new Mat();
        //Core.flip(depthMat, depthFlipped, -1);
        Mat depthMat8bit = new Mat();
        depth.convertTo(depthMat8bit, CvType.CV_8UC1, 0.1f);
        //Core.bitwise_not(depthMat8bit,depthMat8bit);
        //Imgproc.equalizeHist(depthMat8bit, depthMat8bit);  
        
        Mat canneyRslt = new Mat();
        Imgproc.Canny(sobelX,sobelY, canneyRslt, cannyThreshold1 , cannyThreshold2, true);

        Mat laplacianRslt = new Mat();
        Imgproc.Laplacian(gray,laplacianRslt, CvType.CV_32F,5,.1,0);
        
        Mat DTF_NC = new Mat();
        Ximgproc.dtFilter(canneyRslt, depthMat8bit, DTF_NC, sigmaSpacial,sigmaColor, Ximgproc.DTF_NC, dtIter);

		
        Texture2D yTexture = (Texture2D)Util.toTexture(sobelX, TextureFormat.R16);
        sobelTextureEvent.Invoke(yTexture);
       
        Texture2D canneyTexture = (Texture2D)Util.toTexture(canneyRslt, TextureFormat.R8);
        canneyTextureEvent.Invoke(canneyTexture);

        Texture2D depthtexture = (Texture2D)Util.toTexture(DTF_NC, TextureFormat.R8);
        improvedDepth.Invoke(depthtexture);
    }
    
    // public void ProcessDepthImage(Mat depth, out Mat filteredDepth){

    //    // Todo, replace this portion with the edge cleaned depth   
    //     Mat depthMat8bit = new Mat();
    //     depth.convertTo(depthMat8bit, CvType.CV_8UC1, 0.1f);
    //     Core.bitwise_not(depthMat8bit,depthMat8bit);
    //     Imgproc.equalizeHist(depthMat8bit, depthMat8bit);        
    //     filteredDepth = new Mat();
    //     Photo.fastNlMeansDenoising(depthMat8bit, filteredDepth, 12 , 10 , 60);
    // }
}
