using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float movementSpeed;
    public float rotationSpeed;
    public float zoomSpeed;
    public float zoomDistance;
    public float minZoomDistance;
    public float maxZoomDistance;

    // Start is called before the first frame update
    void Start()
    {
        // Ensure camera is within zoom bounds on start
        zoomDistance = Mathf.Clamp(-Camera.main.transform.localPosition.z, minZoomDistance, maxZoomDistance);
        Camera.main.transform.localPosition = new Vector3(0f, 0f, -zoomDistance);
    }

    // Update is called once per frame
    void Update()
    {
        // Camera Movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        transform.Translate(new Vector3(horizontal, 0f, vertical) * (Time.deltaTime * movementSpeed));

        // Camera Rotation
        if (Input.GetMouseButtonDown(1)) Cursor.lockState = CursorLockMode.Locked;
        if (Input.GetMouseButton(1))
        {
            float mouseX = Mathf.Clamp(Input.GetAxis("Mouse X"), -1f, 1f);
            transform.Rotate(Vector3.up, mouseX * rotationSpeed);
        }
        if (Input.GetMouseButtonUp(1)) Cursor.lockState = CursorLockMode.None;

        if (Input.GetMouseButton(2))
        {
            float mouseX = Mathf.Clamp(Input.GetAxis("Mouse X"), -1f, 1f);
            float mouseY = Mathf.Clamp(Input.GetAxis("Mouse Y"), -1f, 1f);
            transform.Translate(new Vector3(mouseX, 0f, mouseY) * movementSpeed / -10f);
        }

        // Camera Zoom
        if (Input.mouseScrollDelta.y != 0f)
        {
            zoomDistance = Mathf.Clamp(zoomDistance - (Input.mouseScrollDelta.y * zoomSpeed), minZoomDistance, maxZoomDistance);
            Camera.main.transform.localPosition = new Vector3(0f, 0f, -zoomDistance);
        }
    }
}
