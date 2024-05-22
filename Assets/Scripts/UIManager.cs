using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIManager : MonoBehaviour
{
    public SimVars simVars;
    public Button addResourcesButton;
    public Button decResourcesButton;

    public Button incHungerButton;
    public Button decHungerButton;

    void Start()
    {
        // Assign button click listener
        addResourcesButton.onClick.AddListener(AddResources);
        decResourcesButton.onClick.AddListener(DecResources);
        incHungerButton.onClick.AddListener(AddHunger);
        decHungerButton.onClick.AddListener(DecHunger);
    }

    public void AddResources()
    {
        if (simVars != null)
        {
            simVars.AddResources(1, 1, 10, 10);
        }
    }

    public void DecResources()
    {
        if (simVars != null)
        {
            simVars.DecResources(1, 1, 10, 10); 
        }
    }

    public void DecHunger()
    {
        if (simVars != null)
        {
            simVars.DecHunger((float)0.1);
        }
    }

    public void AddHunger()
    {
        if (simVars != null)
        {
            simVars.AddHunger((float)0.1);
        }
    }
}
