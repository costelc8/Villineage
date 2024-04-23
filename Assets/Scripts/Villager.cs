using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Villager : MonoBehaviour, ISelectable
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSelect()
    {
        Debug.Log(name + " Selected");
        foreach(Transform t in transform) t.gameObject.SetActive(true);
    }

    public void OnDeselect()
    {
        Debug.Log(name + " Deselected");
        foreach (Transform t in transform) t.gameObject.SetActive(false);
    }
}
