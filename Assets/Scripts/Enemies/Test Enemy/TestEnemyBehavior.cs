using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestEnemyBehavior : MonoBehaviour {

    // Pathfinding Instance Variables

    public float sightAngle;
    public float sightDistanceInner;
    public float sightDistanceOuter;
    public float memoryDistance;
    public float stationaryTurnMultiplier;

    private bool fixRotation;
    private bool sightLineGizmo;
    private Vector3 lastAgentVelocity;

    private Vector3 destination;
    private Quaternion finalRotation;
    private NavMeshAgent nma;

    
    // Attacking/Attacked Instance Variables

    public float attackZoneRadius;
    public float enemyAttackSpeedMultiplier;

    public int health;
    public Font healthFont;
    private int maxHealth;
    private bool drawHealth;

    public float weight;
    private float hitStunTime;
    private bool checkRayBelow;
    private float rayDownLength = 1.2f;

    private Animator anim;


    // Layer Masks

    private int groundRayMask = 1 << 8; // Inverted in Start()
    private int playerMask = 1 << 3;
    private int obstacleMask = 1 << 0;


    // Misc Instance Variables

    private Rigidbody rb;
    private Movement playerScript;
    private GameObject playerRef;

    public enum EnemyState {
        Searching,
        Targeting,
        Hit
    }

    public EnemyState currentEnemyState = EnemyState.Searching;
    public EnemyState stateToSwitchTo;


    void Start() {
        fixRotation = false;
        sightLineGizmo = false;
        checkRayBelow = false;
        maxHealth = health;
        lastAgentVelocity = Vector3.zero;

        groundRayMask = ~groundRayMask;

        playerScript = GameObject.FindWithTag("Player").GetComponent<Movement>();
        playerRef = GameObject.FindWithTag("Player");

        nma = gameObject.GetComponent<NavMeshAgent>();
        rb = gameObject.GetComponent<Rigidbody>();
        anim = transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Animator>();

        StartCoroutine(pathfindingRoutine());
    }


    void FixedUpdate() {
        if (fixRotation) { correctAngle(Vector3.Angle(transform.forward, (playerRef.transform.position - transform.position).normalized), (playerRef.transform.position - transform.position).normalized); }
    }


    void OnTriggerEnter(Collider col) {
        if (currentEnemyState != EnemyState.Hit && col.gameObject.tag == "Sword" && col.gameObject != transform.GetChild(0).GetChild(0).GetChild(0).gameObject) {
            SwordBehavior sb = col.gameObject.GetComponent<SwordBehavior>();

            int h = health - sb.getDamage();
            health = (h <= 0) ? 0 : h;

            if (health == 0) {
                transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<SwordBehavior>().dropWeapon(gameObject);
                Destroy(gameObject);
            }
            else {
                nma.ResetPath();
                nma.enabled = false;

                rb.isKinematic = false;
                rb.AddForce(
                    ((2.7f * col.gameObject.transform.forward) + Vector3.up) * sb.getWeight() * (1.8f / weight),
                    ForceMode.Impulse
                );
                hitStunTime = Mathf.Log(1f + (sb.getWeight() * (8f / weight) * 0.2f));

                currentEnemyState = EnemyState.Hit;
            }
        }
    }


    void OnDrawGizmos() {
        Gizmos.color = (currentEnemyState == EnemyState.Targeting) ? new Color (0, 0.5f, 0.2f, 1) : Color.red;
        Gizmos.DrawWireSphere(transform.position, sightDistanceOuter);
        Gizmos.color = (currentEnemyState == EnemyState.Targeting) ? Color.green : new Color(1, 0.6f, 0, 1);
        Gizmos.DrawWireSphere(transform.position, memoryDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightDistanceInner);

        Debug.DrawRay(transform.position, (Quaternion.Euler(0, -1 * sightAngle, 0) * transform.forward).normalized * sightDistanceOuter, Color.yellow);
        Debug.DrawRay(transform.position, (Quaternion.Euler(0, sightAngle, 0) * transform.forward).normalized * sightDistanceOuter, Color.yellow);
        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * rayDownLength, Color.green);
        
        if (sightLineGizmo) { Debug.DrawRay(transform.position, playerRef.transform.position - transform.position, Color.cyan); }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position + (transform.forward * attackZoneRadius), attackZoneRadius);
    }


    void OnGUI() {
        if (drawHealth) {
            Vector3 relativePos = GameObject.FindWithTag("MainCamera").GetComponent<Camera>().WorldToScreenPoint(transform.position + (Vector3.up * 2));
            GUIStyle healthStyle = new GUIStyle();

            healthStyle.font = healthFont;
            healthStyle.fontSize = 64;
            healthStyle.fontStyle = FontStyle.Bold;
            healthStyle.alignment = TextAnchor.MiddleCenter;
            
            if (Vector3.Dot(playerRef.transform.position - transform.position, playerRef.transform.forward) < 0f) { GUI.Label(new Rect(relativePos.x, Screen.height - relativePos.y - 50, 64, 64), "HP: " + health.ToString() + " / " + maxHealth.ToString(), healthStyle); }
        }
    }


    private IEnumerator pathfindingRoutine() {
        float waitTime = 0.1f;
        WaitForSeconds wait = new WaitForSeconds(waitTime);

        while (true) {
            yield return wait;

            Debug.Log(lastAgentVelocity); // Needs fixing

            if (!playerScript.getEscapeDown()) {
                if (currentEnemyState != EnemyState.Hit) { 
                    nma.isStopped = false;
                }
                destination = playerRef.transform.position;

                if (currentEnemyState == EnemyState.Targeting && Vector3.Distance(transform.position, destination) <= memoryDistance) {
                    nma.SetDestination(destination);
                    if (lastAgentVelocity != Vector3.zero) { 
                        nma.velocity = lastAgentVelocity;
                        lastAgentVelocity = Vector3.zero;
                    }
                    sightLineGizmo = false;

                    if (nma.velocity.magnitude <= 1f && Vector3.Angle(transform.forward, (playerRef.transform.position - transform.position).normalized) >= 10f) {
                        fixRotation = true;
                    }
                    else {
                        fixRotation = false;
                    }

                    if (transform.GetChild(0).transform.GetChild(0).tag != "Empty Slot" && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
                        anim.SetFloat("windupSpeed", transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<SwordBehavior>().attackSpeedMultiplier * enemyAttackSpeedMultiplier);
                        anim.SetFloat("returnSpeed", Mathf.Log(1.5f + (2f * transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<SwordBehavior>().attackSpeedMultiplier * enemyAttackSpeedMultiplier)));
                        searchForPlayerInAttackZone();
                    }
                }
                else if (currentEnemyState == EnemyState.Hit) {
                    drawHealth = true;
                    hitStunTime -= waitTime;

                    if (checkRayBelow && Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), rayDownLength, groundRayMask)) {
                        checkRayBelow = false;
                        currentEnemyState = stateToSwitchTo;
                        rb.isKinematic = true;
                        nma.enabled = true;
                    }
                }
                else {
                    currentEnemyState = EnemyState.Searching;
                    nma.SetDestination(transform.position);
                }

                searchForPlayer();
            }
            else {
                if (currentEnemyState != EnemyState.Hit) {
                    lastAgentVelocity = nma.velocity;
                    nma.velocity = Vector3.zero;
                    nma.isStopped = true;
                }
            }
        }
    }


    void searchForPlayer() {
        Collider[] playerCollidersOuter = Physics.OverlapSphere(transform.position, sightDistanceOuter, playerMask);
        Collider[] playerCollidersInner = Physics.OverlapSphere(transform.position, sightDistanceInner, playerMask);

        if (playerCollidersOuter.Length != 0) {
            Transform target = playerCollidersOuter[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            sightLineGizmo = true;

            if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask)) {
                drawHealth = true;

                if (currentEnemyState == EnemyState.Searching && (Vector3.Angle(transform.forward, directionToTarget) < sightAngle || playerCollidersInner.Length != 0)) {
                    currentEnemyState = EnemyState.Targeting;
                }
                else if (currentEnemyState == EnemyState.Hit && hitStunTime <= 0f) {
                    checkRayBelow = true;
                    stateToSwitchTo = EnemyState.Targeting;
                }
            }
            else { drawHealth = false; }
        }
        else { 
            sightLineGizmo = false;
            drawHealth = false;
            if (currentEnemyState == EnemyState.Hit && hitStunTime <= 0f) {
                checkRayBelow = true;
                stateToSwitchTo = EnemyState.Searching;
            }
        }
    }


    void searchForPlayerInAttackZone() {
        Collider[] playerColliders = Physics.OverlapSphere(transform.position + (transform.forward * attackZoneRadius), attackZoneRadius, playerMask);
        if (playerColliders.Length != 0) {
            Transform target = playerColliders[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask)) {
                anim.SetTrigger("swing");
            }
        }
    }


    void correctAngle(float a, Vector3 d) {
        Vector3 newRotation = Vector3.RotateTowards(transform.forward, d, stationaryTurnMultiplier * a * Time.deltaTime * (Mathf.PI / 180), 0f).normalized;
        transform.rotation = Quaternion.LookRotation(newRotation, Vector3.up);
        Vector3 newEulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
        finalRotation.eulerAngles = newEulerAngles;
        transform.rotation = finalRotation;
    }
}
