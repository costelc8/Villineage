using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : Targetable, ISelectable
{
    [Header("Resource Settings")]
    private GameObject stage0;
    private GameObject stage1;
    [SyncVar(hook = nameof(QuantityHook))]
    public int quantity;
    public float maxDurability = 1;
    private float durability;
    public Villager assignedVillager;
    public ResourceType resourceType;

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
            if (stage1 != null)
            {
                stage0.SetActive(false);
                stage1.SetActive(true);
            }
            if (quantity <= 0)
            {
                ResourceGenerator.RemoveResource(this);

                StartCoroutine(DestroyResource());
            }
            return true;
        }
        return false;
    }

    private void QuantityHook(int oldQuantity, int newQuantity)
    {
        if (newQuantity < oldQuantity && stage1 != null)
        {
            stage0.SetActive(false);
            stage1.SetActive(true);
        }
    }

	IEnumerator DestroyResource()
    {
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(transform.position.x, transform.position.y - 2.5f, transform.position.z);
        float duration = 2f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    public void OnSelect()
    {

    }
    public void OnDeselect()
    {

    }
    public void Start()
    {
        
    }
}