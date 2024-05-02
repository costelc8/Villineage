using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    private Transform construction;
    private Transform finished;

    public bool built = false;
    // Start is called before the first frame update
    void Start()
    {
        construction = transform.Find("Construction");
        finished = transform.Find("Finished");
    }

    // Update is called once per frame
    void Update()
    {
        if (built)
        {
            construction.gameObject.SetActive(false);
            finished.gameObject.SetActive(true);
        } 
        else
        {
            construction.gameObject.SetActive(true);
            finished.gameObject.SetActive(false);
        }
    }
}
