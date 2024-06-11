using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerCam : MonoBehaviour
{
    public Villager villager;

    private void LateUpdate()
    {
        if (villager != null && villager.alive)
        {
            transform.position = villager.transform.position;
            transform.rotation = villager.transform.rotation;
        }
    }
}
