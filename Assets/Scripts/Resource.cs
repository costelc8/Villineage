using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public int quantity;
    public float durability;
    public float maxDurability;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Decrement durability of the resource by the amount of progress made on it.
    // If the durability reaches 0, yield a resource, and reset the durability if
    // there is more resource to be harvested.
    protected bool Progress(float progressValue)
    {
        durability -= progressValue;
        if (durability <= 0)
        {
            quantity--;
            durability = maxDurability;
            if (quantity <= 0) Destroy(gameObject);
            return true;
        }
        return false;
    }

    public virtual ResourceType Harvest(float progressValue)
    {
        return ResourceType.None;
    }
}
