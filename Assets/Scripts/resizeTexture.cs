using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;

public class preProcessTexture : MonoBehaviour
{
    [System.Serializable]
    public class MatEvent : UnityEvent<Mat> { }
    public MatEvent fullSizeMatBinding;
    public MatEvent resizedMatBinding;

    public Texture texture{get;set;}
    public Vector2 size = new Vector2(320,240);
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Mat mat = Util.toMat((Texture2D)texture, CvType.CV_8UC3, true);
        fullSizeMatBinding.Invoke(mat);
        Mat smaller = new Mat();
        Imgproc.resize(mat, smaller, new Size(size.x, size.y));
        resizedMatBinding.Invoke(smaller);

        Resources.UnloadUnusedAssets();
        System.GC.Collect();   
    }
}
