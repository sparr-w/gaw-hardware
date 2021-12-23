using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleHandler : MonoBehaviour {
    public AccessMethods AccessMethod;
    [Header("Appearance")]
    public GameObject[] FrontBumperOptions;
    public GameObject FrontBumper;
    public GameObject[] RearBumperOptions;
    public GameObject RearBumper;
    public GameObject[] SideSkirtsOptions;
    public GameObject SideSkirts;
    public GameObject[] Accessories;
    [Header("Paint")]
    public Material PaintMaterial;
    public Color[] PaintPresets = new Color[4] {Color.red, Color.blue, Color.black, Color.white};
    public Color PaintColor;

    private VehicleDoor[] doors;

    private void ChangePaint(Color newColor) {
        // instantiate material for the body panels, change the colour and apply it to necessary body panels
        Material newMaterial = Instantiate(PaintMaterial);
        newMaterial.color = newColor;
        // apply new color
        foreach(Transform bodyPanel in transform) {
            if (bodyPanel.gameObject.tag == "Body Panel") { // only paint body panels
                Material[] panelMaterials = bodyPanel.GetComponent<Renderer>().materials;
                for (int i = 0; i < panelMaterials.Length; i++) {
                    // concatenation necessary because it instances the material
                    if (panelMaterials[i].name == PaintMaterial.name + " (Instance)")
                        panelMaterials[i] = newMaterial;
                }
                bodyPanel.GetComponent<Renderer>().materials = panelMaterials;
            }
        }
        //
        PaintColor = newColor;
    }

    public void Awake() { // instance loaded
        // select front bumper and create it
        FrontBumper = FrontBumperOptions[Random.Range(0, FrontBumperOptions.Length)];
        FrontBumper = Instantiate(FrontBumper, transform);
        FrontBumper.tag = "Body Panel";
        // select rear bumper and create it
        RearBumper = RearBumperOptions[Random.Range(0, RearBumperOptions.Length)];
        RearBumper = Instantiate(RearBumper, transform);
        RearBumper.tag = "Body Panel";
        // 50% chance for sideskirt, select and create it
        if (Random.Range(0, 2) == 0) {
            SideSkirts = SideSkirtsOptions[Random.Range(0, SideSkirtsOptions.Length)];
            SideSkirts = Instantiate(SideSkirts, transform);
            SideSkirts.tag = "Body Panel";
        }
        // paint car
        ChangePaint(PaintPresets[Random.Range(0, PaintPresets.Length)]);
        // add random accessories
        foreach (GameObject accessory in Accessories) // 33% chance to add accessory
            if (Random.Range(0, 3) == 0) Instantiate(accessory, transform);
        // collate the doors of the vehicle
        doors = GetComponentsInChildren<VehicleDoor>();
    }

    public void UnlockVehicle() {
        foreach (VehicleDoor door in doors)
            door.Locked = false;
        GetComponentInChildren<Transceiver>().SetLight(GetComponentInChildren<Transceiver>().ActiveLight, true);
    }

    public bool Locked {
        get {
            bool result = false;
            foreach (VehicleDoor door in doors) {
                if (door.Locked)
                    result = true;
            }
            return result;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            GameObject.FindGameObjectWithTag("GameController").GetComponent<GameHandler>().DisplayOverlay(AccessMethod);
        }
    }
}
