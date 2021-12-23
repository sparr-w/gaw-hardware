using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transceiver : MonoBehaviour {
    public bool Placed;
    public VehicleHandler Target;
    [Header("Signal Light")]
    public GameObject SignalLight;
    public Material SignalLightDim;
    public Material SignalLightBright;
    [Header("Active Light")]
    public GameObject ActiveLight;
    public Material ActiveLightDim;
    public Material ActiveLightBright;

    private bool signalLightActive = false;
    private float signalLightActiveInterval;
    private float signalLightInactiveInterval;
    private float signalLightTimer = 0f;
    private bool activeLightActive = false;
    private bool playerInRadius = false;
    private SphereCollider triggerCollider;

    private void Awake() { // instance loaded
        SetLight(SignalLight, false);
        SetLight(ActiveLight, false);
        //
        triggerCollider = GetComponent<SphereCollider>();
        // load not placed
        RemoveTransceiver();
        Target = GetComponentInParent<VehicleHandler>();
    }

    public void SetLight(GameObject light, bool active) {
        // sets appropriate appearance to light
        if (active) {
            if (light == SignalLight) { 
                signalLightActive = true;
                light.GetComponent<Renderer>().material = SignalLightBright;
            }
            else if (light == ActiveLight) {
                activeLightActive = true;
                light.GetComponent<Renderer>().material = ActiveLightBright;
            }
            light.GetComponentInChildren<Light>().enabled = true;
        } else {
            if (light == SignalLight) {
                signalLightActive = false;
                light.GetComponent<Renderer>().material = SignalLightDim;
            }
            else if (light == ActiveLight) {
                activeLightActive = false;
                light.GetComponent<Renderer>().material = ActiveLightDim;
            }
            light.GetComponentInChildren<Light>().enabled = false;
        }
    }

    private void PlaceTransceiver() {
        Placed = true;
        foreach (Transform child in transform)
            child.gameObject.SetActive(true);
        transform.Find("Targetter").gameObject.SetActive(false);
        // increase radius of trigger for interacting
        triggerCollider.radius = 3f;
    }

    private void RemoveTransceiver() {
        Placed = false;
        foreach (Transform child in transform)
            child.gameObject.SetActive(false);
        // decrease radius of trigger for placing
        triggerCollider.radius = 2f;
    }

    private void Update() {
        if (Placed) { // this behaviour when the transceiver has been placed
            // flicker signal light depending on how close the player is
            if (playerInRadius) {
                signalLightTimer += Time.deltaTime;
                if (signalLightActive && signalLightTimer >= signalLightActiveInterval) {
                    SetLight(SignalLight, false);
                    signalLightTimer = 0f;
                }
                if (!signalLightActive && signalLightTimer >= signalLightInactiveInterval) {
                    SetLight(SignalLight, true);
                    signalLightTimer = 0f;
                }
            }
        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.gameObject.tag == "Player") {
            PlayerController player = other.GetComponent<PlayerController>();
            if (Placed) { // trigger has this behaviour when the transceiver has been placed
                playerInRadius = true;
                // calculates the distance from the player to the transceiver
                float dist = Vector3.Distance(player.transform.position, transform.position);
                if (dist > triggerCollider.radius) dist = dist - (dist - triggerCollider.radius);
                // affect the flickering intervals depending on how close the player is
                signalLightInactiveInterval = dist;
                signalLightActiveInterval = triggerCollider.radius - dist;
                if (signalLightActiveInterval <= 0.1f) signalLightActiveInterval = 0.1f;

                // handle input to hack the vehicle
                if (Target.Locked) { // car must be locked
                    if (!player.InMinigame) { // player must not already be in a game
                        if (player.Equipped == "Phone") { // player must have phone equipped
                            if (Input.GetMouseButton(0)) {
                                player.InMinigame = true;
                                GameObject.FindGameObjectWithTag("GameController").GetComponent<GameHandler>().InitiateGame(Target.AccessMethod, Target);
                            }
                        }
                    }
                }
            } else { // trigger has this behaviour when the transceiver not placed
                // ensure the player is holding the transceiver // only allow placing of the transciever when the hood is open
                if (player.Equipped == "Transceiver" && Target.GetComponentInChildren<VehicleHood>().Open) {
                // ensure the player is looking at the transceiver
                    if (Vector3.Dot((transform.position - player.transform.position), player.CameraForward) > 0.1f) {
                        transform.Find("Targetter").gameObject.SetActive(true);
                        // accept input to place transciever
                        if (Input.GetMouseButton(0))
                            PlaceTransceiver();
                    }
                } else
                    transform.Find("Targetter").gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (Placed) { // trigger has this behaviour when the transceiver has been placed
            if (other.gameObject.tag == "Player") {
                playerInRadius = false;
                signalLightTimer = 0f;
                SetLight(SignalLight, false);
            }
        } else { // trigger has this behaviour when the transceiver not placed
            transform.Find("Targetter").gameObject.SetActive(false);
        }
    }
}
