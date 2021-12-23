using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BruteforceTargetter : MonoBehaviour {
    public RectTransform TopBorder;
    public RectTransform BottomBorder;
    
    public RectTransform RectTransform {
        get {return GetComponent<RectTransform>();}
    }
}
