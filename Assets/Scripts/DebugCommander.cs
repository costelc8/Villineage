using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCommander : MonoBehaviour
{
    public float selectionRange = 10.0f;
    public Vector3 result;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            RandomNavmeshPoint.RandomPoint(Vector3.zero, selectionRange, out result);
        }
    }
}
