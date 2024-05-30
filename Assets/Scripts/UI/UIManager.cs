using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class UIManager : MonoBehaviour
{
    public SimVars simVars;

    // Resource Variables
    public Slider forestDenSlider;
    public TextMeshProUGUI forestDensityDisplay;
    public TextMeshProUGUI perlinDisplay;

    public Slider perlinForestSlider;
    public TMP_InputField berryRespawn;
    public TMP_InputField sheepFood;
    public TMP_InputField goatFood;
    public TMP_InputField berryFood;
    public TMP_InputField forestSpace;
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
        forestDenSlider.value = simVars.forestDensity;
        forestDensityDisplay.text = "Forest Density: " + simVars.forestDensity.ToString();
        perlinForestSlider.value = simVars.perlinForestThreshold;
        perlinDisplay.text = "Perlin Forest: " + simVars.perlinForestThreshold.ToString();
        forestDenSlider.onValueChanged.AddListener((value) => AddSliderResources("forestSlider", value));
        perlinForestSlider.onValueChanged.AddListener((value) => AddSliderResources("perlinSlider", value));
        berryRespawn.onEndEdit.AddListener((value) => AddResources("berryRespawn", value));
        sheepFood.onEndEdit.AddListener((value) => AddResources("sheepFood", value));
        goatFood.onEndEdit.AddListener((value) => AddResources("goatFood", value));
        berryFood.onEndEdit.AddListener((value) => AddResources("berryFood", value));
        forestSpace.onEndEdit.AddListener((value) => AddResources("forestSpace", value));
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

    public void AddSliderResources(string resourceType, float value)
    {
        switch(resourceType)
        {
            case "forestSlider":
                float roundedValue = Mathf.Round(value * 10) / 10f;
                simVars.forestDensity = roundedValue;
                forestDensityDisplay.text = "Forest Density: " + value.ToString("F1");
                break;
            case "perlinSlider":
                float roundedValueperlin = Mathf.Round(value * 10) / 10f;
                simVars.perlinForestThreshold = roundedValueperlin;
                perlinDisplay.text = "Forest Density: " + value.ToString("F1");
                break;
            default:
                Debug.LogWarning("Something suspicious has happened");
                break;
        }
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
                case "forestSpace":
                    simVars.forestSpacing = intValue;
                    break;
                default:
                    Debug.LogWarning("Something suspicious has happened");
                    break;
            }
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
    }
}
