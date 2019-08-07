using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;

[System.Serializable]
public class Frame3DGPU
{
	public Texture2D rgbImage;
	public Texture2D depthImage;
	public Texture2D postprocessedRGBImage;// mlMean filtered image
	public RenderTexture filteredImage; // applyed brightness, contrast, exposure ect. 
	public Texture2D filteredImageTex2D; // for GPU processing buffer
	//	public Texture2D smallFilteredImage; // resized for the filter previews
	public List<RenderTexture> recoloredImages; // LUT based recoloured 
	public Texture2D refinedDepthTex2D; // for GPU processing buffer

	// below images are only used for offline snap
	public Mat refinedDepth; // edge refined using the mlMean filtered image
	public Mat waterMarkedColor;
	public Mat waterMarkedDepth;

	public bool snapped = false;

	public int colorWidth;
	public int colorHeight;
	public int depthWidth;
	public int depthHeight;
}

