using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgprocModule;

public class ColorImageFilter : MonoBehaviour
{
    [System.Serializable]
    public class MatEvent : UnityEvent<Mat> { }
    public MatEvent fullSizeMatBinding;
    public MatEvent resizedMatBinding;

    public Texture texture{get;set;}
    public Mat textureAsMat{get;set;}
    public Vector2 size = new Vector2(320,240);

    public float exposure{get;set;}
    public float brightness{get; set;}
    public float contrast{get; set;}
    public float sharpness{get; set;}
    // Start is called before the first frame update
    void Start()
    {
        brightness = 0;
        //contrast = 0;
    }

    void sharpenImage(Mat image, Mat sharpened){
        Mat blurred = new Mat();
        //Imgproc.GaussianBlur(image, blurred, new Size(0,0), 5);
        Imgproc.blur(image, blurred, new Size(3,3));
        Core.addWeighted(image, 1 + sharpness, blurred, -1 *sharpness, 0 , sharpened);
    }

    // Update is called once per frame
    void Update()
    {
        Mat mat;
        if(texture){
            mat = Util.toMat((Texture2D)texture, CvType.CV_8UC3);
        }
        else{
            mat = textureAsMat;
        }
        
        mat = mat * Mathf.Pow(2, exposure);
        mat = mat + new Scalar(brightness+0.2, brightness+0.2, brightness+0.2);
        mat = (mat * (contrast/127.0f + 1)) - new Scalar(contrast, contrast, contrast);
        sharpenImage(mat, mat);
        fullSizeMatBinding.Invoke(mat);

        Mat smaller = new Mat();
        Imgproc.resize(mat, smaller, new Size(size.x, size.y));
        resizedMatBinding.Invoke(smaller);
    }
}
