using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.PhotoModule;
using OpenCVForUnity.XimgprocModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.UnityUtils;

public class Frame3DProcessorGPU : MonoBehaviour
{
	public Frame3DGPU frame3Dgpu { get; set; }
	public ComputeShader computeShader;

	[System.Serializable]
	public class TextureEvent : UnityEvent<Texture> { }

	[System.Serializable]
	public class Frame3DGPUEvent : UnityEvent<Frame3DGPU> { }

	public Texture2D watermark;
	public Texture2D watermarkDepth;

	public Texture2D depthRescale;

	public TextureEvent textureBinding;
	public Frame3DGPUEvent frame3DgpuBinding;

	[Header("NlMeansDenoising")]
	public float h = 4;
	public float hColor = 4;
	public int templateWindowSize = 7;
	public int searchWindowSize = 21;

	[Header("Preview")]
	public Vector2 size = new Vector2(320, 240);

//	[Header("LUT recolor")]
	public NamedLUTGPU lut { get; set; }

	[Header("Depth Processing")]
	[Range(0, 255)]
	public int cannyThreshold1 = 50;

	[Range(0, 255)]
	public int cannyThreshold2 = 100;

	public float depthThreshold {get;set;}
	public float blurR {get;set;}
	public enum KernalSize
	{
		one = 1,
		Three = 3,
		Five = 5,
		Seven = 7,
		Nine = 9
	}
	public KernalSize ksize;

	[Range(0, 255)]
	public int sigmaSpacial = 40;
	[Range(0, 255)]
	public int sigmaColor = 40;

	[Range(1, 5)]
	public int dtIter = 3;
	public float sobelScale = 0.01f;

	public GameObject overlay;

	public float exposure { get; set; }
	public float brightness { get; set; }
	public float contrast { get; set; }
	public float sharpness { get; set; }
	public float filterStrength { get; set; }

	void Start()
	{
		filterStrength = 0.75f;
		sharpness = 0.5f;
		depthThreshold = 1.5f;
		blurR = 10;
	}

	void postprocessRGBImage(Frame3DGPU f)
	{
		if (f.snapped)
		{
			var rgbMat = Util.toMat(f.rgbImage, CvType.CV_8UC3);
			var rgbProcessedMat = new Mat();

			//Photo.denoise_TVL1(mats, f.postprocessedRGBImage, 1, 2);
			Photo.fastNlMeansDenoisingColored(rgbMat, rgbProcessedMat, h, hColor, templateWindowSize, searchWindowSize);

			if (f.postprocessedRGBImage)
			{
				Texture.DestroyImmediate(f.postprocessedRGBImage);
				f.postprocessedRGBImage = null;
			}

			f.postprocessedRGBImage = (Texture2D)Util.toTexture(rgbProcessedMat, TextureFormat.RGB24);
		}
	}

	void sharpenImage(Frame3DGPU f)
	{
		computeShader.SetInt("image_width", f.colorWidth);
		computeShader.SetInt("image_height", f.colorHeight);

		// set uniform values to shader
		computeShader.SetInt("blur_r", (int)blurR);

		if(f.snapped)
		{
			//if(depthThreshold > 3){ // differing bias as there is a non linear factor applied to the depth 
			//	depthThreshold += 3f;
			//	depthThreshold *= 6.0f;
			//	}// it's hard coded bias. should be checked.
			//else
			float tempDepthThreshold = depthThreshold;
			tempDepthThreshold += 9.4f; // hand tuned linar offset :/
			tempDepthThreshold *= 3.6f;
			computeShader.SetFloat("depth_threshold", tempDepthThreshold); 
			computeShader.SetTexture(0, "in_color_texture", f.postprocessedRGBImage);
			var tempDepth = new Mat();
			Core.bitwise_not(f.refinedDepth, tempDepth);
			f.refinedDepthTex2D = (Texture2D)Util.toTexture(tempDepth, TextureFormat.R8, true, -1);
			computeShader.SetTexture(0, "in_depth_texture", f.refinedDepthTex2D);
		}
		else
		{
			computeShader.SetFloat("depth_threshold", depthThreshold);
			computeShader.SetTexture(0, "in_color_texture", f.rgbImage);
			computeShader.SetTexture(0, "in_depth_texture", f.depthImage);
		}
		computeShader.SetTexture(0, "out_color_texture", f.filteredImage);
		computeShader.Dispatch(0, f.colorWidth / 8, f.colorHeight / 8, 1);
	}

	void filterImage(Frame3DGPU f)
	{
		// prefer postprocessed image

		// color, brightness, contrast and saturation processing are put here
//		image = image * Mathf.Pow(2, exposure);
//		image = image + new Scalar(brightness + 0.2, brightness + 0.2, brightness + 0.2);
//		image = (image * (contrast / 127.0f + 1)) - new Scalar(contrast, contrast, contrast);
		// blur processing
		sharpenImage(f);
		// resize is removed now. if speed is not fast enough, it will be added.
	}

	// apply the watermark to the color and depth image
	void WaterMarkFrame3D(Frame3DGPU f, Texture2D waterMarkColor, Texture2D waterMarkDepth)
	{

		// converting from RenderTexture to OpenCV Mat via Texture2D
		if (lut != null)
			RenderTexture.active = frame3Dgpu.recoloredImages[lut.index];
		else
			RenderTexture.active = frame3Dgpu.filteredImage;

		f.filteredImageTex2D.ReadPixels(new UnityEngine.Rect(0, 0, frame3Dgpu.filteredImage.width, frame3Dgpu.filteredImage.height), 0, 0);
		f.filteredImageTex2D.Apply();

		Mat filteredImage = Util.toMat(f.filteredImageTex2D, CvType.CV_8UC3);

		Imgproc.resize(filteredImage, filteredImage, new Size(0,0), 2, 2);
		Mat roi = filteredImage.submat(
			filteredImage.rows() - waterMarkColor.height - 70,
			filteredImage.rows() - 70,
			(filteredImage.cols()/2) - (waterMarkColor.width/2),
			(filteredImage.cols()/2) - (waterMarkColor.width/2) +  waterMarkColor.width);
		
		Mat img2gray = new Mat();
		Mat waterMarkMat = Util.toMat(waterMarkColor, CvType.CV_8UC3, true, -1);
		Imgproc.cvtColor(waterMarkMat, img2gray, Imgproc.COLOR_BGR2GRAY);
        Mat mask = new Mat();
		Mat maskInv = new Mat();
		Imgproc.threshold(img2gray, mask, 10, 255, Imgproc.THRESH_BINARY);
		Core.bitwise_not(mask, maskInv);

		Mat bg = new Mat();
		Core.bitwise_and(roi, roi, bg, maskInv);
		Mat fg = new Mat();
		Core.bitwise_and(waterMarkMat, waterMarkMat, fg, mask);

		Mat rslt = new Mat();
		Core.add(bg, fg, rslt);
		rslt.copyTo(roi);
		f.waterMarkedColor = filteredImage;

		Debug.Log("Applying Watermark to depth image");
		// here something good watermarking to depth

		Mat refinedDepth = new Mat();
		Core.flip(f.refinedDepth, refinedDepth, -1);

		Imgproc.resize(refinedDepth, refinedDepth, new Size(0,0), 2, 2);
		roi = refinedDepth.submat(
			refinedDepth.rows() - waterMarkColor.height - 70,
			refinedDepth.rows() - 70,
			(refinedDepth.cols()/2) - (waterMarkColor.width/2),
			(refinedDepth.cols()/2) - (waterMarkColor.width/2) +  waterMarkColor.width);
		

		Mat filledMask = mask.clone();

		Imgproc.floodFill(filledMask,new Mat(), new Point(0,0), new Scalar(255));
		Mat filledMaskInv = new Mat();
		Core.bitwise_not(filledMask, filledMaskInv);
		Core.bitwise_or(mask, filledMaskInv, mask);

		//Imgcodecs.imwrite("C:/Users/SIGLab/AppData/LocalLow/Intel/Photo3D/3dImages/" + "filledMask.png", mask);

		bg = new Mat();
		//Imgproc.dilate(mask, mask, new Mat(3,3, CvType.CV_8UC1), new Point(0,0));
		//Imgproc.blur(mask, mask, new Size(2,2), new Point(1,1));
		Core.bitwise_not(mask, maskInv);
		//Core.bitwise_and(roi, roi, bg, maskInv);
		rslt = new Mat();
		Core.add(roi, mask, rslt);
		rslt.copyTo(roi);
		refinedDepth.copyTo(f.waterMarkedDepth);
	}

	void domainTransferDepthImage(Frame3DGPU f)
	{
		//Utils.setDebugMode(true);
		Debug.Log("Applying EdgeCleanup to Depth");
		// convert from texture to mat
		Mat rgbMat = new Mat();
		Core.flip(Util.toMat(f.postprocessedRGBImage, CvType.CV_8UC3), rgbMat, -1);
		Mat depthMat = Util.toMat(f.depthImage, CvType.CV_16UC1);

		Mat gray = new Mat();
		Imgproc.cvtColor(rgbMat, gray, Imgproc.COLOR_RGBA2GRAY);
		Mat sobelX = new Mat();
		Mat sobelY = new Mat();
		Imgproc.Sobel(gray, sobelX, CvType.CV_16S, 1, 0, (int)ksize, sobelScale, 0, Core.BORDER_DEFAULT);
		Imgproc.Sobel(gray, sobelY, CvType.CV_16S, 0, 1, (int)ksize, sobelScale, 0, Core.BORDER_DEFAULT);

		Mat depthMat8bit = new Mat();
		depthMat.convertTo(depthMat8bit, CvType.CV_8UC1, 0.03f);
		Core.bitwise_not(depthMat8bit, depthMat8bit);
		//Imgproc.equalizeHist(depthMat8bit, depthMat8bit);  

		Mat depthFlipped = new Mat();
		Core.flip(depthMat8bit, depthFlipped, -1);

		Mat canneyRslt = new Mat();
		Imgproc.Canny(sobelX, sobelY, canneyRslt, cannyThreshold1, cannyThreshold2, true);

		//Imgcodecs.imwrite("C:/Users/SIGLab/AppData/LocalLow/Intel/Photo3D/3dImages/" + "depth.png", canneyRslt);
		
		//415 incomplete depth
		Mat cropped = depthFlipped.submat(0, 690, 0, 1190); 
		Core.copyMakeBorder(cropped, depthFlipped, 0, 720 - 690, 0, 1280 - 1190, Core.BORDER_REPLICATE | Core.BORDER_ISOLATED);
		

		Mat laplacianRslt = new Mat();
		Imgproc.Laplacian(gray, laplacianRslt, CvType.CV_32F, 5, .1, 0);

		Ximgproc.dtFilter(canneyRslt, depthFlipped, f.refinedDepth, sigmaSpacial, sigmaColor, Ximgproc.DTF_NC, dtIter);
	
		// Not working with built solutions, cant figure out why
		List<Mat> matList = new List<Mat>();
		Mat depthLUT = Util.toMat(depthRescale, CvType.CV_8UC3);
		Core.split(depthLUT, matList);
		Mat temp = new Mat();
		f.refinedDepth.convertTo(temp, CvType.CV_8UC1);
		Core.LUT(temp,matList[0], f.refinedDepth);
		//Utils.setDebugMode(false);
	}

	void Update()
	{
		if (frame3Dgpu == null) { return; }

		if (frame3Dgpu.snapped)
		{
			postprocessRGBImage(frame3Dgpu);
			domainTransferDepthImage(frame3Dgpu);
			filterImage(frame3Dgpu);
		}
		else
		{
			filterImage(frame3Dgpu);
			overlay.SetActive(false);
		}
	}

	public void OnPostRenderUpdate()
	{
		if(frame3Dgpu == null) { return; }

		//		fullSizeBinding.Invoke(frame3D.filteredImage);
		//		previewBinding.Invoke(frame3D.smallFilteredImage);

		// just refering one of filtered effect. Not processed here
		//		if(lut != null)
		//			if (lut.LUT != null)
		//				lut.filterImage(frame3Dgpu, filterStrength);

		// change invoke texture if lut is set or not.

		if (frame3Dgpu.snapped)
		{
			// This function uses readback data from GPU memory. So need to be called after Rendered
			WaterMarkFrame3D(frame3Dgpu, watermark, watermarkDepth);
		}

		if (lut != null)
			textureBinding.Invoke(frame3Dgpu.recoloredImages[lut.index]);
		else
			textureBinding.Invoke(frame3Dgpu.filteredImage);

		// only when snapped, it will invoke to save file
		if (frame3Dgpu.snapped)
			frame3DgpuBinding.Invoke(frame3Dgpu);
	}
}
