using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    private Transform house0;
    private Transform house1;
    private Transform house2;

    [Range(0, 2)]
    public int state = 0;
    // Start is called before the first frame update
    void Start()
    {
        house0 = transform.Find("House_0");
        house1 = transform.Find("House_1");
        house2 = transform.Find("House_2");
    }

    // Update is called once per frame
    void Update()
    {
        if (state == 0)
        {
            house0.gameObject.SetActive(true);
            house1.gameObject.SetActive(false);
            house2.gameObject.SetActive(false);
        }
        else if (state == 1)
        {
            house0.gameObject.SetActive(false);
            house1.gameObject.SetActive(true);
            house2.gameObject.SetActive(false);
        }
        else if (state == 2)
        {
            house0.gameObject.SetActive(false);
            house1.gameObject.SetActive(false);
            house2.gameObject.SetActive(true);
        }
       
    }
    
}
