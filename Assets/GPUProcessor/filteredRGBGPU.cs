using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class filteredRGBGPU : MonoBehaviour
{
	[System.Serializable]
	public class TextureEvent : UnityEvent<Texture> { }
	public TextureEvent textureBinding;

	[System.Serializable]
	public class LUTEvent : UnityEvent<NamedLUTGPU> { }
	public LUTEvent lutBinding;
/*
	public Frame3DProcessorGPU matSouce;
	public filteredRGB imagePreview;
	public Frame3DProcessorGPU frame3DProcessor;
*/
	public Frame3DGPU frame3dgpu = null;

//	public Mat rgbMat { get; set; }
	public float brightness{get;set;}
	public float contrast{get;set;}
	public float filterStrength { get; set; }
	public NamedLUTGPU lut { get; set; }


	private void updateContrast(float value){
		contrast = value;
		Debug.Log(contrast);
	}

	private void updateBrightness(float value){
		brightness = value;
		Debug.Log(brightness);
	}

	private void updateFilterStrength(float value)
	{
		filterStrength = value;
		Debug.Log(filterStrength);
	}

	private void updateFilteredRGBImage()
	{
		// lut filter here
		Vector3[] corners = new Vector3[4];
		Vector3[] vpcorners = new Vector3[4];
		gameObject.GetComponent<RectTransform>().GetWorldCorners(corners);
		UnityEngine.Rect screenRect = new UnityEngine.Rect(0, 0, Screen.width, Screen.height);
		bool onscreen = screenRect.Contains((Vector2)corners[0]) || screenRect.Contains((Vector2)corners[2]);

		if (onscreen)
		{
			if (lut != null)
			{
				if(lut.filterImage(frame3dgpu, brightness, contrast, filterStrength))
					textureBinding.Invoke(frame3dgpu.recoloredImages[lut.index]);
			}
			else
			{
				textureBinding.Invoke(frame3dgpu.filteredImage);
			}
		}
	}

	void setPreviewLut()
	{
		lutBinding.Invoke(lut);
	}

	private void OnEnable()
	{
		// attach the ImagePreview
		var button = gameObject.GetComponentInChildren<Button>();
		if (button)
		{
			button.onClick.AddListener(setPreviewLut);
		}

		// attach the filter    
		var stengthSlider = GameObject.Find("FilterStrength").GetComponent<Slider>();
		stengthSlider.onValueChanged.AddListener(updateFilterStrength);
		var brightnessSlider = GameObject.Find("Brightness").GetComponent<Slider>();
		brightnessSlider.onValueChanged.AddListener(updateBrightness);
		var contrastSlider = GameObject.Find("Contrast").GetComponent<Slider>();
		contrastSlider.onValueChanged.AddListener(updateContrast);
	}

	// Start is called before the first frame update
	void Start()
	{
		filterStrength = 1.0f;
		if(lut.name == "AllNatural")
			setPreviewLut();
	}

	// Update is called once per frame
	void Update()
	{
		if (frame3dgpu != null)
			updateFilteredRGBImage();
	}

	public void OnFrame3DGPU(Frame3DGPU inFrame)
	{
		frame3dgpu = inFrame;
	}
}
