using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenWebsite : MonoBehaviour
{
    private string websiteAddress;
    public string WebsiteAddress{set{
        websiteAddress = value;
        Application.OpenURL(websiteAddress);
    } get{return websiteAddress;}}
}
