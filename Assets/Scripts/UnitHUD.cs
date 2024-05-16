using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitHUD : MonoBehaviour
{
    public static UnitHUD HUD;
    public GameObject villagerHUD;
    public GameObject storageHUD;
    public GameObject resourceHUD;
    public Dictionary<GameObject, GameObject> unitHUDs = new Dictionary<GameObject, GameObject>();
    public Dictionary<GameObject, float> hudOffsets = new Dictionary<GameObject, float>();

    private void Awake()
    {
        if (HUD == null || HUD == this) HUD = this;
        else Destroy(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var unit in unitHUDs.Keys)
        {
            unitHUDs[unit].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(unit.transform.position + new Vector3(0f, hudOffsets[unit], 0f));
        }
    }

    /// <summary>
    /// This function will create an instance of the given HUD prefab, and attach it to the given
    /// GameObject so that it follows that GameObject on the screen
    /// </summary>
    /// <param name="unit">The GameObject that the HUD should be attached to</param>
    /// <param name="hudPrefab">The HUD prefab that will be instantiated</param>
    /// <param name="offset">The offset of how far above/below the unit the HUD should appear</param>
    /// <returns>
    /// The instantiated HUD for the given unit, or null if the unit already has a HUD
    /// </returns>
    public GameObject AddUnitHUD(GameObject unit, GameObject hudPrefab, float offset)
    {
        if (!unitHUDs.ContainsKey(unit))
        {
            GameObject hud = Instantiate(hudPrefab, transform);
            unitHUDs.Add(unit, hud);
            hudOffsets.Add(unit, offset);
            return hud;
        }
        else
        {
            Debug.LogWarning("Attempted to add a unitHUD that already exists");
            return null;
        }
    }

    public void RemoveUnitHUD(GameObject unit)
    {
        if (unitHUDs.ContainsKey(unit))
        {
            Destroy(unitHUDs[unit]);
            unitHUDs.Remove(unit);
            hudOffsets.Remove(unit);
        }
    }
}
