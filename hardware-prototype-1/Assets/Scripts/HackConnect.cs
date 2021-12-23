using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HackConnect : MonoBehaviour {
    public VehicleHandler vehicleToUnlock;
    [Header("Components")]
    public GameTimer TimerComponent;
    public Text TargetComponent;
    public Transform CellsComponent;
    public Transform AttemptsComponent;
    public GameObject WinSplash;
    public GameObject FailSplash;
    [Header("Variables")]
    public float GameTime;
    public float CellMoveInterval;
    [Header("Audio Clips")]
    public AudioSource MoveSound;
    public AudioSource FailSound;
    public AudioSource VictorySound;
    public AudioSource IncorrectSound;

    private enum GameStates {
        Inactive,
        Active,
        Victory,
        Fail
    }
    private GameStates gameState;
    private HackConnectCell[] cells;
    private float timer;
    private float cellMoveTimer;
    private int controllerPos = 33;
    private int targetIndex;
    private int remainingAttempts;

    private void Awake() { // instance loaded
        gameState = GameStates.Inactive;
    }

    public void GenerateGame(VehicleHandler vehicle, float gameTimeInSeconds = 60f, float cellMoveIntervalInSeconds = 1f) {
        vehicleToUnlock = vehicle;
        //
        cells = GetComponentsInChildren<HackConnectCell>();
        GameTime = gameTimeInSeconds;
        CellMoveInterval = cellMoveIntervalInSeconds;
        // generate game cells
        foreach (HackConnectCell cell in cells)
            cell.GenerateCell();
        UpdateCellAppearance();
        // handle timers
        timer = GameTime;
        cellMoveTimer = 0f;
        TimerComponent.PlaySound();
        // generate target and attempts
        GenerateTarget();
        remainingAttempts = AttemptsComponent.childCount;
        //
        gameState = GameStates.Active;
    }

    private void GenerateTarget() {
        // generate the target based on the cells array
        targetIndex = Random.Range(0, cells.Length);
        // get string concatenation for the target display
        string concat = "" + cells[targetIndex].GetComponentInChildren<Text>().text;
        for (int i = 1; i < 4; i++) {
            if ((targetIndex + i) > (cells.Length - 1)) concat += "." + cells[targetIndex + i - cells.Length].GetComponentInChildren<Text>().text;
            else concat += "." + cells[targetIndex + i].GetComponentInChildren<Text>().text;
        }
        TargetComponent.text = concat;
    }

    private void UpdateCellAppearance() {
        // set all to white and then update the appearance of the selected IP
        foreach (HackConnectCell cell in cells)
            cell.GetComponentInChildren<Text>().color = Color.white;
        for (int i = 0; i < 4; i++) {
            if (controllerPos + i > cells.Length - 1) CellsComponent.GetChild(controllerPos + i - cells.Length).GetComponentInChildren<Text>().color = Color.red;
            else CellsComponent.GetChild(controllerPos + i).GetComponentInChildren<Text>().color = Color.red;
        }
    }

    private void MoveCells() {
        CellsComponent.GetChild(0).SetAsLastSibling();
        UpdateCellAppearance();
    }

    private void MoveController(int value) {
        controllerPos += value;
        // correct position if player goes beyond bounds
        if (controllerPos < 0)
            controllerPos += cells.Length;
        else if (controllerPos > cells.Length - 1)
            controllerPos -= cells.Length;
        UpdateCellAppearance();
        // play sound
        MoveSound.PlayOneShot(MoveSound.clip);
    }

    private void Attempt() {
        if (controllerPos == cells[targetIndex].transform.GetSiblingIndex()) {
            // change to win screen
            CellsComponent.gameObject.SetActive(false);
            WinSplash.SetActive(true);
            TargetComponent.GetComponentInChildren<Text>().color = new Color(0f, 0.8f, 0f, 1f);
            // handle timer
            TimerComponent.StopSound();
            // change game state
            VictorySound.PlayOneShot(VictorySound.clip);
            gameState = GameStates.Victory;
        } else {
            remainingAttempts--;
            // update UI
            foreach (Transform child in AttemptsComponent.GetChild(remainingAttempts))
                child.gameObject.SetActive(false);
            // game fail
            if (remainingAttempts <= 0)
                Fail();
            else // if game didn't fail play incorrect sound
                IncorrectSound.PlayOneShot(IncorrectSound.clip);
        }
    }

    private void Fail() {
        // change to fail screen
        CellsComponent.gameObject.SetActive(false);
        FailSplash.SetActive(true);
        TimerComponent.GetComponent<Text>().color = new Color(.8f, 0f, 0f, 1f);
        foreach (Transform child in AttemptsComponent) {
            foreach (Transform c in child) 
                c.gameObject.SetActive(false);
            child.GetComponent<Image>().color = new Color(.8f, 0f, 0f, 1f);
        }
        // handle sounds
        FailSound.PlayOneShot(FailSound.clip);
        TimerComponent.StopSound();
        // set state and await input to close game
        gameState = GameStates.Fail;
    }

    private void Update() {
        switch (gameState) { 
            case GameStates.Active:
                // handle timer
                if (timer > 0f) {
                    if (Time.deltaTime > timer) timer = 0f; // time hits zero rather than negative number
                    else timer -= Time.deltaTime;
                    TimerComponent.UpdateTimer(timer);
                } else // ran out of time
                    Fail();
                // handle moving cells
                cellMoveTimer += Time.deltaTime;
                if (cellMoveTimer >= CellMoveInterval) {
                    MoveCells();
                    cellMoveTimer = 0f;
                }
                // handle player controller
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) MoveController(-10);
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) MoveController(-1);
                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) MoveController(10);
                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) MoveController(1);
                // handle attempt
                if (Input.GetKeyDown(KeyCode.Space)) Attempt();
                break;
            case GameStates.Victory:
                // taking any input now will unlock the target vehicle
                if (Input.anyKeyDown) {
                    vehicleToUnlock.UnlockVehicle();
                    GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().InMinigame = false;
                    Destroy(gameObject);
                }
                break;
            case GameStates.Fail:
                if (Input.anyKeyDown) {
                    GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().InMinigame = false;
                    Destroy(gameObject);
                }
                break;
        }
    }
}
