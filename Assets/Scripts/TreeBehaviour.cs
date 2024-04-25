using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBehaviour : Resource
{
    public override ResourceType Harvest(float progressValue)
    {
        if (Progress(progressValue)) return ResourceType.Wood;
        else return ResourceType.None;
    }
}
