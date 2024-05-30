using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class UIManager : MonoBehaviour
{
    public SimVars simVars;

    // Resource Variables
    public TMP_InputField sheep;
    public TMP_InputField goat;
    public TMP_InputField berryRespawn;
    public TMP_InputField sheepFood;
    public TMP_InputField goatFood;
    public TMP_InputField berryFood;
    public TMP_InputField wolves;
    public TMP_InputField woodTree;

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

    // Building Costs
    public TMP_InputField houseC;
    public TMP_InputField outpostC;
    public TMP_InputField villagerSpawnC;

    // TimeScale, Seed, LogSim
    public TMP_InputField timeScale;
    public TMP_InputField seed;
    public Toggle logSim;

    void Start()
    {
        // Resource Text Entry
        sheep.onEndEdit.AddListener((value) => AddResources("numSheep", value));
        goat.onEndEdit.AddListener((value) => AddResources("numGoat", value));
        berryRespawn.onEndEdit.AddListener((value) => AddResources("berryRespawn", value));
        sheepFood.onEndEdit.AddListener((value) => AddResources("sheepFood", value));
        goatFood.onEndEdit.AddListener((value) => AddResources("goatFood", value));
        berryFood.onEndEdit.AddListener((value) => AddResources("berryFood", value));
        wolves.onEndEdit.AddListener((value) => AddResources("numWolves", value));
        woodTree.onEndEdit.AddListener((value) => AddResources("woodTree", value));

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

        // Building Text Entry
        houseC.onEndEdit.AddListener((value) => AddBuilding("houseC", value));
        outpostC.onEndEdit.AddListener((value) => AddBuilding("outpostC", value));
        villagerSpawnC.onEndEdit.AddListener((value) => AddBuilding("villagerSpawnC", value));

        // TimeScale, seed
        timeScale.onEndEdit.AddListener((value) => AddTandS("timeScale", value));
        seed.onEndEdit.AddListener((value) => AddTandS("seed", value));
        logSim.onValueChanged.AddListener(AddToggle);
        
        // Separate so hunger rate can be a float
        hungerRate.onEndEdit.AddListener(AddHunger);
    }

    public void AddResources(string resourceType, string value)
    {
        if (int.TryParse(value, out int intValue))
        {
            switch (resourceType)
            {
                case "berryRespawn":
                    simVars.berryRespawnTime = intValue;
                    break;
                case "sheepFood":
                    simVars.foodPerSheep = intValue;
                    break;
                case "goatFood":
                    simVars.foodPerGoat = intValue;
                    break;
                case "berryFood":
                    simVars.foodPerBerry = intValue;
                    break;
                case "woodTree":
                    simVars.woodPerTree = intValue;
                    break;
                default:
                    Debug.LogWarning("Something suspicious has happened");
                    break;
            }
        }
        else
        {
            Debug.LogWarning("Invalid input");
        }
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
        else
        {
            Debug.LogWarning("Invalid input");
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
        else
        {
            Debug.LogWarning("Invalid input");
        }
    }

    public void AddBuilding(string buildingType, string value)
    {
        if (int.TryParse(value, out int intValue))
        {
            switch (buildingType)
            {
                case "houseC":
                    simVars.houseBuildCost = intValue;
                    break;
                case "outpostC":
                    simVars.outpostBuildCost = intValue;
                    break;
                case "villagerSpawnC":
                    simVars.villagerSpawnCost = intValue;
                    break;
                default:
                    Debug.LogWarning("Something suspicious has happened");
                    break;
            }
        }
        else
        {
            Debug.LogWarning("Invalid input");
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
                case "seed":
                    simVars.seed = intValue;
                    break;
                default:
                    Debug.LogWarning("Something suspicious has happened");
                    break;
            }
        }
        else
        {
            Debug.LogWarning("Invalid input");
        }
    }

    public void AddToggle(bool isOn)
    {
        simVars.logSim = isOn;
    }
    
    public void AddHunger(string value)
    {
        if (float.TryParse(value, out float floatValue))
        {
            simVars.villagerHungerRate = floatValue;
        }
        else
        {
            Debug.LogWarning("Invalid input");
        }
    }
}
