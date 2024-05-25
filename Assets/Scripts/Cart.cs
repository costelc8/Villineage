using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class Cart : NetworkBehaviour, ISelectable
{
    private NavMeshAgent agent;
    private Animator anim;
    public Storage hub;

    public bool selected;  // Have they been selected?

    public readonly SyncList<int> inventory = new SyncList<int>();  // Their resources inventory
    public Targetable target;  // The object they are targeting for their role

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        target = TownCenter.TC.GetComponent<Storage>();
    }

    public void Start()
    {
        Selection.Selector.AddSelectable(this);
        if (isServer)
        {
            // If this is the server, initialize the agents.
            agent.enabled = true;
            for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++) inventory.Add(0);

            agent.speed = SimVars.VARS.villagerMoveSpeed * 3;
            agent.acceleration = SimVars.VARS.villagerMoveSpeed * 16;
        }
    }

    public void Update()
    {
        if (!isServer) return;
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // arrived at hub
            Storage storage = target.GetComponent<Storage>();
            storage.Deposit(this);
            if (storage == hub)
            {
                // at outpost
                storage.Collect(this);

                // go to TC
                target = TownCenter.TC.GetComponent<Storage>();
            } 
            else
            {
                // at TC
                storage.Request(this, hub.requestedResources);

                // go to the outpost
                target = hub;
            }
            agent.SetDestination(target.transform.position + ((transform.position - target.transform.position).normalized * 2f));
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
