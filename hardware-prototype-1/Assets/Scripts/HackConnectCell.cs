using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HackConnectCell : MonoBehaviour {
    private int value;

    public void GenerateCell() {
        value = Random.Range(0, 100);
        // display in appropriate format
        if (value < 10)
            GetComponentInChildren<Text>().text = "0" + value;
        else
            GetComponentInChildren<Text>().text = "" + value;
    }
}
