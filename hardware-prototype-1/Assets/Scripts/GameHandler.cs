using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum AccessMethods {
    HackConnect,
    Bruteforce
}

public class GameHandler : MonoBehaviour {
    [Header("Components")]
    public GameObject UserInterface;
    public GameObject PauseScreen;
    public GameObject IntroductionScreen;
    [Header("Minigames")]
    public GameObject HackConnectPrefab;
    public GameObject BruteforcePrefab;
    [Header("Introductions")]
    public GameObject HackConnectIntroduction;
    public GameObject BruteforceIntroduction;

    private enum GameStates {
        Introduction,
        Active,
        Paused
    }
    private GameStates gameState;
    private GameObject currentGame;
    private GameObject currentOverlay = null;
    private bool overlayPresent = false;
    private bool hackconnectSeen = false;
    private bool bruteforceSeen = false;

    private void Start() {
        SwitchState(GameStates.Introduction);
        Time.timeScale = 0f;
    }

    private void SwitchState(GameStates state) {
        switch (state) {
            case GameStates.Introduction:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case GameStates.Active:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
            case GameStates.Paused:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }
        gameState = state;
    }

    public void InitiateGame(AccessMethods method, VehicleHandler vehicle) {
        switch (method) {
            case AccessMethods.HackConnect:
                currentGame = Instantiate(HackConnectPrefab, UserInterface.transform);
                HackConnect hackConnect = currentGame.GetComponent<HackConnect>();
                hackConnect.GenerateGame(vehicle);
                break;
            case AccessMethods.Bruteforce:
                currentGame = Instantiate(BruteforcePrefab, UserInterface.transform);
                Bruteforce bruteForce = currentGame.GetComponent<Bruteforce>();
                bruteForce.GenerateGame(vehicle);
                break;
        }
    }

    public void DisplayOverlay(AccessMethods method) {
        if (!overlayPresent) {
            switch (method) {
                case AccessMethods.HackConnect:
                    if (!hackconnectSeen) {
                        currentOverlay = Instantiate(HackConnectIntroduction, UserInterface.transform);
                        hackconnectSeen = true;
                        overlayPresent = true;
                    }
                    break;
                case AccessMethods.Bruteforce:
                    if (!bruteforceSeen) {
                        currentOverlay = Instantiate(BruteforceIntroduction, UserInterface.transform);
                        bruteforceSeen = true;
                        overlayPresent = true;
                    }
                    break;
            }
        }
    }

    public void RemoveOverlay() {
        Destroy(currentOverlay.gameObject);
        currentOverlay = null;
        overlayPresent = false;
    }

    public void PauseGame() {
        Time.timeScale = 0f;
        SwitchState(GameStates.Paused);
        UserInterface.SetActive(false);
        PauseScreen.SetActive(true);
    }

    public void ResumeGame() {
        Time.timeScale = 1f;
        SwitchState(GameStates.Active);
        UserInterface.SetActive(true);
        PauseScreen.SetActive(false);
        IntroductionScreen.SetActive(false);
    }

    public void ResetScene() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }

    public void Quit() {
        Application.Quit();
    }

    public bool Paused {
        get {
            if (gameState == GameStates.Paused)
                return true;
            return false;
        }
    }

    private void Update() {
        if (overlayPresent) {
            if (Input.GetKey(KeyCode.Space))
                RemoveOverlay();
        }

        switch (gameState) {
            case GameStates.Introduction:
                if (Input.GetKey(KeyCode.Space))
                    ResumeGame();
                break;
            case GameStates.Active:
                if (Input.GetKeyDown(KeyCode.Escape)) {
                    if (GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().InMinigame) {
                        Destroy(currentGame.gameObject);
                        currentGame = null;
                        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().InMinigame = false;
                    }
                    else
                        PauseGame();
                }
                break;
            case GameStates.Paused:
                if (Input.GetKeyDown(KeyCode.Escape))
                    ResumeGame();
                break;
        }
    }
}