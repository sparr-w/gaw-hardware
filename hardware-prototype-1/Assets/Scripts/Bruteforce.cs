using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bruteforce : MonoBehaviour {
    public VehicleHandler vehicleToUnlock;
    public string[] Combinations;
    public string Combination;
    [Header("Components")]
    public BruteforceTargetter TargetterComponent;
    public RectTransform SelectorComponent;
    public GameTimer TimerComponent;
    public GameObject ColumnsComponent;
    public GameObject WinSplash;
    public GameObject FailSplash;
    public GameObject FreezeCover;
    public Transform AttemptsComponent;
    [Header("Variables")]
    public float Speed = 300f;
    public float GameTime = 60f;
    [Header("Audio Clips")]
    public AudioSource MoveSound;
    public AudioSource CorrectSound;
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
    private BruteforceCell[] cells;
    private BruteforceColumn[] columns;
    private int selectColumn;
    private float timer;
    private bool incorrectFreeze;
    private float freezeTimer;
    private bool resetReady;
    private int remainingAttempts;

    private void Awake() { // instance loaded
        // fix combinations format
        for (int i = 0; i < Combinations.Length; i++)
            Combinations[i] = Combinations[i].ToUpper();
        // generate game
        gameState = GameStates.Inactive;
    }

    public void GenerateGame(VehicleHandler vehicle, float gameTime = 60f, float speed = 300f) {
        vehicleToUnlock = vehicle;
        // collate cells and columns
        cells = GetComponentsInChildren<BruteforceCell>();
        columns = GetComponentsInChildren<BruteforceColumn>();
        foreach (BruteforceColumn column in columns)
            column.Locked = false;
        // generate cells and target
        GenerateCells();
        GenerateTarget();
        // generate selector
        selectColumn = 0;
        SelectorComponent.anchoredPosition = new Vector2(columns[selectColumn].GetComponent<RectTransform>().anchoredPosition.x, SelectorComponent.anchoredPosition.y);
        // speed
        Speed = speed;
        // handle timer
        GameTime = gameTime;
        timer = GameTime;
        TimerComponent.PlaySound();
        // handle attempts and incorrect attempts
        remainingAttempts = AttemptsComponent.childCount;
        incorrectFreeze = false;
        freezeTimer = 0f;
        resetReady = true;
        //
        gameState = GameStates.Active;
    }

    private void GenerateCells() {
        foreach (BruteforceCell cell in cells)
            cell.GenerateCell();
    }

    private void GenerateTarget() {
        Combination = Combinations[Random.Range(0, Combinations.Length)];
        for (int i = 0; i < columns.Length; i++)
            columns[i].GenerateTarget(Combination[i]);
    }

    private void MoveColumns() {
        for (int i = 0; i < columns.Length; i++) {
            if (!columns[i].Locked) { // don't move locked columns in regular pattern
                columns[i].GetComponent<RectTransform>().anchoredPosition += new Vector2(0f, -(Speed) * Time.deltaTime);
                // rejig
                if (columns[i].GetComponent<RectTransform>().anchoredPosition.y <= -20f) {
                    columns[i].GetComponent<RectTransform>().anchoredPosition += new Vector2(0f, cells[i].GetComponent<RectTransform>().sizeDelta.y);
                    columns[i].transform.GetChild(columns[i].transform.childCount - 1).SetAsFirstSibling();
                }
            } else if (!columns[i].InLockPosition) { // move locked columns towards centre based on position of target cell
                float distance = columns[i].cells[columns[i].TargetIndex].RectTransform.position.y - TargetterComponent.RectTransform.position.y;
                if (Mathf.Abs(distance) > Speed * Time.deltaTime) { // move normally towards the centre
                    if (distance < 0f) // move upwards
                        columns[i].GetComponent<RectTransform>().anchoredPosition += new Vector2(0f, Speed * Time.deltaTime);
                    else // move downwards
                        columns[i].GetComponent<RectTransform>().anchoredPosition += new Vector2(0f, -(Speed) * Time.deltaTime);
                } else { // too close, lock into position
                    columns[i].InLockPosition = true;
                    columns[i].cells[columns[i].TargetIndex].RectTransform.position = new Vector2(columns[i].cells[columns[i].TargetIndex].RectTransform.position.x, TargetterComponent.RectTransform.position.y);
                }
            }
        }
    }

    private bool Attempt() {
        // check whether the select column's target character is in the targetter
        if (columns[selectColumn].cells[columns[selectColumn].TargetIndex].RectTransform.position.y <= TargetterComponent.TopBorder.position.y) {
            // middle of target cell is beyond the top border of targetter
            if (columns[selectColumn].cells[columns[selectColumn].TargetIndex].RectTransform.position.y >= TargetterComponent.BottomBorder.position.y) {
                // middle of target is above the bottom border of targetter
                return true;
            }
        }
        return false;
    }

    private int LockCount {
        get { int _count = 0;
            foreach (BruteforceColumn column in columns)
                if (column.Locked) _count++;
            return _count; }
    }

    private void LockSelected() {
        columns[selectColumn].Locked = true;
        // due to bug, ensure target is sibling index 3, this occurs right as the last sibling is changed to first sibling at the time the user locks in the column, readjust
        if (columns[selectColumn].cells[columns[selectColumn].TargetIndex].transform.GetSiblingIndex() != 3) {
            columns[selectColumn].GetComponent<RectTransform>().anchoredPosition -= new Vector2(0f, cells[0].GetComponent<RectTransform>().sizeDelta.y);
            columns[selectColumn].transform.GetChild(0).SetAsLastSibling();
        }
        // change font color of locked column to green
        columns[selectColumn].cells[columns[selectColumn].TargetIndex].GetComponentInChildren<Text>().color = new Color(0f, 1f, 0f, 1f);
        // check how many are locked and set victorious if all are locked
        if (LockCount == columns.Length) {
            // change to win screen
            ColumnsComponent.SetActive(false);
            TargetterComponent.gameObject.SetActive(false);
            WinSplash.SetActive(true);
            // handle timer
            VictorySound.PlayOneShot(VictorySound.clip);
            TimerComponent.StopSound();
            // change game state
            gameState = GameStates.Victory;
        }
        else // if the game isn't victorious, move selector off the now locked column
            MoveSelector(1, true);
    }

    private void IncorrectAttempt() {
        // update remaining attempts
        remainingAttempts--;
        foreach (Transform child in AttemptsComponent.GetChild(remainingAttempts))
            child.gameObject.SetActive(false);
        // game fail
        if (remainingAttempts <= 0)
            Fail();
        else { // if there are remaining attempts, proceed with visuals for incorrect attempt
            incorrectFreeze = true;
            freezeTimer = 1f;
            FreezeCover.SetActive(true);
            IncorrectSound.PlayOneShot(IncorrectSound.clip);
        }
    }

    private void ResetColumns() {
        // reset position and randomise cells
        foreach (BruteforceColumn column in columns) {
            column.Locked = false;
            column.InLockPosition = false;
            column.GetComponent<RectTransform>().anchoredPosition = new Vector2(column.GetComponent<RectTransform>().anchoredPosition.x, 0f);
            column.cells[column.TargetIndex].GetComponentInChildren<Text>().color = Color.red;
            // randomise cells
            for (int i = column.cells.Length - 1; i >= 0; i--)
                column.cells[Random.Range(0, i + 1)].transform.SetAsFirstSibling();
        }
        resetReady = false;
    }

    private void Fail() {
        // change to fail screen
        ColumnsComponent.SetActive(false);
        TargetterComponent.gameObject.SetActive(false);
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

    private void MoveSelector(int dir, bool fromLock = false) {
        selectColumn += dir;
        // prevent landing on a locked column, but keep moving in that direction
        if (selectColumn < columns.Length && selectColumn >= 0) {
            if (columns[selectColumn].Locked) {
                if (dir > 0) { // move right
                    for (int i = selectColumn; i <= columns.Length; i++) {
                        selectColumn = i;
                        if (i < columns.Length) {
                            if (!columns[i].Locked)
                                break;
                        }
                    }
                } else { // move left
                    for (int i = selectColumn; i >= -1; i--) {
                        selectColumn = i;
                        if (i >= 0) {
                            if (!columns[i].Locked)
                                break;
                        }
                    }
                }
            }
        }
        // portal // prevent landing on a locked column
        if (selectColumn > columns.Length - 1) { // moving right
            for (int i = 0; i < columns.Length; i++) {
                if (!columns[i].Locked) {
                    selectColumn = i;
                    break;
                }
            }
        }
        else if (selectColumn < 0) { // moving left
            for (int i = columns.Length - 1; i >= 0; i--) {
                if (!columns[i].Locked) {
                    selectColumn = i;
                    break;
                }
            }
        }
        // alter position
        SelectorComponent.anchoredPosition = new Vector2(columns[selectColumn].GetComponent<RectTransform>().anchoredPosition.x, SelectorComponent.anchoredPosition.y);
        // play appropriate sound
        if (fromLock) CorrectSound.PlayOneShot(CorrectSound.clip);
        else MoveSound.PlayOneShot(MoveSound.clip);
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
                //
                if (!incorrectFreeze) { // if not frozen by incorrect attempt
                    MoveColumns();
                    // handle moving the column selector
                    if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                        MoveSelector(-1);
                    if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                        MoveSelector(1);
                    // handle locking in target cells
                    if (Input.GetKeyDown(KeyCode.Space)) {
                        if (Attempt()) // successful attempt
                            LockSelected();
                        else // unsuccessful attempt
                            IncorrectAttempt();
                    }
                } else { // flash red, fade back to game froze, flash red again, fade back to columns unlocked, once cover removed unfreeze game
                    freezeTimer -= Time.deltaTime;
                    if (freezeTimer > 0.5f) {
                        FreezeCover.GetComponent<Image>().color = new Color(0.6f, 0f, 0f, 0.8f * ((freezeTimer - 0.5f) / 0.5f));
                    } else if (freezeTimer > 0f) {
                        // reset columns and generate random positions
                        if (resetReady) ResetColumns();
                        MoveColumns();
                        FreezeCover.GetComponent<Image>().color = new Color(0.6f, 0f, 0f, 0.8f * (freezeTimer / 0.5f));
                    } else {
                        FreezeCover.SetActive(false);
                        freezeTimer = 0f;
                        incorrectFreeze = false;
                        resetReady = true;
                    }
                }
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