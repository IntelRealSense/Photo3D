using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;

public class Util
{ 
    public static Texture toTexture(Mat mat, TextureFormat format, bool flip=false, int flipCode=0){
        Texture2D tex = new Texture2D(mat.cols(), mat.rows(), format, false);
        Utils.fastMatToTexture2D(mat, tex, flip, flipCode);
        return tex;
    }

    public static Mat toMat(Texture tex, int type, bool flip=false, int flipCode=0){
        Mat mat = new Mat(tex.height, tex.width, type);
        Utils.fastTexture2DToMat((Texture2D)tex, mat, flip, flipCode);
        return mat;
    }
}
