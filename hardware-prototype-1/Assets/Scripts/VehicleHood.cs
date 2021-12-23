using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleHood : MonoBehaviour {
    public bool Open = false;

    private Animator anim;
    private Transform indicator;

    private void Awake() { // instance loaded
        anim = GetComponent<Animator>();
        // load indicator
        indicator = transform.Find("Indicator");
        indicator.gameObject.SetActive(false);
    }

    public void DoorClosed() { // animation trigger // door closing animation complete
        Open = false;
    }

    public void DoorOpened() { // animation trigger // door opening animation complete
        Open = true;
    }

    private void DoorInteract() {
        if (Open) anim.Play("close");
        else anim.Play("open");
    }

    private void OnTriggerStay(Collider other) {
        if (other.gameObject.tag == "Player") {
            PlayerController player = other.GetComponent<PlayerController>();
            // ensure the player is looking at the hood
            if (Vector3.Dot((indicator.transform.position - player.transform.position), player.CameraForward) > 0.1f) {
                // handle indicator
                indicator.gameObject.SetActive(true);
                indicator.transform.LookAt(player.transform);
                // get input
                if (Input.GetAxis("Interact") > 0f) {
                    DoorInteract();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.tag == "Player") {
            indicator.gameObject.SetActive(false);
        }
    }
}
