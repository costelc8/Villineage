using System.Collections;
using System.Collections.Generic;
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
        zoomDistance = Mathf.Clamp(-Camera.main.transform.localPosition.z, minZoomDistance, maxZoomDistance);
        Camera.main.transform.localPosition = new Vector3(0f, 0f, -zoomDistance);
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        transform.Translate(new Vector3(horizontal, 0f, vertical) * (Time.deltaTime * movementSpeed));
        if (Input.GetMouseButtonDown(1)) Cursor.lockState = CursorLockMode.Locked;
        if (Input.GetMouseButton(1))
        {
            float mouseX = Mathf.Clamp(Input.GetAxis("Mouse X"), -1f, 1f);
            transform.Rotate(Vector3.up, mouseX * rotationSpeed);
        }
        if (Input.GetMouseButtonUp(1)) Cursor.lockState = CursorLockMode.None;
        if (Input.mouseScrollDelta.y != 0f)
        {
            zoomDistance = Mathf.Clamp(zoomDistance - (Input.mouseScrollDelta.y * zoomSpeed), minZoomDistance, maxZoomDistance);
            Camera.main.transform.localPosition = new Vector3(0f, 0f, -zoomDistance);
        }
    }
}
