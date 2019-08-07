using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using OpenCVForUnity.CoreModule;
using UnityEngine.UI;

public class Frame3DProducerGPU : MonoBehaviour
{
	[System.Serializable]
	public class Frame3DGPUEvent : UnityEvent<Frame3DGPU> { }
	public Frame3DGPUEvent frame3DgpuBinding;

	public Texture depth { get; set; }
	public Texture color { get; set; }

	//public float snapDelay{get; set;}
	//public Slider delaySlider;
	//int remainingTillSnap = 0;
	//bool triggered = false;

//	public bool snapped { get; set; }
//	int frameID = 0;

	private Frame3DGPU frame3Dgpu = null;

	// Start is called before the first frame update
	void Start()
	{
//		snapped = false;
	}

	// Update is called once per frame
	void Update()
	{
		if (depth != null && color != null && frame3Dgpu == null)
		{
			// frame3Dgpu exists only one instance and passed to others as reference
			// to prevent GC overhead
			frame3Dgpu = new Frame3DGPU();
			frame3Dgpu.colorHeight = color.height;
			frame3Dgpu.colorWidth = color.width;
			frame3Dgpu.depthHeight = depth.height;
			frame3Dgpu.depthWidth = depth.width;
			frame3Dgpu.depthImage = (Texture2D)depth;
			frame3Dgpu.rgbImage = (Texture2D)color;
			frame3Dgpu.filteredImage = new RenderTexture(frame3Dgpu.colorWidth, frame3Dgpu.colorHeight, 0, RenderTextureFormat.ARGB32);
			frame3Dgpu.filteredImage.filterMode = FilterMode.Point;
			frame3Dgpu.filteredImage.enableRandomWrite = true;
			frame3Dgpu.filteredImage.Create();

			frame3Dgpu.recoloredImages = new List<RenderTexture>();
			frame3Dgpu.filteredImageTex2D = new Texture2D(frame3Dgpu.colorWidth, frame3Dgpu.colorHeight, TextureFormat.RGB24, false);
			frame3Dgpu.refinedDepthTex2D = new Texture2D(frame3Dgpu.depthWidth, frame3Dgpu.depthHeight, TextureFormat.R8, false); ;

			frame3Dgpu.snapped = false;

			frame3Dgpu.refinedDepth = new Mat();
			frame3Dgpu.waterMarkedColor = new Mat();
			frame3Dgpu.waterMarkedDepth = new Mat();

			frame3DgpuBinding.Invoke(frame3Dgpu);
		}
		Resources.UnloadUnusedAssets();
	}

	/* Not working due to exicution order
	IEnumerator Delay(){
		
		if(remainingTillSnap <= 0)
			frame3Dgpu.snapped = true;
		else{
			yield return new WaitForSeconds(1);
			Debug.Log(remainingTillSnap);
			remainingTillSnap -= 1;
			delaySlider.value = remainingTillSnap;
			StartCoroutine(Delay());
		}
		
	}*/

	public void OnSnap()
	{
		//remainingTillSnap = (int)snapDelay;
		//StartCoroutine(Delay());
		if (frame3Dgpu != null){
			frame3Dgpu.snapped = true;
		}
	}
}
