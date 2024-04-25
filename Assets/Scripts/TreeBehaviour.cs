using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBehaviour : MonoBehaviour
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
}
