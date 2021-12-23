using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlot : MonoBehaviour {
    public GameObject ItemPrefab;

    private int number;
    private new string name;
    private Image background;

    private void Awake() { // instance loaded
        // get slot number
        for (int i = 0; i < transform.parent.childCount; i++) {
            if (transform.parent.GetChild(i) == transform)
                number = i + 1;
        }
        // get slot name
        name = transform.Find("Label").GetComponent<Text>().text;
        // get background
        background = transform.Find("Background").GetComponent<Image>();
    }

    #region Setters and Getters
    public string Name {
        get {return name;}
    }

    public int Index {
        get {return number;}
    }
    #endregion

    private void SetAppearance(bool equipped) {
        if (equipped)
            background.color = new Color(1f, 1f, 1f, background.color.a);
        else
            background.color = new Color(0f, 0f, 0f, background.color.a);
    }

    public void Equip() {
        SetAppearance(true);
        // if there is an item that the player can hold, add it to the player's object
        if (ItemPrefab != null)
            Instantiate(ItemPrefab, GameObject.FindGameObjectWithTag("Player").transform);
    }

    public void Dequip() {
        SetAppearance(false);
        // if there is an item the player can hold, remove all instances from the player's object
        if (ItemPrefab != null)
            Destroy(GameObject.FindGameObjectWithTag("Player").transform.Find(ItemPrefab.name + "(Clone)").gameObject);
    }
}
