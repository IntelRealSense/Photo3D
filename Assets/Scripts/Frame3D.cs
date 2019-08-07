using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;

[System.Serializable]
public class Frame3D
{
    public Mat rgbImage;
    public Mat depthImage;    
    public Mat postprocessedRGBImage;// mlMean filtered image
    public Mat filteredImage; // applyed brightness, contrast, exposure ect. 
    public Mat smallFilteredImage; // resized for the filter previews
    public Mat recoloredImage; // LUT based recoloured 
    public Mat refinedDepth; // edge refined using the mlMean filtered image
    public Mat waterMarkedColor;
    public Mat waterMarkedDepth;   

    public bool snapped = false; 

    public int colorWidth;
    public int colorHeight;
    public int depthWidth;
    public int depthHeight;
}

