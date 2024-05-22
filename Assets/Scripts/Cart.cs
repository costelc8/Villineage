using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Experimental.GraphView.Port;

public class Cart : NetworkBehaviour, ISelectable
{
    private NavMeshAgent agent;
    private Animator anim;
    public Storage hub;
    public Storage townCenter;

    public bool selected;  // Have they been selected?

    public readonly SyncList<int> inventory = new SyncList<int>();  // Their resources inventory
    public Targetable target;  // The object they are targeting for their role
    public int capacity;  // Maximum amount of resources it can carry

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();     
    }

    public void Start()
    {
        Selection.Selector.AddSelectable(this);
        if (isServer)
        {
            // If this is the server, initialize the agents.
            agent.enabled = true;
            for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++) inventory.Add(0);

            agent.speed = SimVars.VARS.villagerMoveSpeed * 2;
            agent.acceleration = agent.speed * 2;
            capacity = SimVars.VARS.villagerCarryCapacity * 10;

            

        }
    }

    // When selected, display ui
    public void OnSelect()
    {
        selected = true;
        //GameObject HUD = UnitHUD.HUD.AddUnitHUD(gameObject, UnitHUD.HUD.villagerHUD, 1f);
        //HUD.GetComponent<DisplayController>().villager = this;
    }

    // When deselected, stop displaying ui
    public void OnDeselect()
    {
        selected = false;
        //UnitHUD.HUD.RemoveUnitHUD(gameObject);
    }


}
