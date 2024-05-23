using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selection : MonoBehaviour
{
    public static Selection Selector;

    public GameObject selectionBox;
    public ManyResourceDisplay manyResourceDisplay;

    private Vector2 selectionStart;
    private Vector2 selectionEnd;
    private List<ISelectable> selectables = new List<ISelectable>();
    public List<ISelectable> selected = new List<ISelectable>();
    public List<ISelectable> selectedVillagers = new List<ISelectable>();
    public List<ISelectable> selectedResources = new List<ISelectable>();
    public List<ISelectable> selectedStorages = new List<ISelectable>();

    private void Awake()
    {
        if (Selector == null || Selector == this) Selector = this;
        else Destroy(this);
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
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // On mouse down, track selection start position, and enable selection box
            selectionStart = Input.mousePosition;
            selectionBox.SetActive(true);
        }
        if (Input.GetMouseButton(0))
        {
            // On mouse hold, move and resize selection box accordingly
            selectionEnd = Input.mousePosition;
            selectionBox.GetComponent<RectTransform>().anchoredPosition = (selectionStart + selectionEnd) / 2f;
            selectionBox.GetComponent<RectTransform>().sizeDelta = new Vector2(Mathf.Abs(selectionEnd.x - selectionStart.x), Mathf.Abs(selectionEnd.y - selectionStart.y));
        }
        if (Input.GetMouseButtonUp(0))
        {
            // On mouse up, track selection end position and disable selection box
            selectionEnd = Input.mousePosition;
            selectionBox.SetActive(false);

            // Clear the previously selected units, then make new selection
            DeselectAll();
            if (selectionStart == selectionEnd) SelectSingle(Input.mousePosition);     
            else SelectMany();
        }
        //if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        //{
            if (Input.GetKeyDown(KeyCode.V)) SelectAllVillagers();
            if (Input.GetKeyDown(KeyCode.R)) SelectAllResources();
            if (Input.GetKeyDown(KeyCode.S)) SelectAllStorages();
            if (Input.GetKeyDown(KeyCode.L)) SelectAllVillagers(VillagerJob.Lumberjack);
            if (Input.GetKeyDown(KeyCode.B)) SelectAllVillagers(VillagerJob.Builder);
            if (Input.GetKeyDown(KeyCode.G)) SelectAllVillagers(VillagerJob.Gatherer);
            if (Input.GetKeyDown(KeyCode.H)) SelectAllVillagers(VillagerJob.Hunter);
        //}
    }

    private void DeselectAll()
    {
        foreach (ISelectable selectable in selected) selectable.OnDeselect();
        selected.Clear();
        selectedVillagers.Clear();
        selectedResources.Clear();
        selectedStorages.Clear();
    }

    // Single-click selection, perform raycast
    private void SelectSingle(Vector3 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit[] hits = Physics.RaycastAll(ray, float.PositiveInfinity, ~0, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit hit in hits)
        {
            ISelectable selectable = hit.collider.GetComponent<ISelectable>();
            if (selectable != null && !selected.Contains(selectable))
            {
                selected.Add(selectable);
                selectable.OnSelect();
                break;
            }
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
                else if (selectable is Resource) selectedResources.Add(selectable);
                else if (selectable is Storage)
                {
                    selectedStorages.Add(selectable);
                    selectable.OnSelect();
                }
            }
        }
        if (selectedResources.Count == 1) selectedResources[0].OnSelect();
        //SelectSingle((selectionStart + selectionEnd) / 2);
    }

    public void SelectAllVillagers()
    {
        DeselectAll();
        foreach (ISelectable selectable in selectables)
        {
            if (selectable is Villager)
            {
                selected.Add(selectable);
                selectedVillagers.Add(selectable);
                selectable.OnSelect();
            }
        }
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

    public void SelectAllVillagers(VillagerJob job)
    {
        DeselectAll();
        foreach (ISelectable selectable in selectables)
        {
            if (selectable is Villager villager && villager.job == job)
            {
                selected.Add(selectable);
                selectedVillagers.Add(selectable);
                selectable.OnSelect();
            }
        }
    }

    public void SelectEverything()
    {
        DeselectAll();
        foreach (ISelectable selectable in selectables) selectable.OnSelect();
    }
}
