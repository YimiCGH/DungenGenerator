using UnityEngine;
using System.Collections;

public static class VectorUtil
{
    public static Vector3 RoundVector3ToInt(this Vector3 _src) {
        return new Vector3(Mathf.RoundToInt(_src.x),
            Mathf.RoundToInt(_src.y),
            Mathf.RoundToInt(_src.z));
    }
    
}
