using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
    public static Villager focusedVillager;

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
        transform.Translate(new Vector3(horizontal, 0f, vertical).normalized * (Time.unscaledDeltaTime * movementSpeed * zoomDistance / 5f));
        if (horizontal != 0 || vertical != 0) focusedVillager = null;

        // Camera Rotation
        if (Input.GetMouseButtonDown(1)) Cursor.lockState = CursorLockMode.Locked;
        if (Input.GetMouseButton(1))
        {
            float mouseX = Mathf.Clamp(Input.GetAxis("Mouse X"), -10f, 10f);
            transform.Rotate(Vector3.up, mouseX * rotationSpeed);
        }
        if (Input.GetMouseButtonUp(1)) Cursor.lockState = CursorLockMode.None;

        // Camera Movement
        if (Input.GetMouseButton(2))
        {
            float mouseX = Mathf.Clamp(Input.GetAxis("Mouse X"), -10f, 10f);
            float mouseY = Mathf.Clamp(Input.GetAxis("Mouse Y"), -10f, 10f);
            transform.Translate(new Vector3(mouseX, 0f, mouseY) * (movementSpeed * zoomDistance / -100f));
            focusedVillager = null;
        }

        // Camera Zoom
        if (Input.mouseScrollDelta.y != 0f)
        {
            float zoomLog = Mathf.Log10(zoomDistance) - (Input.mouseScrollDelta.y * zoomSpeed / 100f);
            zoomDistance = Mathf.Pow(10, zoomLog);
            zoomDistance = Mathf.Clamp(zoomDistance, minZoomDistance, maxZoomDistance);
            Camera.main.transform.localPosition = new Vector3(0f, 0f, -zoomDistance);
            Camera.main.transform.parent.localRotation = Quaternion.Euler(30f * Mathf.Clamp(zoomLog, 1f, 2f), 0f, 0f);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TownCenter.TC.CenterCamera();
        }

        if (focusedVillager != null)
        {
            transform.position = focusedVillager.transform.position;
        }

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, 0, SimVars.VARS.terrainSize);
        pos.y = SimVars.VARS.terrainDepth / 2;
        pos.z = Mathf.Clamp(pos.z, 0, SimVars.VARS.terrainSize);
        transform.position = pos;
    }
}
