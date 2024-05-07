using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : Targetable
{
    private GameObject stage0;
    private GameObject stage1;
    [SyncVar(hook = nameof(QuantityHook))]
    public int quantity = 50;
    public float maxDurability = 1;
    private float durability;
    public Villager assignedVillager;
    public ResourceType resourceType;

    // Start is called before the first frame update
    void Awake()
    {
        stage0 = transform.GetChild(0).gameObject;
        stage1 = transform.GetChild(1).gameObject;
        durability = maxDurability;
    }

    // Decrement durability of the resource by the amount of progress made on it.
    // If the durability reaches 0, yield a resource, and reset the durability if
    // there is more resource to be harvested.
    public override bool Progress(Villager villager, float progressValue)
    {
        durability -= progressValue;
        while (durability <= 0)
        {
            quantity--;
            villager.totalResources++;
            villager.inventory[(int)resourceType]++;
            durability += maxDurability;
            stage0.SetActive(false);
            stage1.SetActive(true);
            if (quantity <= 0)
            {
                ResourceGenerator.RemoveResource(this);
                Destroy(gameObject);
            }
            return true;
        }
        return false;
    }

    private void QuantityHook(int oldQuantity, int newQuantity)
    {
        stage0.SetActive(false);
        stage1.SetActive(true);
    }
}
