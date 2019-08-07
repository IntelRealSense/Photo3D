using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.PhotoModule;

public class mlMeansFilter : MonoBehaviour
{
    public Mat image{get; set;}
    [System.Serializable]
    public class MatEvent : UnityEvent<Mat> { }
    public MatEvent filteredImage;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Mat filtered = new Mat();
        Photo.fastNlMeansDenoisingColored(image, filtered, 4, 4 , 5 , 20);
        filteredImage.Invoke(filtered);
    }
}
