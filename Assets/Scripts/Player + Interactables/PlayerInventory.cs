using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour {

    private int inventorySlot;
    public Font slotFont;

    private Animator anim;

    void Start() {
        anim = transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Animator>();

        inventorySlot = 1;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1)) { inventorySlot = 1; anim.Play("Idle", -1); }
        if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2)) { inventorySlot = 2; anim.Play("Idle", -1); }
        if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3)) { inventorySlot = 3; anim.Play("Idle", -1); }
        if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4)) { inventorySlot = 4; anim.Play("Idle", -1); }
        if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5)) { inventorySlot = 5; anim.Play("Idle", -1); }
        if (Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6)) { inventorySlot = 6; anim.Play("Idle", -1); }
        if (Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Alpha7)) { inventorySlot = 7; anim.Play("Idle", -1); }
        if (Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Alpha8)) { inventorySlot = 8; anim.Play("Idle", -1); }
        if (Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Alpha9)) { inventorySlot = 9; anim.Play("Idle", -1); }
    }

    void OnGUI() {
        string slotOutput = "" + inventorySlot;
        GUIStyle slotStyle = new GUIStyle();

        slotStyle.font = slotFont;
        slotStyle.fontSize = 128;
        slotStyle.fontStyle = FontStyle.Bold;
        slotStyle.alignment = TextAnchor.MiddleCenter;

        GUI.Label(new Rect(40, Screen.height - 180, 128, 128), slotOutput, slotStyle);
    }

    public int getInventorySlot() {
        return inventorySlot;
    }
    public void setInventorySlot(int s) {
        inventorySlot = s;
    }
}
