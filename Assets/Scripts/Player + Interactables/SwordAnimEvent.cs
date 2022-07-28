using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAnimEvent : MonoBehaviour {

    private Movement playerScript;
    private PlayerInventory inventoryScript;
    private BoxCollider bc;
    private SkinnedMeshRenderer smr;


    void Start() {
        playerScript = GameObject.FindWithTag("Player").GetComponent<Movement>();
        inventoryScript = GameObject.FindWithTag("Player").GetComponent<PlayerInventory>();

        if (gameObject.tag == "Arm") { smr = transform.GetChild(9).gameObject.GetComponent<SkinnedMeshRenderer>(); }
    }

    void Update() {
        if (gameObject.tag == "Arm") {
            if (playerScript.playerMovementState == Movement.MovementState.Climb) {
                smr.enabled = false;
            }
            else {
                smr.enabled = true;
            }
        }
    }

    public void activateWeaponCollider() {
        int i = (gameObject.tag == "Arm") ? inventoryScript.getInventorySlot() - 1 : 0;
        if (transform.GetChild(i).gameObject.tag != "Empty Slot") {
            bc = transform.GetChild(i).gameObject.GetComponent<BoxCollider>();
            bc.enabled = true;
        }
    }

    public void deactivateWeaponCollider() {
        int i = (gameObject.tag == "Arm") ? inventoryScript.getInventorySlot() - 1 : 0;
        if (transform.GetChild(i).gameObject.tag != "Empty Slot") {
            bc = transform.GetChild(i).gameObject.GetComponent<BoxCollider>();
            bc.enabled = false;
        }
    }
}
