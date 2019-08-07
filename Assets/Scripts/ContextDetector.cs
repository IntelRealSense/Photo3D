using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Intel.RealSense;
using UnityEngine.SceneManagement;
using System.Threading;

public class ContextDetector : MonoBehaviour
{   
    bool threadRunning;
    Thread thread;
    int d435Count = 0;
    int d415Count = 0;
    bool workdone = false;
    public Animation anim;
    // Start is called before the first frame update
    void Awake()
    {
        thread = new Thread(ThreadedLoad);
        thread.Start();
    }

    void ThreadedLoad(){
        Debug.Log("theed1");
        threadRunning = true;
        
        Context context = new Context();
        foreach(var device in context.Devices){
            string serial = device.Info[CameraInfo.SerialNumber];
            Debug.Log( device.Info[CameraInfo.Name]);
            if(device.Info[CameraInfo.Name].Contains("D435"))
                d435Count++;
            
            if(device.Info[CameraInfo.Name].Contains("D415"))
                d415Count++;
        }
        
        //LogText.GetComponentInChildren<Text>().text = "Please ensure only a single RealSense depth camera is connected";  

        threadRunning = false;
        workdone = true;
    }

    private void Update() {
        if(workdone && !anim.isPlaying)
            if(d435Count + d415Count < 1)
                SceneManager.LoadScene("NoCameraPlugged");
            else
                SceneManager.LoadScene("Photo3DGPU");
    }
    void OnDisable()
    {
        // If the thread is still running, we should shut it down,
        // otherwise it can prevent the game from exiting correctly.
        if(threadRunning)
        {
            // This forces the while loop in the ThreadedWork function to abort.
            threadRunning = false;

            // This waits until the thread exits,
            // ensuring any cleanup we do after this is safe. 
            thread.Join();
        }
    }
}
