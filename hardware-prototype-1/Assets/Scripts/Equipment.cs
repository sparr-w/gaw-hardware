using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour {
    public EquipmentSlot[] Inventory;
    public EquipmentSlot Equipped = null;

    private void Awake() { // instance loaded
        Inventory = GetComponentsInChildren<EquipmentSlot>();
    }

    public void Dequip() {
        Equipped.Dequip();
        Equipped = null;
    }

    public void Equip(int newSlot) {
        if (Equipped != null && Equipped.Index == newSlot)
            Dequip();
        else {
            // find the correct item and set it as equipped
            foreach (EquipmentSlot item in Inventory) {
                if (item.Index == newSlot) {
                    if (Equipped != null)
                        Equipped.Dequip();
                    //
                    Equipped = item;
                    Equipped.Equip();
                    break;
                }
            }
        }
    }
}
