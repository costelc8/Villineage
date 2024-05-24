using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Resource : Targetable, ISelectable
{
    [Header("Resource Settings")]
    private GameObject stage0;
    private GameObject stage1;
    private int stage;
    [SyncVar(hook = nameof(QuantityHook))]
    public int quantity;
    public int maxQuantity;
    public Villager assignedVillager;
    public ResourceType resourceType;
    public bool respawning;
    public bool isAnimal = false;

    void Awake()
    {
        stage0 = transform.GetChild(0).gameObject;
        stage1 = transform.GetChild(1).gameObject;
    }

    // Decrement durability of the resource by the amount of progress made on it.
    // If the durability reaches 0, yield a resource, and reset the durability if
    // there is more resource to be harvested.
    public override bool Progress(Villager villager)
    {
        quantity--;
        villager.totalResources++;
        villager.inventory[(int)resourceType]++;
        if (quantity <= 0)
        {
            ResourceGenerator.RemoveResource(this);
            Selection.Selector.RemoveSelectable(this);
            UnitHUD.HUD.RemoveUnitHUD(gameObject);
            UntargetAll(false);
            StartCoroutine(DestroyResource());
        }
        return true;
    }

    private void QuantityHook(int oldQuantity, int newQuantity)
    {
        if (newQuantity < oldQuantity && oldQuantity == maxQuantity)
        {
            priority *= 1.5f;
        }
        if (stage0 != null && stage1 != null)
        {
            if (resourceType == ResourceType.Wood && newQuantity < oldQuantity && oldQuantity == maxQuantity)
            {
                stage0.SetActive(false);
                stage1.SetActive(true);
            }
            else if (resourceType == ResourceType.Food && newQuantity < oldQuantity && newQuantity == 0)
            {
                stage0.SetActive(false);
                stage1.SetActive(true);
            }
        }
    }

    protected IEnumerator DestroyResource()
    {
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(transform.position.x, transform.position.y - 2.5f, transform.position.z);
        float duration = 2f;

        while (elapsedTime < duration && (resourceType != ResourceType.Food || isAnimal))
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (resourceType == ResourceType.Food && !isAnimal)
        {
            yield return new WaitForSeconds(SimVars.VARS.berryRespawnTime);
            SetBerriesActive();
        }
        else Destroy(gameObject);
    }

    public void OnSelect()
    {
        GameObject HUD = UnitHUD.HUD.AddUnitHUD(gameObject, UnitHUD.HUD.resourceHUD, 1f);
        HUD.GetComponent<ResourceDisplay>().resource = this;
    }

    // When deselected, stop displaying ui
    public void OnDeselect()
    {
        UnitHUD.HUD.RemoveUnitHUD(gameObject);
    }

    public void SetBerriesActive()
    {
        quantity = SimVars.VARS.foodPerBerry;
        maxQuantity = SimVars.VARS.foodPerBerry;
        priority = 0;
        stage1.SetActive(false);
        stage0.SetActive(true);
        ResourceGenerator.ReAddBerries(this);
        Selection.Selector.AddSelectable(this);   
    }

    private void OnDestroy()
    {
        ResourceGenerator.RemoveResource(this);
        Selection.Selector.RemoveSelectable(this);
        UnitHUD.HUD.RemoveUnitHUD(gameObject);
    }

    public void Start()
    {
        Selection.Selector.AddSelectable(this);
        maxQuantity = quantity;
    }
}