using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class AdminDashboardDuring : MonoBehaviour
{
    public GameObject panel;

    // Timescale
    public TMP_InputField timeScale;
    public Slider timeScaleSlider;

    // Job Weights
    public TMP_InputField hunterWeight;
    public TMP_InputField gathererWeight;
    public TMP_InputField lumberjackWeight;
    public TMP_InputField builderWeight;

    // Villager Variables
    public TMP_InputField startingVillagers;
    public TMP_InputField moveSpeed;
    public TMP_InputField workSpeed;
    public TMP_InputField carryCapacity;
    public TMP_InputField hungerRate;
    public TMP_InputField vitalityPerFood;
    public TMP_InputField spawnTime;
    public TMP_InputField spawnCost;

    void Start()
    {
        // Timescale
        timeScale.text = SimVars.VARS.timeScale.ToString();
        timeScale.onEndEdit.AddListener((value) => SetTimescale(int.Parse(value)));
        timeScaleSlider.value = SimVars.VARS.timeScale;
        timeScaleSlider.onValueChanged.AddListener((value) => SetTimescale((int)value));

        // Job Weights
        hunterWeight.text = SimVars.VARS.hunterWeight.ToString();
        hunterWeight.onEndEdit.AddListener((value) => SimVars.VARS.hunterWeight = int.Parse(value));
        gathererWeight.text = SimVars.VARS.gathererWeight.ToString();
        gathererWeight.onEndEdit.AddListener((value) => SimVars.VARS.gathererWeight = int.Parse(value));
        lumberjackWeight.text = SimVars.VARS.lumberjackWeight.ToString();
        lumberjackWeight.onEndEdit.AddListener((value) => SimVars.VARS.lumberjackWeight = int.Parse(value));
        builderWeight.text = SimVars.VARS.builderWeight.ToString();
        builderWeight.onEndEdit.AddListener((value) => SimVars.VARS.builderWeight = int.Parse(value));

        // Villagers
        startingVillagers.text = SimVars.VARS.startingVillagers.ToString();
        startingVillagers.onEndEdit.AddListener((value) => SimVars.VARS.startingVillagers = int.Parse(value));
        moveSpeed.text = SimVars.VARS.villagerMoveSpeed.ToString();
        moveSpeed.onEndEdit.AddListener((value) => SimVars.VARS.villagerMoveSpeed = float.Parse(value));
        workSpeed.text = SimVars.VARS.villagerWorkSpeed.ToString();
        workSpeed.onEndEdit.AddListener((value) => SimVars.VARS.villagerWorkSpeed = float.Parse(value));
        carryCapacity.text = SimVars.VARS.villagerCarryCapacity.ToString();
        carryCapacity.onEndEdit.AddListener((value) => SimVars.VARS.villagerCarryCapacity = int.Parse(value));
        hungerRate.text = SimVars.VARS.villagerHungerRate.ToString();
        hungerRate.onEndEdit.AddListener((value) => SimVars.VARS.villagerHungerRate = float.Parse(value));
        vitalityPerFood.text = SimVars.VARS.vitalityPerFood.ToString();
        vitalityPerFood.onEndEdit.AddListener((value) => SimVars.VARS.vitalityPerFood = float.Parse(value));
        spawnTime.text = SimVars.VARS.villagerSpawnTime.ToString();
        spawnTime.onEndEdit.AddListener((value) => SimVars.VARS.villagerSpawnTime = float.Parse(value));
        spawnCost.text = SimVars.VARS.villagerSpawnCost.ToString();
        spawnCost.onEndEdit.AddListener((value) => SimVars.VARS.villagerSpawnCost = int.Parse(value));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            panel.SetActive(!panel.activeSelf);
            Selection.Selector.mouseMode = panel.activeSelf ? MouseMode.None : MouseMode.Selecting;
        }
    }

    private void SetTimescale(int value)
    {
        SimVars.VARS.timeScale = Mathf.Clamp(value, 1, 10);
        timeScale.text = SimVars.VARS.timeScale.ToString();
        timeScaleSlider.value = SimVars.VARS.timeScale;
    }
}
