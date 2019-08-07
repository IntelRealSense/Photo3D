using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class postRenderEventHandler : MonoBehaviour
{
	public UnityEvent postRenderBinding;

    // Start is called before the first frame update

    // Update is called once per frame
    void OnPostRender()
    {
		postRenderBinding.Invoke();        
    }
}
