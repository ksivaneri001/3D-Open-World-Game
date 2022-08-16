using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    // Walk State Instance Variables

    public float forcePerSecond;
    public float jumpForce;

    private bool canJump = false;
    private float rayDownLength = 1.3f;

    private float dragCounterForce = 0;
    public float dragCounterForcePerSecond;

    private float movementX = 0;
    private float movementZ = 0;

    public float maxFallSpeed;
    public float speedMultiplier;


    // Climb State Instance Variables

    public float climbSpeedUp;
    public float climbSpeedDown;

    public float turnDegreesPerSecond;

    private int climbDirectionModifier = 1;
    private bool swapClimb = false;


    // Hit State Instance Variables

    public int maxHealth;
    private int health;

    public float maxHitSpeed;
    public float hitBounciness;

    private float hitStunTime;
    private float hitStunStartTime;

    private float baseDrag;


    // Misc Instance Variables

    public float weight;

    private Vector3 startPos;
    private Rigidbody rb;
    private Animator anim;
    private CapsuleCollider cc;
    private PhysicMaterial walkState_pm;
    private PlayerInventory inventoryScript;

    private int groundRayMask = 1 << 3; // Inverted in Start()
    private int climbableRayMask = 1 << 6;
    private int containerRayMask = 1 << 7;

    private bool escapeDown = false;
    public Font promptFont;
    private string keyPrompt = "";

    public enum MovementState {
        Walk,
        Climb,
        Hit
    }

    public MovementState playerMovementState = MovementState.Walk;

    void Start() {
        rb = gameObject.GetComponent<Rigidbody>();
        anim = transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Animator>();
        cc = gameObject.GetComponent<CapsuleCollider>();
        walkState_pm = cc.material;
        inventoryScript = gameObject.GetComponent<PlayerInventory>();
        groundRayMask = ~groundRayMask;
        dragCounterForcePerSecond *= 100;
        health = maxHealth;
        baseDrag = rb.drag;
        startPos = transform.position;
    }

    void FixedUpdate() {
        if (!escapeDown) {
            if (playerMovementState == MovementState.Walk || playerMovementState == MovementState.Hit) {
                rb.AddForce(transform.up * -1 * dragCounterForce * Time.deltaTime, ForceMode.Force);
            }
            if (playerMovementState == MovementState.Hit) {
                if (rb.velocity.magnitude > maxHitSpeed) {
                    rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxHitSpeed);
                }
            }
        }
    }

    void Update() {
        Debug.Log(cc.material.bounciness);

        if (!escapeDown) {

            if (playerMovementState == MovementState.Walk) {
                jump();
                checkInteractables();
                walkMovement();
                checkSpeed();
            }
            else if (playerMovementState == MovementState.Climb) {
                climbMovement();
            }
            else if (playerMovementState == MovementState.Hit) {
                rb.useGravity = true;

                hitStunTime -= Time.deltaTime;

                rb.drag = ((hitStunStartTime - hitStunTime) / hitStunStartTime) * 0.25f * baseDrag;

                if (hitStunTime <= 0) {
                    rb.drag = baseDrag;
                    cc.material = walkState_pm;
                    playerMovementState = MovementState.Walk;
                }
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Physics.autoSimulation = true;
        }
        else {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Physics.autoSimulation = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            escapeDown = !escapeDown;
        }
    }


    // Misc Functions    

    void reset() {
        health = maxHealth;
        playerMovementState = MovementState.Walk;
        transform.position = startPos;
        rb.velocity = new Vector3(0, 0, 0);
    }

    void OnTriggerEnter(Collider col) {
        if (playerMovementState != MovementState.Hit && col.gameObject.tag == "Sword" && col.gameObject != transform.GetChild(0).GetChild(0).GetChild(0).gameObject) {
            SwordBehavior sb = col.gameObject.GetComponent<SwordBehavior>();

            int h = health - sb.getDamage();
            health = (h <= 0) ? 0 : h;

            if (health == 0) {
                reset();
            }

            rb.AddForce(
                ((2.7f * col.gameObject.transform.forward) + Vector3.up) * sb.getWeight() * (5f / weight),
                ForceMode.Impulse
            );
            hitStunTime = Mathf.Log(1f + (sb.getWeight() * (6f / weight) * 0.2f));
            hitStunStartTime = hitStunTime;
            rb.drag = 0f;

            PhysicMaterial new_pm = new PhysicMaterial();
            new_pm.bounciness = hitBounciness;
            cc.material = new_pm;

            playerMovementState = MovementState.Hit;
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 0.5f, 2.5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 2.25f, 0.75f);
    }

    void OnGUI() {
        GUIStyle promptStyle = new GUIStyle();

        promptStyle.font = promptFont;
        promptStyle.fontSize = 128;
        promptStyle.fontStyle = FontStyle.Bold;
        promptStyle.alignment = TextAnchor.MiddleCenter;

        GUIStyle healthStyle = new GUIStyle();

        healthStyle.font = promptFont;
        healthStyle.fontSize = 96;
        healthStyle.fontStyle = FontStyle.Bold;

        GUI.Label(new Rect(960, Screen.height - 180, 128, 128), keyPrompt, promptStyle);
        GUI.Label(new Rect(40, 30, 128, 128), "HP: " + health.ToString() + " / " + maxHealth.ToString(), healthStyle);
    }


    // Walk State Functions

    void jump() {
        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * rayDownLength, Color.green);

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), rayDownLength, groundRayMask)) {
            canJump = true;
            dragCounterForce = 0f;
        }
        else {
            canJump = false;
            dragCounterForce += dragCounterForcePerSecond * Time.deltaTime;
        }
    }

    void checkInteractables() {
        Collider[] containerColliders = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, 2.5f, containerRayMask);
        Collider[] climbableColliders = Physics.OverlapCapsule(transform.position, transform.position + transform.forward * 2.25f, 0.75f, climbableRayMask);

        foreach (var c in containerColliders) {
            keyPrompt = "E to pick up";

            if (Input.GetKeyDown(KeyCode.E)) {
                Transform w = c.gameObject.transform.GetChild(0);
                w.parent = transform.GetChild(0).transform.GetChild(0);

                for (int i = 0; i < 9; i++) {
                    if (transform.GetChild(0).transform.GetChild(0).transform.GetChild(i).gameObject.tag == "Empty Slot") {
                        Destroy(transform.GetChild(0).transform.GetChild(0).transform.GetChild(i).gameObject);
                        w.SetSiblingIndex(i);
                        if (transform.GetChild(0).transform.GetChild(0).transform.GetChild(inventoryScript.getInventorySlot() - 1).gameObject.tag == "Empty Slot") {
                            inventoryScript.setInventorySlot(i + 1);
                        }
                        Destroy(c.gameObject);
                        break;
                    }
                }
            }

            break;
        }

        if (containerColliders.Length == 0) {
            foreach (var c in climbableColliders) {
                keyPrompt = "E to climb";

                if (Input.GetKeyDown(KeyCode.E)) {
                    anim.Play("Idle", -1);

                    transform.parent = c.gameObject.transform;
                    snapToClimbable();
                    playerMovementState = MovementState.Climb;
                    keyPrompt = "";
                }

                break;
            }
        }

        if (containerColliders.Length == 0 && climbableColliders.Length == 0) {
            keyPrompt = "";
        }
    }

    void snapToClimbable() {
        float theta = Mathf.Atan2(transform.localPosition.z, transform.localPosition.x);
        float r = transform.parent.gameObject.GetComponent<Collider>().bounds.size.x + gameObject.GetComponent<Collider>().bounds.size.x + 1f;
        transform.localPosition = new Vector3(r * Mathf.Cos(theta), transform.localPosition.y, r * Mathf.Sin(theta));

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), rayDownLength, groundRayMask)) {
            transform.position += new Vector3(0f, rayDownLength, 0f);
        }

        climbDirectionModifier = 1;
        swapClimb = false;
    }

    void walkMovement() {
        float m = 1f;

        if (Input.GetKey(KeyCode.LeftShift)) {
            m = speedMultiplier;
        }

        if (Input.GetKey(KeyCode.W)) {
            movementX = forcePerSecond * Time.deltaTime * m;
        }
        else if (Input.GetKey(KeyCode.S)) {
            movementX = -1 * forcePerSecond * Time.deltaTime;
        }
        else {
            movementX = 0;
        }

        if (Input.GetKey(KeyCode.A)) {
            movementZ = -1 * forcePerSecond * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.D)) {
            movementZ = forcePerSecond * Time.deltaTime;
        }
        else {
            movementZ = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space) && canJump) {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

        rb.AddForce(transform.forward * movementX, ForceMode.VelocityChange);
        rb.AddForce(transform.right * movementZ, ForceMode.VelocityChange);

        if (transform.position.y <= -30) {
            reset();
        }
    }

    void checkSpeed() {
        if (Mathf.Sqrt((rb.velocity.x * rb.velocity.x) + (rb.velocity.y * rb.velocity.y) + (rb.velocity.z * rb.velocity.z)) < 1.5f && canJump && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) {
            rb.velocity = new Vector3(0f, 0f, 0f);
            rb.useGravity = false;
        }
        else {
            rb.useGravity = true;
        }

        if (rb.velocity.y < -1 * maxFallSpeed) {
            rb.velocity = new Vector3(rb.velocity.x, -1 * maxFallSpeed, rb.velocity.z);
        }
    }


    // Climb State Functions

    void climbMovement() {
        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * rayDownLength, Color.green);

        rb.useGravity = false;

        float theta = Mathf.Atan2(transform.localPosition.z, transform.localPosition.x);
        float r = transform.parent.gameObject.GetComponent<Collider>().bounds.size.x + gameObject.GetComponent<Collider>().bounds.size.x + 1f;

        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)) {
            if (Vector3.Dot(transform.forward, new Vector3(r * Mathf.Cos(theta), 0f, r * Mathf.Sin(theta))) > 0) {
                swapClimb = true;
            }
            else {
                swapClimb = false;
            }
        }

        if (Input.GetKey(KeyCode.W)) {
            if (swapClimb) {
                rb.velocity = new Vector3(0f, -1 * climbSpeedDown, 0f);
            }
            else if (transform.localPosition.y < 1) {
                rb.velocity = new Vector3(0f, climbSpeedUp, 0f);
            }
            else {
                rb.velocity = Vector3.zero;
            }
        }
        else if (Input.GetKey(KeyCode.S)) {
            if (!swapClimb) {
                rb.velocity = new Vector3(0f, -1 * climbSpeedDown, 0f);
            }
            else if (transform.localPosition.y < 1) {
                rb.velocity = new Vector3(0f, climbSpeedUp, 0f);
            }
            else {
                rb.velocity = Vector3.zero;
            }
        }
        else {
            rb.velocity = Vector3.zero;
        }


        if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) {
            if (Vector3.Dot(new Vector3(transform.forward.x, 0f, transform.forward.z), new Vector3(r * Mathf.Cos(theta), 0f, r * Mathf.Sin(theta))) > 0) {
                climbDirectionModifier = -1;
            }
            else {
                climbDirectionModifier = 1;
            }
        }

        if (Input.GetKey(KeyCode.A)) {
            theta -= (Mathf.PI / 180) * turnDegreesPerSecond * Time.deltaTime * climbDirectionModifier;
        }
        else if (Input.GetKey(KeyCode.D)) {
            theta += (Mathf.PI / 180) * turnDegreesPerSecond * Time.deltaTime * climbDirectionModifier;
        }

        transform.localPosition = new Vector3(r * Mathf.Cos(theta), transform.localPosition.y, r * Mathf.Sin(theta));

        if (Input.GetKeyDown(KeyCode.LeftShift) || Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), rayDownLength, groundRayMask)) {
            transform.parent = null;
            playerMovementState = MovementState.Walk;
            dragCounterForce = 0;
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            Vector3 directedJump = new Vector3(r * Mathf.Cos(theta), 0f, r * Mathf.Sin(theta));
            
            transform.parent = null;
            playerMovementState = MovementState.Walk;
            dragCounterForce = 0;

            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            rb.AddForce(directedJump * jumpForce * 0.5f, ForceMode.Impulse);
        }
    }


    // Getters

    public bool getEscapeDown() {
        return escapeDown;
    }
}
