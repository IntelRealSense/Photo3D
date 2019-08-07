using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.PhotoModule;
using System.Runtime.InteropServices;
using OpenCVForUnity.XimgprocModule;
using System.IO;

public class SavePNG : MonoBehaviour
{
    [System.Serializable]
    public class TextureEvent : UnityEvent<Texture> { }
    //public TextureEvent sobelTextureEvent;
    //public TextureEvent canneyTextureEvent;
    //public TextureEvent improvedDepth;
    public Texture rawRgbTexture{get;set;}
    public Mat depth{get;set;}
    public Mat color{get;set;}

    [System.Serializable]
    public class MatEvent : UnityEvent<Mat> { }

    public Texture2D watermark;
    public Texture2D watermarkDepth;
    private Mat watermarkMat;    
    private Mat watermarkDepthMat;

    //prep watermark
    void Start() {
        // convert watermark texture to Mat
        Debug.Log("converting watermake texture to mat");
        watermarkMat = Util.toMat(watermark, CvType.CV_8UC4);
        //Imgproc.cvtColor(watermarkMat, watermarkMat, Imgproc.COLOR_BGR2RGBA);
        if(watermarkDepth){
            Debug.Log("Converting watermake texture to mat");
            watermarkDepthMat = Util.toMat(watermarkDepth, CvType.CV_8UC4);
            //Imgproc.cvtColor(watermarkMat, watermarkMat, Imgproc.COLOR_BGR2RGBA);
        }else{
            Debug.Log("Using the rgb to compute a depth watermark");
            Mat waterGray = new Mat();
            Imgproc.cvtColor(watermarkMat, waterGray, Imgproc.COLOR_RGBA2GRAY);
            Imgproc.threshold(waterGray, watermarkDepthMat, 1, 255, Imgproc.THRESH_BINARY);
        }
        /////////////
    }

    

    // Update is called once per frame
    void Update()
    {
        //Mat colorMat = new Mat(color.height, color.width, CvType.CV_8UC3);
        //Utils.fastTexture2DToMat((Texture2D)color, colorMat, false);
        //Mat colorFlipped = new Mat();
        //Core.flip(colorMat, colorFlipped, -1);
        //Imgproc.cvtColor(colorMat, colorFlipped, Imgproc.COLOR_BGR2RGBA);
        
        //Imgcodecs.imwrite("C:/Users/SIGLab/AppData/LocalLow/DefaultCompany/Photo3D/colorFlipped.png", colorFlipped);
        saveButtonClicked();
    }

    // function that is triggered on the click of the snap button
    public void saveButtonClicked(){
        // aquire the latest frame from the camera

        // establish the save directory and file path
        string outpath = Application.persistentDataPath + "/3dImages/";
        Directory.CreateDirectory(outpath);
        string filePath = outpath + "/image";

        // post process the depth image
        
        Mat filteredRGB;
        Debug.Log("Improving the quality of the RGB Image");
        ProcessRBGImage(color, out filteredRGB);

        // Watermark the depth and color image
        Debug.Log("Applying watermarks");
        Mat watermarkedDepth;
        Mat watermarkedColor;
        // write the resulting images to disk

        Debug.Log("writing color to " + filePath + ".png");
        Imgcodecs.imwrite(filePath + ".png", color);
        Debug.Log("writing depth to " + filePath + "_depth.png");
        Imgcodecs.imwrite(filePath + "_depth.png", depth); 
        

        // Load the directory where the images are saved 
        Debug.Log("explorer.exe"+" /n, /e, "+outpath.Replace('/', '\\'));
        System.Diagnostics.Process.Start("explorer.exe","/n, /e, "+outpath.Replace('/', '\\'));
        gameObject.SetActive(false);
    }


    public void ProcessRBGImage(Mat color, out Mat filteredRGB){    
        filteredRGB = new Mat();
        Photo.fastNlMeansDenoisingColored(color, filteredRGB, 4, 4 , 5 , 30);
    }

    public void SavePNGsToDisk(Mat depthMat, Mat rgbMat, string path){
        // Mat watermarkMat = new Mat(watermark.height, watermark.width, CvType.CV_8UC4);
        // Utils.fastTexture2DToMat(watermark, watermarkMat);
        // Imgproc.cvtColor(watermarkMat, watermarkMat, Imgproc.COLOR_BGR2RGBA);
        // Mat waterGray = new Mat();
        // Imgproc.cvtColor(watermarkMat, waterGray, Imgproc.COLOR_RGBA2GRAY);
        // Mat waterDepth = new Mat();
        // Imgproc.threshold(waterGray, waterDepth, 1, 255, Imgproc.THRESH_BINARY);
 

        Mat depthFlipped = new Mat();
        Core.flip(depthMat, depthFlipped, -1);
        Mat depthMat8bit = new Mat();
        depthFlipped.convertTo(depthMat8bit, CvType.CV_8UC1, 0.1f);
        Core.bitwise_not(depthMat8bit,depthMat8bit);
        Imgproc.equalizeHist(depthMat8bit, depthMat8bit);        
        Mat filtereddepth = new Mat();
        Photo.fastNlMeansDenoising(depthMat8bit, filtereddepth, 12 , 10 , 60);

        //Imgcodecs.imwrite(filePath + "_depth.png", rslt); 
        //Debug.Log("wrote depth to " + filePath + "_depth.png");
        



        //// color
        //Mat colorMat = new Mat(colorMat.height, color.width, CvType.CV_8UC3);
        //Utils.fastTexture2DToMat((Texture2D)color, colorMat);
        Mat colorFlipped = new Mat();
        Core.flip(color, colorFlipped, -1);
        Imgproc.cvtColor(colorFlipped, colorFlipped, Imgproc.COLOR_BGR2RGBA);


        Mat filtered = new Mat();
        Photo.fastNlMeansDenoisingColored(colorFlipped, filtered, 4, 4 , 5 , 30);
        //Mat filtered2 = new Mat();
        //Imgproc.bilateralFilter(filtered, filtered2, 3, 120, 120);
        Mat blurred = new Mat();
        Imgproc.GaussianBlur(filtered, blurred, new Size(0,0), 5);
        Mat sharpened = new Mat();
        Core.addWeighted(filtered, 1.5, blurred, -0.5, 0 , sharpened);
              
        Mat clahed = new Mat();
        var clahe = Imgproc.createCLAHE();
    
        clahe.setClipLimit(20);
        clahe.setTilesGridSize(new Size(8,8));
        clahe.apply(sharpened, sharpened);

    }
}
