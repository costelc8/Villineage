using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public int quantity = 50;
    public float maxDurability = 1;
    private float durability;
    public Villager assignedVillager;
    public ResourceType resourceType;

    // Start is called before the first frame update
    void Start()
    {
        durability = maxDurability;
    }

    // Decrement durability of the resource by the amount of progress made on it.
    // If the durability reaches 0, yield a resource, and reset the durability if
    // there is more resource to be harvested.
    public bool Progress(float progressValue)
    {
        durability -= progressValue;
        if (durability <= 0)
        {
            quantity--;
            durability = maxDurability;
            if (quantity <= 0)
            {
                ResourceGenerator.RemoveResource(this);
                Destroy(gameObject);
            }
            return true;
        }
        return false;
    }

    public virtual ResourceType Harvest(float progressValue)
    {
        if (Progress(progressValue)) return resourceType;
        return ResourceType.None;
    }
}
