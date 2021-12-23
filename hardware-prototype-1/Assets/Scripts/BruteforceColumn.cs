using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BruteforceColumn : MonoBehaviour {
    public bool Locked = false;
    public BruteforceCell[] cells;
    public int TargetIndex;

    private bool inLockPosition = false;

    public bool InLockPosition {
        get {return inLockPosition;} set {inLockPosition = value;}
    }

    public void GenerateTarget(char value) {
        // collate cells and generate target location
        cells = GetComponentsInChildren<BruteforceCell>();
        TargetIndex = Random.Range(0, cells.Length);
        // change appearance of cell
        cells[TargetIndex].GetComponentInChildren<Text>().text = "" + value;
        cells[TargetIndex].GetComponentInChildren<Text>().color = Color.red;
    }
}
