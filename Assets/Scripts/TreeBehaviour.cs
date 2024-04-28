using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBehaviour : Resource
{
    public int selected = 0;
    public int selectionLimit = 1;
    // Start is called before the first frame update
    void Start()
    {
        quantity = 1;
        durability = 5;
        maxDurability = 5;
    }

    public override ResourceType Harvest(float progressValue)
    {
        if (Progress(progressValue)){
            ResourceGenerator.UpdateList(this.transform.parent.gameObject);
            return ResourceType.Wood;
        } 
        return ResourceType.None;
    }
}
