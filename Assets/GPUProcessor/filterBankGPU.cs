using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// OpenCV dependency
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.UnityUtils;
using System.IO;



[System.Serializable]
public class NamedLUTGPU
{
	public string name;
	public string name2;
	public string hsvName;
	public string hsvName2;
	public int index;
	public Texture2D LUT;
	public Texture2D modifiyedLUT;
	public Texture2D LUT2;
	public Texture2D modifiyedLUT2;

	public Texture2D hsvLUT;
	public Texture2D hsvLUT2;

	public ComputeShader computeShader;

	public bool filterImage(Frame3DGPU f, float brightness, float contrast, float strength)
	{
		if(f.colorWidth == 0 || f.colorHeight == 0) { return false; }
		// if recoloredImages are not initialized, new them here
		if (f.recoloredImages.Count <= index)
		{
			var add_count = index + 1 - f.recoloredImages.Count;
			for (int i = 0; i < add_count; i++)
			{
				f.recoloredImages.Add(new RenderTexture(f.colorWidth, f.colorHeight, 0, RenderTextureFormat.ARGB32));
				f.recoloredImages[f.recoloredImages.Count - 1].filterMode = FilterMode.Point;
				f.recoloredImages[f.recoloredImages.Count - 1].enableRandomWrite = true;
				f.recoloredImages[f.recoloredImages.Count - 1].Create();
			}
		}

		//////////////////////////////////////////////////////////////
		// Modifiy the LUT to include Brightness and Contrast

		Mat LUTMat = Util.toMat(LUT, CvType.CV_8UC3);
		LUTMat = LUTMat + new Scalar(brightness + 0.2, brightness + 0.2, brightness + 0.2);
		LUTMat = (LUTMat * (contrast / 127.0f + 1)) - new Scalar(contrast, contrast, contrast);
		modifiyedLUT = (Texture2D)Util.toTexture(LUTMat, TextureFormat.RGB24);

		Mat LUTMat2 = Util.toMat(LUT2, CvType.CV_8UC3);
		LUTMat2 = LUTMat2 + new Scalar(brightness + 0.2, brightness + 0.2, brightness + 0.2);
		LUTMat2 = (LUTMat2 * (contrast / 127.0f + 1)) - new Scalar(contrast, contrast, contrast);
		modifiyedLUT2 = (Texture2D)Util.toTexture(LUTMat2, TextureFormat.RGB24);

		//////////////////////////////////////////////////////////////

		computeShader.SetInt("image_width", f.colorWidth);
		computeShader.SetInt("image_height", f.colorHeight);

		// set uniform values to shader
		computeShader.SetInt("map_width", LUT.width - 1);
		computeShader.SetFloat("filter_strength", strength);

		computeShader.SetTexture(1, "in_color_map", modifiyedLUT);
		computeShader.SetTexture(1, "in_color_map2", modifiyedLUT2);
		computeShader.SetTexture(1, "in_color_texture", f.filteredImage);
		if (f.snapped)
		{
			// I believe depth threshold is already provided. Hopefully...
			// for color LUT processing
			computeShader.SetFloat("depth_threshold", 1.5f * 26.0f);
			f.refinedDepthTex2D = (Texture2D)Util.toTexture(f.refinedDepth, TextureFormat.R8);
			var tempDepth = new Mat();
			Core.bitwise_not(f.refinedDepth, tempDepth);
			f.refinedDepthTex2D = (Texture2D)Util.toTexture(tempDepth, TextureFormat.R8, true, -1);
			computeShader.SetTexture(1, "in_depth_texture", f.refinedDepthTex2D);
			computeShader.SetTexture(1, "out_color_texture", f.recoloredImages[index]);
			computeShader.Dispatch(1, f.colorWidth / 8, f.colorHeight / 8, 1);

			// for HSV LUT processing
			computeShader.SetTexture(2, "in_hsv_map", hsvLUT);
			computeShader.SetTexture(2, "in_hsv_map2", hsvLUT2);
//			computeShader.SetTexture(2, "in_color_texture", f.filteredImage); // used when RGB LUT not processed
			computeShader.SetTexture(2, "in_depth_texture", f.refinedDepthTex2D);
			computeShader.SetTexture(2, "out_color_texture", f.recoloredImages[index]); // in/out
			computeShader.Dispatch(2, f.colorWidth / 8, f.colorHeight / 8, 1);
		}
		else
		{
			// for color LUT processing
			computeShader.SetFloat("depth_threshold", 1.5f);
			computeShader.SetTexture(1, "in_depth_texture", f.depthImage);
			computeShader.SetTexture(1, "out_color_texture", f.recoloredImages[index]);
			computeShader.Dispatch(1, f.colorWidth / 8, f.colorHeight / 8, 1);

			// for HSV LUT processing
			computeShader.SetTexture(2, "in_hsv_map", hsvLUT);
			computeShader.SetTexture(2, "in_hsv_map2", hsvLUT2);
//			computeShader.SetTexture(2, "in_color_texture", f.filteredImage); // used when RGB LUT not processed
			computeShader.SetTexture(2, "in_depth_texture", f.depthImage);
			computeShader.SetTexture(2, "out_color_texture", f.recoloredImages[index]);	// in/out
			computeShader.Dispatch(2, f.colorWidth / 8, f.colorHeight / 8, 1);
		}


		return true;
	}
}

public class filterBankGPU : MonoBehaviour
{
	public Frame3DProducerGPU frame3DProcessorGPU;
	public List<NamedLUTGPU> Luts;
	public NamedLUTGPU mainLut;
	public GameObject filterButtonPrefab;
	public ComputeShader lutProcessor;

	// This implementation needs to be replaced to much sophisticated way :)
	void LoadLUTs(string path, string path2, string path3, string path4)
	{
		string[] LUTPaths = Directory.GetFiles(path);
		string[] LUT2Paths = Directory.GetFiles(path2);
		string[] hsvLUTPaths = Directory.GetFiles(path3);
		string[] hsvLUT2Paths = Directory.GetFiles(path4);

		// if number of LUTs is not matched, load aborted
		if (LUTPaths.Length != LUT2Paths.Length
		|| LUTPaths.Length != hsvLUTPaths.Length
		|| LUTPaths.Length != hsvLUT2Paths.Length) { return; }

		int index_counter = 0;
		foreach (string lutPath in LUTPaths)
		{
			var lutPath2 = LUT2Paths[index_counter];
			var hsvLutPath = hsvLUTPaths[index_counter];
			var hsvLutPath2 = hsvLUT2Paths[index_counter];
			var newLUTGPU = new NamedLUTGPU()
			{
				name = Path.GetFileNameWithoutExtension(lutPath),
				name2 = Path.GetFileNameWithoutExtension(lutPath2),
				hsvName = Path.GetFileNameWithoutExtension(hsvLutPath),
				hsvName2 = Path.GetFileNameWithoutExtension(hsvLutPath2),
				// OpenCV dependency
				LUT = (Texture2D)Util.toTexture(Imgcodecs.imread(lutPath), TextureFormat.RGB24),
				LUT2 = (Texture2D)Util.toTexture(Imgcodecs.imread(lutPath2), TextureFormat.RGB24),
				hsvLUT = (Texture2D)Util.toTexture(Imgcodecs.imread(hsvLutPath), TextureFormat.RGB24),
				hsvLUT2 = (Texture2D)Util.toTexture(Imgcodecs.imread(hsvLutPath2), TextureFormat.RGB24),
				computeShader = lutProcessor,
				index = index_counter++
			};

			newLUTGPU.LUT.wrapMode = TextureWrapMode.Clamp;
			newLUTGPU.LUT.Apply();
			newLUTGPU.LUT2.wrapMode = TextureWrapMode.Clamp;
			newLUTGPU.LUT2.Apply();
			newLUTGPU.hsvLUT.wrapMode = TextureWrapMode.Clamp;
			newLUTGPU.hsvLUT.Apply();
			newLUTGPU.hsvLUT2.wrapMode = TextureWrapMode.Clamp;
			newLUTGPU.hsvLUT2.Apply();
			Luts.Add(newLUTGPU);
		}
	}

	// Start is called before the first frame update
	void Start()
	{
		string streamPath = Application.dataPath;
		Debug.Log("Data Path: " + streamPath);
		LoadLUTs(streamPath + "/LUTs/", streamPath + "/LUTs/BackLUTs/", streamPath + "/LUTs/HSVLUTs/", streamPath + "/LUTs/BackHSVLUTs/");
		//LoadLUTs(Application.persistentDataPath + "/LUTs/", Application.persistentDataPath + "/LUTs/BackLUTs/", Application.persistentDataPath + "/LUTs/HSVLUTs/", Application.persistentDataPath + "/LUTs/BackHSVLUTs/");
		foreach (NamedLUTGPU lut in Luts)
		{
			var filter = Instantiate(filterButtonPrefab, gameObject.transform);
			filter.transform.localScale = new Vector3(1, 1, 1);
			filter.GetComponentInChildren<Text>().text = lut.name;
			filter.GetComponentInChildren<filteredRGBGPU>().lut = lut;
			filter.SetActive(true);
			// distribute frame3dgpu to each filteredRGBGPU
			frame3DProcessorGPU.frame3DgpuBinding.AddListener(filter.GetComponentInChildren<filteredRGBGPU>().OnFrame3DGPU);
		}
	}

}
