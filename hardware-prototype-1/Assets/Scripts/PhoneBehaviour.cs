using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneBehaviour : MonoBehaviour {
    public List<GameObject> TransceiversInRange;
    public GameObject ClosestTransceiver;
    [Header("Components")]
    public GameObject SignalComponent;
    public GameObject BasePercentComponent;
    public GameObject TwentyPercentComponent;
    public GameObject FourtyPercentComponent;
    public GameObject SixtyPercentComponent;
    public GameObject EightyPercentComponent;
    public GameObject HundredPercentComponent;
    public GameObject ErrorComponent;

    private float maxRange;
    private bool justWoke;

    private void Awake() { // instance loaded
        maxRange = GetComponent<SphereCollider>().radius;
        justWoke = true;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Transceiver") {
            // ensure the transceiever is placed
            if (other.gameObject.GetComponent<Transceiver>().Placed) {
                // ensure it doesn't add a transceiver that is already in the range
                bool alreadyExists = false;
                foreach (GameObject obj in TransceiversInRange)
                    if (obj == other.gameObject) alreadyExists = true;
                if (!alreadyExists) 
                    TransceiversInRange.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.tag == "Transceiver") {
            // if transceiver exists in the list, remove it
            foreach (GameObject obj in TransceiversInRange) {
                if (obj == other.gameObject) {
                    if (obj == ClosestTransceiver) ClosestTransceiver = null;
                    TransceiversInRange.Remove(obj);
                    return;
                }
            }
        }
    }

    private void OnTriggerStay(Collider other) {
        // this should only be called once, when the player equips the phone
        if (justWoke) {
            if (other.gameObject.tag == "Transceiver") {
                // ensure the transceiever is placed
                if (other.gameObject.GetComponent<Transceiver>().Placed) {
                    // ensure it doesn't add a transceiver that is already in the range
                    bool alreadyExists = false;
                    foreach (GameObject obj in TransceiversInRange)
                        if (obj == other.gameObject) alreadyExists = true;
                    if (!alreadyExists) 
                        TransceiversInRange.Add(other.gameObject);
                }
            }
        }
    }

    private void CalculateClosest() {
        // if nothing is selected then select first index for comparison
        if (ClosestTransceiver == null)
            ClosestTransceiver = TransceiversInRange[0];
        foreach (GameObject obj in TransceiversInRange) {
            if (Vector3.Distance(transform.position, obj.transform.position) > Vector3.Distance(transform.position, ClosestTransceiver.transform.position))
                ClosestTransceiver = obj;
        }
    }

    private void UpdateSignal() {
        // the radius is maximum range 
        float signalStrength = 1f - (Vector3.Distance(transform.position, ClosestTransceiver.transform.position) / maxRange);
        // set appropriate signal bars to active
        if (signalStrength >= .6f) {
            HundredPercentComponent.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, (0.15f - (0.75f - signalStrength)) / 0.15f);
            HundredPercentComponent.SetActive(true);
        } else HundredPercentComponent.SetActive(false);
        if (signalStrength >= .45f) {
            EightyPercentComponent.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, (0.15f - (0.6f - signalStrength)) / 0.15f);
            EightyPercentComponent.SetActive(true);
        } else EightyPercentComponent.SetActive(false);
        if (signalStrength >= .3f) {
            SixtyPercentComponent.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, (0.15f - (0.45f - signalStrength)) / 0.15f);
            SixtyPercentComponent.SetActive(true);
        } else SixtyPercentComponent.SetActive(false);
        if (signalStrength >= .15f) {
            FourtyPercentComponent.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, (0.15f - (0.3f - signalStrength)) / 0.15f);
            FourtyPercentComponent.SetActive(true);
        } else FourtyPercentComponent.SetActive(false);
        if (signalStrength >= 0f) {
            TwentyPercentComponent.SetActive(true);
            TwentyPercentComponent.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, (0.15f - (0.15f - signalStrength)) / 0.15f);
            //
            BasePercentComponent.SetActive(true);
            ErrorComponent.SetActive(false);
        } else {
            TwentyPercentComponent.SetActive(false);
            //
            BasePercentComponent.SetActive(false);
            ErrorComponent.SetActive(true);
        }
    }

    private void Update() {
        if (justWoke) justWoke = false;
        if (TransceiversInRange.Count > 0) {
            CalculateClosest();
            UpdateSignal();
        }
    }
}
