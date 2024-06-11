using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Selection : MonoBehaviour
{
    public static Selection Selector;

    public GameObject selectionBox;
    public ManyResourceDisplay manyResourceDisplay;
    public GameObject manyVillagerPanel;
    public GameObject villagerButtonPrefab;

    private Vector2 selectionStart;
    private Vector2 selectionEnd;
    private List<ISelectable> selectables = new List<ISelectable>();
    public List<ISelectable> selected = new List<ISelectable>();
    public List<ISelectable> selectedVillagers = new List<ISelectable>();
    public List<ISelectable> selectedResources = new List<ISelectable>();
    public List<ISelectable> selectedStorages = new List<ISelectable>();
    public List<ISelectable> selectedBuildings = new List<ISelectable>();
    public List<ISelectable> selectedCarts = new List<ISelectable>();

    public MouseMode mouseMode;
    public Villager spotlightedVillager;
    public VillagerCam villagerCam;
    public RawImage villagerCamDisplay;
    public VillagerDisplay spotlightedVillagerDisplay;
    private List<GameObject> villagerButtons = new List<GameObject>();
    public bool leftDashboardOpen;
    public bool rightDashboardOpen;

    private const float leftPanelBound = 500f / 1600f;
    private const float rightPanelBound = 1050f / 1600f;

    private void Awake()
    {
        if (Selector == null || Selector == this) Selector = this;
        else Destroy(this);
        mouseMode = MouseMode.Selecting;
    }

    // The following two functions are currently how selectable objects are tracked.
    // Alternatively, we could use FindGameObjectsWithTag every time we make a selection.
    // Eg. selectables = GameObject.FindGameObjectsWithTag("Selectable");
    // I am genuinely unsure of which solution is better.

    public void AddSelectable(ISelectable selectable)
    {
        if (!selectables.Contains(selectable)) selectables.Add(selectable);
    }

    public void RemoveSelectable(ISelectable selectable)
    {
        selectables.Remove(selectable);
        selected.Remove(selectable);
        selectedVillagers.Remove(selectable);
        selectedResources.Remove(selectable);
        selectedStorages.Remove(selectable);
        selectedBuildings.Remove(selectable);
        selectedCarts.Remove(selectable);
    }

    private bool MouseInRange()
    {
        return Input.mousePosition.x > (leftDashboardOpen ? Screen.width * leftPanelBound : 0)
            && Input.mousePosition.x < (rightDashboardOpen ? Screen.width * rightPanelBound : Screen.width);
    }

    public Vector2 ScreenToCanvasSpace(Vector2 position)
    {
        position *= 1600f / Screen.width;
        return position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!(NetworkServer.active || NetworkClient.active)) return;
        if (mouseMode == MouseMode.Selecting)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (MouseInRange())
                {
                    // On mouse down, track selection start position, and enable selection box
                    selectionStart = Input.mousePosition;
                    selectionBox.SetActive(true);
                }
            }
            if (Input.GetMouseButton(0))
            {
                // On mouse hold, move and resize selection box accordingly
                selectionEnd = Input.mousePosition;
                selectionBox.GetComponent<RectTransform>().anchoredPosition = ScreenToCanvasSpace((selectionStart + selectionEnd) / 2f);
                selectionBox.GetComponent<RectTransform>().sizeDelta = ScreenToCanvasSpace(new Vector2(Mathf.Abs(selectionEnd.x - selectionStart.x), Mathf.Abs(selectionEnd.y - selectionStart.y)));
            }
            if (Input.GetMouseButtonUp(0))
            {
                // On mouse up, track selection end position and disable selection box
                selectionEnd = Input.mousePosition;
                selectionBox.SetActive(false);
                
                if (MouseInRange())
                {
                    // Clear the previously selected units, then make new selection
                    DeselectAll();
                    if (selectionStart == selectionEnd) SelectSingle(Input.mousePosition);
                    else SelectMany();
                }
            }
        }
        if (!leftDashboardOpen)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectAllVillagers(VillagerJob.Hunter);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SelectAllVillagers(VillagerJob.Gatherer);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SelectAllVillagers(VillagerJob.Lumberjack);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SelectAllVillagers(VillagerJob.Builder);
            if (Input.GetKeyDown(KeyCode.Alpha5)) SelectAllVillagers();
            if (Input.GetKeyDown(KeyCode.Alpha6)) SelectAllCarts();
            if (Input.GetKeyDown(KeyCode.Alpha7)) SelectAllStorages();
            if (Input.GetKeyDown(KeyCode.Alpha8)) SelectAllBuildings();
            if (Input.GetKeyDown(KeyCode.Alpha9)) SelectAllResources();
            if (Input.GetKeyDown(KeyCode.Alpha0)) SelectEverything();
        }
        //if (Input.GetKey(KeyCode.Escape))
        //{
        //    selectionBox.SetActive(false);
        //    DeselectAll();
        //    mouseMode = MouseMode.Selecting;
        //}
    }

    private void DeselectAll()
    {
        foreach (ISelectable selectable in selected)
        {
            selectable.OnDeselect();
            if (selectable is Resource resource) resource.RemoveHUD();
        }
        selected.Clear();
        selectedVillagers.Clear();
        selectedResources.Clear();
        selectedStorages.Clear();
        selectedBuildings.Clear();
        selectedCarts.Clear();
        if (spotlightedVillager != null) spotlightedVillager.RemoveHUD();
        foreach (GameObject button in villagerButtons) Destroy(button);
        villagerButtons.Clear();
        manyVillagerPanel.SetActive(false);
        spotlightedVillager = null;
        spotlightedVillagerDisplay.villager = null;
        villagerCamDisplay.enabled = false;
        rightDashboardOpen = false;
    }

    // Single-click selection, perform raycast
    private void SelectSingle(Vector3 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit[] hits = Physics.RaycastAll(ray, float.PositiveInfinity, ~0, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit hit in hits)
        {
            ISelectable[] select = hit.collider.GetComponents<ISelectable>();
            bool foundSelectable = false;
            foreach (ISelectable selectable in select)
            {
                if (!selected.Contains(selectable))
                {
                    selected.Add(selectable);
                    selectable.OnSelect();
                    if (selectable is Villager villager) SpotlightVillager(villager);
                    if (selectable is Resource resource) resource.DisplayHUD();
                    foundSelectable = true;
                }
            }
            if (foundSelectable) break;
        }
    }

    // Click-and-drag selection, check selection bounds
    private void SelectMany()
    {
        float lowerX, lowerY, upperX, upperY;
        if (selectionStart.x < selectionEnd.x)
        {
            lowerX = selectionStart.x;
            upperX = selectionEnd.x;
        }
        else
        {
            lowerX = selectionEnd.x;
            upperX = selectionStart.x;
        }
        if (selectionStart.y < selectionEnd.y)
        {
            lowerY = selectionStart.y;
            upperY = selectionEnd.y;
        }
        else
        {
            lowerY = selectionEnd.y;
            upperY = selectionStart.y;
        }
        foreach (ISelectable selectable in selectables)
        {
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(selectable.gameObject.transform.position);
            if (screenPosition.x > lowerX && screenPosition.x < upperX && screenPosition.y > lowerY && screenPosition.y < upperY)
            {
                selected.Add(selectable);
                if (selectable is Villager)
                {
                    selectedVillagers.Add(selectable);
                    selectable.OnSelect();
                }
                else if (selectable is Resource)
                {
                    selectedResources.Add(selectable);
                    selectable.OnSelect();
                }
                else if (selectable is Storage)
                {
                    selectedStorages.Add(selectable);
                    selectable.OnSelect();
                }
                else if (selectable is Building)
                {
                    selectedBuildings.Add(selectable);
                    selectable.OnSelect();
                }
                else if (selectable is Cart)
                {
                    selectedCarts.Add(selectable);
                    selectable.OnSelect();
                }
            }
        }
        if (selectedResources.Count == 1) ((Resource)selectedResources[0]).DisplayHUD();
        if (selectedVillagers.Count == 1) SpotlightVillager((Villager)selectedVillagers[0]);
        else if (selectedVillagers.Count > 1) DisplayVillagerPanel();
        //SelectSingle((selectionStart + selectionEnd) / 2);
    }

    private void DisplayVillagerPanel()
    {
        int i = 0;
        manyVillagerPanel.SetActive(true);
        rightDashboardOpen = true;
        foreach (Villager villager in selectedVillagers.Cast<Villager>())
        {
            if (i >= 50) break;
            GameObject button = Instantiate(villagerButtonPrefab, manyVillagerPanel.transform);
            villagerButtons.Add(button);
            button.GetComponent<RectTransform>().anchoredPosition = new Vector3(10 + (50 * (i % 10)), -(10 + (50 * (i / 10))));
            switch (villager.job)
            {
                case VillagerJob.Hunter: button.GetComponent<Image>().color = new Color(1f, 0.5f, 0.5f); break;
                case VillagerJob.Gatherer: button.GetComponent<Image>().color = new Color(1f, 1f, 0.5f); break;
                case VillagerJob.Lumberjack: button.GetComponent<Image>().color = new Color(0.5f, 1f, 0.5f); break;
                case VillagerJob.Builder: button.GetComponent<Image>().color = new Color(0.5f, 1f, 1f); break;
            }
            button.GetComponent<Button>().onClick.AddListener(() => {
                if (villager != null)
                {
                    SpotlightVillager(villager);
                    button.GetComponent<UnityEngine.UI.Outline>().enabled = true;
                }
            });
            i++;
            button.GetComponentInChildren<TextMeshProUGUI>().text = i.ToString();
        }
    }

    private void SpotlightVillager(Villager villager)
    {
        if (spotlightedVillager != null) spotlightedVillager.RemoveHUD();
        spotlightedVillager = villager;
        spotlightedVillagerDisplay.villager = villager;
        villagerCam.villager = villager;
        villagerCam.transform.position = villager.transform.position;
        villagerCam.transform.rotation = villager.transform.rotation;
        villagerCamDisplay.enabled = true;
        spotlightedVillager.DisplayHUD();
        foreach (GameObject button in villagerButtons)
        {
            button.GetComponent<UnityEngine.UI.Outline>().enabled = false;
        }
    }

    public void FocusCamOnVillager()
    {
        if (spotlightedVillager != null)
        {
            CameraControl.focusedVillager = spotlightedVillager;
        }
    }

    public void SelectAllVillagers()
    {
        DeselectAll();
        foreach (ISelectable selectable in selectables)
        {
            if (selectable is Villager villager && villager.alive)
            {
                selected.Add(selectable);
                selectedVillagers.Add(selectable);
                selectable.OnSelect();
            }
        }
        if (selectedVillagers.Count > 0) DisplayVillagerPanel();
    }

    public void SelectAllResources()
    {
        DeselectAll();
        foreach (ISelectable selectable in selectables)
        {
            if (selectable is Resource)
            {
                selected.Add(selectable);
                selectedResources.Add(selectable);
                selectable.OnSelect();
            }
        }
    }

    public void SelectAllStorages()
    {
        DeselectAll();
        foreach (ISelectable selectable in selectables)
        {
            if (selectable is Storage)
            {
                selected.Add(selectable);
                selectedResources.Add(selectable);
                selectable.OnSelect();
            }
        }
    }

    public void SelectAllBuildings()
    {
        DeselectAll();
        foreach (ISelectable selectable in selectables)
        {
            if (selectable is Building)
            {
                selected.Add(selectable);
                selectedBuildings.Add(selectable);
                selectable.OnSelect();
            }
        }
    }

    public void SelectAllCarts()
    {
        DeselectAll();
        foreach (ISelectable selectable in selectables)
        {
            if (selectable is Cart)
            {
                selected.Add(selectable);
                selectedCarts.Add(selectable);
                selectable.OnSelect();
            }
        }
    }

    public void SelectAllVillagers(VillagerJob job)
    {
        DeselectAll();
        foreach (ISelectable selectable in selectables)
        {
            if (selectable is Villager villager && villager.job == job && villager.alive)
            {
                selected.Add(selectable);
                selectedVillagers.Add(selectable);
                selectable.OnSelect();
            }
        }
        if (selectedVillagers.Count > 0) DisplayVillagerPanel();
    }

    public void SelectAllVillagers(int job)
    {
        DeselectAll();
        if (job >= (int)VillagerJob.MAX_VALUE) return;
        foreach (ISelectable selectable in selectables)
        {
            if (selectable is Villager villager && villager.job == (VillagerJob)job && villager.alive)
            {
                selected.Add(selectable);
                selectedVillagers.Add(selectable);
                selectable.OnSelect();
            }
        }
        if (selectedVillagers.Count > 0) DisplayVillagerPanel();
    }

    public void SelectEverything()
    {
        DeselectAll();
        foreach (ISelectable selectable in selectables)
        {
            selected.Add(selectable);
            if (selectable is Villager)
            {
                selectedVillagers.Add(selectable);
                selectable.OnSelect();
            }
            if (selectable is Resource)
            {
                selectedResources.Add(selectable);
                selectable.OnSelect();
            }
            if (selectable is Storage)
            {
                selectedStorages.Add(selectable);
                selectable.OnSelect();
            }
            else if (selectable is Building)
            {
                selectedBuildings.Add(selectable);
                selectable.OnSelect();
            }
            else if (selectable is Cart)
            {
                selectedCarts.Add(selectable);
                selectable.OnSelect();
            }
        }
        if (selectedVillagers.Count > 0) DisplayVillagerPanel();
    }
}

public enum MouseMode
{
    None,
    Selecting,
    Drawing,
}
