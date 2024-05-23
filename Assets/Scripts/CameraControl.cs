using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float movementSpeed = 5;
    public float rotationSpeed = 5;
    public float zoomSpeed = 5;
    public float zoomDistance = 30;
    public float minZoomDistance = 10;
    public float maxZoomDistance = 100;

    // Start is called before the first frame update
    private void Awake()
    {
        // Ensure camera is within zoom bounds on start
        zoomDistance = Mathf.Clamp(-Camera.main.transform.localPosition.z, minZoomDistance, maxZoomDistance);
        Camera.main.transform.localPosition = new Vector3(0f, 0f, -zoomDistance);
    }

    // Update is called once per frame
    void Update()
    {
        // Camera Movement
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        transform.Translate(new Vector3(horizontal, 0f, vertical).normalized * (Time.deltaTime * movementSpeed * zoomDistance / 5f));

        // Camera Rotation
        if (Input.GetMouseButtonDown(1)) Cursor.lockState = CursorLockMode.Locked;
        if (Input.GetMouseButton(1))
        {
            float mouseX = Mathf.Clamp(Input.GetAxis("Mouse X"), -10f, 10f);
            transform.Rotate(Vector3.up, mouseX * rotationSpeed);
        }
        if (Input.GetMouseButtonUp(1)) Cursor.lockState = CursorLockMode.None;

        if (Input.GetMouseButton(2))
        {
            float mouseX = Mathf.Clamp(Input.GetAxis("Mouse X"), -10f, 10f);
            float mouseY = Mathf.Clamp(Input.GetAxis("Mouse Y"), -10f, 10f);
            transform.Translate(new Vector3(mouseX, 0f, mouseY) * (movementSpeed * zoomDistance / -100f));
        }

        // Camera Zoom
        if (Input.mouseScrollDelta.y != 0f)
        {
            zoomDistance = Mathf.Pow(10, Mathf.Log10(zoomDistance) - (Input.mouseScrollDelta.y * zoomSpeed / 100f));
            zoomDistance = Mathf.Clamp(zoomDistance, minZoomDistance, maxZoomDistance);
            Camera.main.transform.localPosition = new Vector3(0f, 0f, -zoomDistance);
        }
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, 0, SimVars.VARS.terrainSize);
        pos.z = Mathf.Clamp(pos.z, 0, SimVars.VARS.terrainSize);
        transform.position = pos;
    }
}
