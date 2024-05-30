using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class AdminDashboardDuring : MonoBehaviour
{
    public SimVars simVars;

    // Villager Variables
    public TMP_InputField startVillagers;
    public TMP_InputField spawnT;
    public TMP_InputField vitalityPerF;
    public TMP_InputField carryCap;
    public TMP_InputField moveS;
    public TMP_InputField workS;
    public TMP_InputField hungerRate;

    // Job Weights
    public TMP_InputField GathererW;
    public TMP_InputField LumberW;
    public TMP_InputField HunterW;
    public TMP_InputField BuilderW;

    // TimeScale, Seed, LogSim
    public TMP_InputField timeScale;

    void Start()
    {

        // Villager Text entry
        startVillagers.onEndEdit.AddListener((value) => AddVillagers("startVillagers", value));
        spawnT.onEndEdit.AddListener((value) => AddVillagers("spawnT", value));
        vitalityPerF.onEndEdit.AddListener((value) => AddVillagers("vitality", value));
        carryCap.onEndEdit.AddListener((value) => AddVillagers("carryCap", value));
        moveS.onEndEdit.AddListener((value) => AddVillagers("moveS", value));
        workS.onEndEdit.AddListener((value) => AddVillagers("workS", value));
        
        // Job Weights Text Entry
        GathererW.onEndEdit.AddListener((value) => AddJob("GathererW", value));
        LumberW.onEndEdit.AddListener((value) => AddJob("LumberW", value));
        HunterW.onEndEdit.AddListener((value) => AddJob("HunterW", value));
        BuilderW.onEndEdit.AddListener((value) => AddJob("BuilderW", value));
        
        // Separate so hunger rate can be a float
        hungerRate.onEndEdit.AddListener(AddHunger);
    }

    public void AddVillagers(string villagerType, string value)
    {
        if (int.TryParse(value, out int intValue))
        {
            switch (villagerType)
            {
                case "startVillagers":
                    simVars.startingVillagers = intValue;
                    break;
                case "spawnT":
                    simVars.villagerSpawnTime = intValue;
                    break;
                case "vitality":
                    simVars.vitalityPerFood = intValue;
                    break;
                case "carryCap":
                    simVars.villagerCarryCapacity = intValue;
                    break;
                case "moveS":
                    simVars.villagerMoveSpeed = intValue;
                    break;
                case "workS":
                    simVars.villagerWorkSpeed = intValue;
                    break;
                default:
                    Debug.LogWarning("Something suspicious has happened");
                    break;
            }
        }
    }

    public void AddJob(string jobType, string value)
    {
        if (int.TryParse(value, out int intValue))
        {
            switch (jobType)
            {
                case "GathererW":
                    simVars.gathererWeight = intValue;
                    break;
                case "LumberW":
                    simVars.lumberjackWeight = intValue;
                    break;
                case "HunterW":
                    simVars.hunterWeight = intValue;
                    break;
                case "BuilderW":
                    simVars.builderWeight = intValue;
                    break;
                default:
                    Debug.LogWarning("Something suspicious has happened");
                    break;
            }
        }
    }

    public void AddTandS(string otherType, string value)
    {
        if (int.TryParse(value, out int intValue))
        {
            switch (otherType)
            {
                case "timeScale":
                    simVars.timeScale = intValue;
                    break;
                default:
                    Debug.LogWarning("Something suspicious has happened");
                    break;
            }
        }
    }
    
    public void AddHunger(string value)
    {
        if (float.TryParse(value, out float floatValue))
        {
            simVars.villagerHungerRate = floatValue;
        }
    }
}
