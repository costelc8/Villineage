using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBehaviour : Resource
{
    public Villager villager;

    public void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("InteractRange")) {
            villager = other.gameObject.GetComponentInParent<Villager>();
            villager.wood += 10;
            villager.working = false;
            Destroy(gameObject);
        }
    }

    public override ResourceType Harvest(float progressValue)
    {
        if (Progress(progressValue))
        {
            return ResourceType.Wood;
        }
        else return ResourceType.None;
    }
}
