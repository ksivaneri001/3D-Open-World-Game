using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

    public float rotationSpeed;

    private float newXRotation = 0;
    private float newYRotation = 0;
    
    private Vector3 currentEulerAngles;
    private Vector3 currentParentEulerAngles;

    private Quaternion currentRotation;
    private Quaternion currentParentRotation;

    private Movement parentScript;

    void Start() {
        parentScript = transform.parent.GetComponent<Movement>();
    }

    void Update() {
        if (!parentScript.getEscapeDown()) {
            newXRotation = 0;
            newYRotation = 0;

            newXRotation += rotationSpeed * Input.GetAxis("Mouse Y");
            newYRotation += rotationSpeed * Input.GetAxis("Mouse X");

            currentParentEulerAngles += new Vector3(0f, newYRotation, 0f);
            currentParentRotation.eulerAngles = currentParentEulerAngles;
            transform.parent.transform.rotation = currentParentRotation;

            currentEulerAngles += ((transform.eulerAngles.x < 90 && Vector3.Dot(transform.up, Vector3.down) > 0 && newXRotation < 0) || (transform.eulerAngles.x > 270 && Vector3.Dot(transform.up, Vector3.down) > 0 && newXRotation > 0))
            ? new Vector3(0f, newYRotation, 0f)
            : new Vector3(-1 * newXRotation, newYRotation, 0f);
            currentRotation.eulerAngles = currentEulerAngles;
            transform.rotation = currentRotation;
        }
    }
}
