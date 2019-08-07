using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.PhotoModule;
using OpenCVForUnity.ImgcodecsModule;

public class ImageFilter : MonoBehaviour
{
    public enum KernalSize 
    {
        one = 1,
        Three = 3,
        Five = 5,
        Seven = 7,
        Nine = 9
    }
    
    public Texture RGBImage{get;set;}
    
    [System.Serializable]
    public class TextureEvent : UnityEvent<Texture> { }
    [System.Serializable]
    public class MatEvent : UnityEvent<Mat> { }
    public TextureEvent filterResultEvent;
    public MatEvent filterMatEvent;
    public float sharpness{get;set;}
    public float contrast{get;set;}
    public float brightness{get;set;}
    public float filterStrength{get;set;}

    public KernalSize SharpenKernalSize = KernalSize.Five;

    [Space]
    public float test;
    public enum LUTNames 
        {
            Amaro,
            FallAway,
            GoldDust,
            Neon,
            NightLark,
            TestingTheWater,
            ThisCity,
            UnTouched
        }

    public LUTNames filterLut;
    void sharpenImage(Mat image, Mat sharpened){
        Mat blurred = new Mat();
        Imgproc.GaussianBlur(image, blurred, new Size(0,0), (int)SharpenKernalSize);
        Core.addWeighted(image, 1 + sharpness, blurred, -1 *sharpness, 0 , sharpened);
    }
    
    float linear(float a, float b, float t)
    {
        return a * (1.0f - t) + b * t;
    }

    void makeLUT(List<float> values, ref Mat LUT){
        LUT = new Mat(1, 256, CvType.CV_8UC1, new Scalar(0));

        // float h = 256.0f /(values.Count-1);
        // for(int j = 0; j < values.Count-1; j++){
        //     float lower = values[j];
        //     float upper = values[j+1];
        //     for(int i = 0; i <= h; i++){
        //         float val = linear(lower, upper, (float)i/h);
        //         LUT.put(0, (j*(int)h)+i, (byte)(val*256.0f));
        //     }            
        // }            

        for(int i = 0; i < 256; i++){
            int range = (int)(((float)i/256.0f) * (float)((values.Count)-1));
            float lower = values[range];
            float upper = values[range+1];
            int index = i%(256/(values.Count-1));
            float frac = (index/256.0f) * (values.Count-1);
            float val = linear(lower, upper, frac);
            LUT.put(0, i, (byte)(val*256.0f));
        }
    }

    void channelAdjust(Mat rgbImage, Mat RLUT, Mat GLUT, Mat BLUT, ref Mat adjusted){
        adjusted = rgbImage.clone();
        List<Mat> channels = new List<Mat>();
        Core.split(adjusted, channels);
        Core.LUT(channels[0], RLUT, channels[0]);
        Core.LUT(channels[1], GLUT, channels[1]);
        Core.LUT(channels[2], BLUT, channels[2]);
        Core.merge(channels, adjusted);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        //Mat rgbMat = Util.toMat((Texture2D)RGBImage, CvType.CV_8UC3);

        //Mat sharpened = new Mat();
        //sharpenImage(rgbMat, sharpened);

        //Mat smaller = new Mat();
        //Imgproc.resize(sharpened, smaller,new Size(0,0), .2, .2);
        
        //Mat rLUT = new Mat();
        //Mat gLUT = new Mat();
        //Mat bLUT = new Mat();
        //makeLUT(new List<float>{0f, 1}, ref rLUT);
        //makeLUT(new List<float>{0f, 0.6f , 0.8f ,1}, ref gLUT);
        //makeLUT(new List<float>{0f, 1f}, ref bLUT);

        // Mat rawLUT = new Mat(1, 256, CvType.CV_8UC3);
        // for(int i = 0; i < 256; i++){
        //     rawLUT.put(0,i, new double[]{i,i,i});
        // }
        // Imgcodecs.imwrite("C:/Users/SIGLab/AppData/LocalLow/DefaultCompany/Photo3D/rawLUT.png", rawLUT);
      
        //filterMatEvent.Invoke(smaller);

        //List<Mat> splitLUT = new List<Mat>();
        //Core.split(LUT, splitLUT);

        //Mat shifted = new Mat();
        //channelAdjust(sharpened,splitLUT[2],splitLUT[1],splitLUT[0], ref shifted); // BGR
        //Texture2D filteredTexture = (Texture2D)Util.toTexture(sharpened, TextureFormat.RGB24);
        //filterResultEvent.Invoke(filteredTexture);
        
    }
}
