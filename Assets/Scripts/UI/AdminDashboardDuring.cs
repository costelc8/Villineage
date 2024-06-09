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
        SetAllInputs();

        // Timescale
        timeScale.onEndEdit.AddListener((value) => SetTimescale(int.Parse(value)));
        timeScaleSlider.onValueChanged.AddListener((value) => SetTimescale((int)value));

        // Job Weights
        hunterWeight.onEndEdit.AddListener((value) => SimVars.VARS.CmdSetIntVariable(nameof(SimVars.hunterWeight), Mathf.Max(0, int.Parse(value))));
        gathererWeight.onEndEdit.AddListener((value) => SimVars.VARS.CmdSetIntVariable(nameof(SimVars.gathererWeight), Mathf.Max(0, int.Parse(value))));
        lumberjackWeight.onEndEdit.AddListener((value) => SimVars.VARS.CmdSetIntVariable(nameof(SimVars.lumberjackWeight), Mathf.Max(0, int.Parse(value))));
        builderWeight.onEndEdit.AddListener((value) => SimVars.VARS.CmdSetIntVariable(nameof(SimVars.builderWeight), Mathf.Max(0, int.Parse(value))));

        // Villagers
        startingVillagers.onEndEdit.AddListener((value) => SimVars.VARS.CmdSetIntVariable(nameof(SimVars.startingVillagers), Mathf.Max(1, int.Parse(value))));
        moveSpeed.onEndEdit.AddListener((value) => SimVars.VARS.CmdSetFloatVariable(nameof(SimVars.villagerMoveSpeed), Mathf.Max(1, float.Parse(value))));
        workSpeed.onEndEdit.AddListener((value) => SimVars.VARS.CmdSetFloatVariable(nameof(SimVars.villagerWorkSpeed), Mathf.Max(1, float.Parse(value))));
        carryCapacity.onEndEdit.AddListener((value) => SimVars.VARS.CmdSetIntVariable(nameof(SimVars.villagerCarryCapacity), Mathf.Max(1, int.Parse(value))));
        hungerRate.onEndEdit.AddListener((value) => SimVars.VARS.CmdSetFloatVariable(nameof(SimVars.villagerHungerRate), Mathf.Max(0, float.Parse(value))));
        vitalityPerFood.onEndEdit.AddListener((value) => SimVars.VARS.CmdSetFloatVariable(nameof(SimVars.vitalityPerFood), Mathf.Max(1, float.Parse(value))));
        spawnTime.onEndEdit.AddListener((value) => SimVars.VARS.CmdSetFloatVariable(nameof(SimVars.villagerSpawnTime), Mathf.Max(0, float.Parse(value))));
        spawnCost.onEndEdit.AddListener((value) => SimVars.VARS.CmdSetIntVariable(nameof(SimVars.villagerSpawnCost), Mathf.Max(0, int.Parse(value))));
    }

    public void SetAllInputs()
    {
        timeScale.text = SimVars.VARS.timeScale.ToString();
        timeScaleSlider.value = SimVars.VARS.timeScale;

        hunterWeight.text = SimVars.VARS.hunterWeight.ToString();
        gathererWeight.text = SimVars.VARS.gathererWeight.ToString();
        lumberjackWeight.text = SimVars.VARS.lumberjackWeight.ToString();
        builderWeight.text = SimVars.VARS.builderWeight.ToString();


        startingVillagers.text = SimVars.VARS.startingVillagers.ToString();
        moveSpeed.text = SimVars.VARS.villagerMoveSpeed.ToString();
        workSpeed.text = SimVars.VARS.villagerWorkSpeed.ToString();
        carryCapacity.text = SimVars.VARS.villagerCarryCapacity.ToString();
        hungerRate.text = SimVars.VARS.villagerHungerRate.ToString();
        vitalityPerFood.text = SimVars.VARS.vitalityPerFood.ToString();
        spawnTime.text = SimVars.VARS.villagerSpawnTime.ToString();
        spawnCost.text = SimVars.VARS.villagerSpawnCost.ToString();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            panel.SetActive(!panel.activeSelf);
            Selection.Selector.leftDashboardOpen = panel.activeSelf;
            if (panel.activeSelf) SetAllInputs();
            //Selection.Selector.mouseMode = panel.activeSelf ? MouseMode.None : MouseMode.Selecting;
        }
    }

    private void SetTimescale(int value)
    {
        value = Mathf.Clamp(value, 0, 10);
        SimVars.VARS.CmdSetFloatVariable(nameof(SimVars.timeScale), value);
        timeScale.text = value.ToString();
        timeScaleSlider.value = value;
    }
}
