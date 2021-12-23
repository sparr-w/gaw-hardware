using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour {
    private string FormatTimer(float value) {
        float time = value;
        // handle minutes
        float minutes = time / 60f;
        minutes = Mathf.Floor(minutes);
        time -= minutes * 60f;
        // handle seconds
        float seconds = Mathf.Floor(time);
        time -= seconds;
        // handle milliseconds
        float milliseconds = time * 1000f;
        milliseconds = Mathf.Floor(milliseconds);
        // format values into a string
        string concat = "";
        for (int i = 2 - minutes.ToString().Length; i > 0; i--)
            concat += "0";
        concat += minutes + ":";
        for (int j = 2 - seconds.ToString().Length; j > 0; j--)
            concat += "0";
        concat += seconds + ":";
        for (int k = 3 - milliseconds.ToString().Length; k > 0; k--)
            concat += "0";
        concat += milliseconds;
        return concat;
    }

    public void UpdateTimer(float time) {
        GetComponent<Text>().text = "" + FormatTimer(time);
    }

    private void TickSound() {
        GetComponent<AudioSource>().PlayOneShot(GetComponent<AudioSource>().clip);
    }

    public void PlaySound() {
        InvokeRepeating("TickSound", 0f, 1f);
    }

    public void StopSound() {
        CancelInvoke("TickSound");
        GetComponent<AudioSource>().volume = 0;
    }
}
