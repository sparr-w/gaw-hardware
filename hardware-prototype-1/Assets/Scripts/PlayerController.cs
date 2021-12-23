using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public Vector2 MouseSensitivity = new Vector2(1f, 1f);
    public float Speed = 5.0f;
    public Equipment Inventory;

    private bool inMinigame;
    private Rigidbody rb;
    private Camera Camera;
    private float lookX;
    private float lookY;
    private Vector3 movement;

    private void Awake() { // instance loaded
        rb = GetComponent<Rigidbody>();
        //
        Camera = GetComponentInChildren<Camera>();
    }

    #region Setters and Getters
    public Vector3 CameraForward {
        get {return Camera.transform.forward;}
    }
    public string Equipped {
        get {if (Inventory.Equipped != null) return Inventory.Equipped.Name;
            else return "";}
    }
    public bool InMinigame {
        set {inMinigame = value;} get {return inMinigame;}
    }
    #endregion

    private void GetInput() {
        // mouse input // look around
        lookX = Input.GetAxis("Mouse X");
        lookY += Input.GetAxis("Mouse Y");
        lookY = Mathf.Clamp(lookY, -80f, 80f);
        // keyboard input // move around
        movement.z = Input.GetAxis("Vertical");
        movement.z = Mathf.Clamp(movement.z, -0.5f, 1f);
        movement.x = Input.GetAxis("Horizontal");
        movement.x = Mathf.Clamp(movement.x, -0.5f, 0.5f);
        // handle equipping items
        if (Input.GetKeyDown(KeyCode.Alpha1))
            Inventory.Equip(1);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            Inventory.Equip(2);
    }

    private void Update() { // every frame
        // only accept input if the game isn't paused
        if (!GameObject.FindGameObjectWithTag("GameController").GetComponent<GameHandler>().Paused) {
            // only accept input when the player is not in an active minigame
            if (!inMinigame)
                GetInput();
        }
    }

    private void Look(float X, float Y) {
        transform.Rotate(0f, X * 6f * MouseSensitivity.x, 0f);
        Camera.transform.localEulerAngles = new Vector3(-(Y * MouseSensitivity.y), Camera.transform.localEulerAngles.y, Camera.transform.localEulerAngles.z);
    }

    private void Move(float X, float Z) {
        Vector3 direction = rb.rotation * new Vector3(X, 0f, Z);
        rb.MovePosition(transform.position + (direction * Speed * Time.deltaTime));
    }

    private void FixedUpdate() { // every physics update
        // only move player if not paused
        if (!GameObject.FindGameObjectWithTag("GameController").GetComponent<GameHandler>().Paused) {
            // only move player if not in active minigame
            if (!inMinigame) {
                Look(lookX, lookY);
                Move(movement.x, movement.z);
            }
        }
    }
}
