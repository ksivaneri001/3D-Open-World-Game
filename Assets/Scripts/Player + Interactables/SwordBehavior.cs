using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordBehavior : MonoBehaviour {

    public float attackSpeedMultiplier;
    public int damage;
    public float weight;
    public float durability;
    public float adrenalinePerSwing;

    public GameObject container;
    public GameObject slot;

    public Vector3 heldPosition;
    public Vector3 heldRotation;
    private Quaternion heldRotationQuat;

    private Quaternion prefabRotation;

    private Animator anim;
    private MeshRenderer mr;
    private BoxCollider bc;
    private Movement playerScript;
    private PlayerInventory inventoryScript;


    public enum WeaponState {
        Dropped,
        Held,
        InInventory,
        HeldByEnemy
    }

    public WeaponState currentWeaponState = WeaponState.Dropped;


    void Start() {
        mr = gameObject.GetComponent<MeshRenderer>();
        bc = gameObject.GetComponent<BoxCollider>();

        heldRotationQuat.eulerAngles = heldRotation;
    }


    void Update() {
        checkState();

        if (playerScript == null || !playerScript.getEscapeDown()) {
            if (anim != null) {
                anim.enabled = true;
            }

            if (currentWeaponState == WeaponState.Held) {
                mr.enabled = true;
                anim.SetFloat("windupSpeed", attackSpeedMultiplier);
                anim.SetFloat("returnSpeed", Mathf.Log(1.5f + (2f * attackSpeedMultiplier)));

                transform.localPosition = heldPosition;
                transform.localRotation = heldRotationQuat;

                if (playerScript.playerMovementState == Movement.MovementState.Walk && !playerScript.getEscapeDown() && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && Input.GetKeyDown(KeyCode.Mouse0)) {
                    anim.SetTrigger("swing");
                }
                
                if (Input.GetKeyDown(KeyCode.Q) && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
                    dropWeapon(transform.parent.transform.parent.transform.parent.gameObject);
                }
            }
            else if (currentWeaponState == WeaponState.HeldByEnemy) {
                mr.enabled = true;
                transform.localPosition = heldPosition;
                transform.localRotation = heldRotationQuat;
            }
            else {
                bc.enabled = false;

                if (currentWeaponState == WeaponState.Dropped) {
                    mr.enabled = true;
                }
                else if (currentWeaponState == WeaponState.InInventory) {
                    mr.enabled = false;
                }
            }
        }
        else {
            if (anim != null) {
                anim.enabled = false;
            }
        }
    }


    void OnDrawGizmos() {
        Debug.DrawRay(transform.position, transform.forward, Color.black);
        Debug.DrawRay(transform.position, transform.up, Color.white);
    }


    void checkState() {
        playerScript = null;
        inventoryScript = null;

        if (transform.parent.gameObject.tag == "Arm") {
            anim = transform.parent.gameObject.GetComponent<Animator>();
            playerScript = transform.parent.transform.parent.transform.parent.GetComponent<Movement>();
            inventoryScript = transform.parent.transform.parent.transform.parent.GetComponent<PlayerInventory>();

            if (inventoryScript.getInventorySlot() - 1 == transform.GetSiblingIndex() && playerScript.playerMovementState != Movement.MovementState.Climb) {
                currentWeaponState = WeaponState.Held;
            }
            else {
                currentWeaponState = WeaponState.InInventory;
            }
        }
        else if (transform.parent.gameObject.tag == "Enemy") {
            anim = transform.parent.gameObject.GetComponent<Animator>();
            currentWeaponState = WeaponState.HeldByEnemy;
        }
        else {
            anim = null;
        }
    }


    public void dropWeapon(GameObject p) {
        Vector3 parentPosition = p.transform.position;
        transform.parent = null;
        currentWeaponState = WeaponState.Dropped;

        if (p.tag == "Player") {
            GameObject newEmptySlot = Instantiate(slot, Vector3.zero, Quaternion.identity);
            newEmptySlot.transform.parent = p.transform.GetChild(0).transform.GetChild(0);
            newEmptySlot.transform.SetSiblingIndex(inventoryScript.getInventorySlot() - 1);
        }

        prefabRotation.eulerAngles = new Vector3(5f, 0f, 0f);
        GameObject newContainer = Instantiate(container, (new Vector3(parentPosition.x, parentPosition.y + 1, parentPosition.z)) + p.transform.forward, prefabRotation);
        transform.parent = newContainer.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Vector3 parentVelocity = p.GetComponent<Rigidbody>().velocity;
        newContainer.GetComponent<Rigidbody>().AddForce(new Vector3(parentVelocity.x, parentVelocity.y * 0.5f, parentVelocity.z), ForceMode.Impulse);
    }

    public int getDamage() {
        return damage;
    }

    public float getWeight() {
        return weight;
    }
}
