using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCommander : MonoBehaviour
{
    public RandomNavmeshPoint manager;
    public float selectionRange = 10.0f;
    public Vector3 result;
    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.FindWithTag("Manager").GetComponent<RandomNavmeshPoint>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            manager.RandomPoint(Vector3.zero, selectionRange, out result);
        }
    }
}
