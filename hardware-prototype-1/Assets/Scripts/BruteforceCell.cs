using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BruteforceCell : MonoBehaviour {
    private char value;

    public void GenerateCell() {
        string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        value = characters[Random.Range(0, characters.Length)];
        // display values
        GetComponentInChildren<Text>().text = "" + value;
    }

    #region Setters and Getters
    public RectTransform RectTransform {
        get {return GetComponent<RectTransform>();}
    }
    #endregion
}
