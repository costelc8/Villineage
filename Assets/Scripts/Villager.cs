using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Villager : NetworkBehaviour, ISelectable
{
    private NavMeshAgent agent;  // Navigates the villager
    private Animator anim;  // Animates the villager
    public List<Storage> hubs;  // The different outposts/alternatives to town center
    public Storage hub; // Current parent hub
	public Outline outlineS;
    public GameObject arrowPrefab;

    [Header("Stats")]
    [SyncVar(hook = nameof(AliveHook))]
    public bool alive = true;  // Are they alive?
    public float vitality;  // How much hunger do they have left
    public float maxVitality = 100f;  // The highest their hunger value can be (full)
    public float vitalityThreshold;
    public float huntingRange = 10f;
    [SyncVar]
    public string causeOfDeath;
    public bool damaged;
    public float despawnTimer = 0f;

    [Header("Jobs")]
    [SyncVar(hook = nameof(JobHook))]
    public VillagerJob job;  // Their current role
    public readonly SyncList<int> inventory = new SyncList<int>();  // Their resources inventory
    public int totalResources = 0; // Total number of resources across all types
    public Targetable target;  // The object they are targeting for their role
    public float progressCooldown = 1;

    [Header("Movement")]
    public float selectionRange = 10.0f;  // Selection range for random movement
    private Vector3 previousPosition;

    [Header("States")]
    [SyncVar(hook = nameof(StateHook))]
    public VillagerState state;  // The state of the villager (see enum below)

    [Header("Tools")]  // For animation purposes
    public GameObject axe;
    public GameObject hammer;
    public GameObject bow;
    public GameObject quiver;
    public GameObject backWood;
    public GameObject backFood;
    public GameObject backOther;

    private void Awake()
    {
        // Get anim and agent objects and set up hunger variables
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        vitality = maxVitality;
        progressCooldown = 1;
		outlineS = gameObject.GetComponent<Outline>();
    }

    public void Start()
    {
        // Add to list of selectables
        Selection.Selector.AddSelectable(this);
        if (isServer)
        {
            // If this is the server, initialize the agents.
            agent.enabled = true;
            for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++) inventory.Add(0);
            agent.speed = SimVars.VARS.villagerMoveSpeed;
            agent.acceleration = SimVars.VARS.villagerMoveSpeed * 4;

            // set default hub to town center
            // if they're reparented to an outpost later, can change this
            hub = TownCenter.TC.GetComponent<Storage>();
            hub.villagers.Add(this);
            previousPosition = transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer) return;
        if (!alive)
        {
            if (SimVars.VARS.clearBodies)
            {
                despawnTimer += Time.deltaTime;
                if (despawnTimer >= 300f) Destroy(gameObject);
            }
            return;
        }
        if (state == VillagerState.Pending) // If no job/target is assigned
        {
            FindNewDestination();
            if (target == hub) ChangeState(VillagerState.Returning);
        }
        else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) // If the target has been reached
        {
            if (target == null) ChangeState(VillagerState.Pending); // If the target is gone, switch to pending
            else
            {
                if (state == VillagerState.Walking) ChangeState(VillagerState.Working); // If walking, start working
                if (state == VillagerState.Working) // If working, do appropriate work based on job
                {
                    progressCooldown -= SimVars.VARS.villagerWorkSpeed * Time.deltaTime;
                    if (progressCooldown <= 0)
                    {
                        if (job == VillagerJob.Builder && inventory[(int)ResourceType.Wood] > 0)
                        {
                            ((Building)target).ContributeWood(this);
                            DisableBackVisuals();
                        }
                        progressCooldown++;
                        bool progress = false;
                        if (job == VillagerJob.Hunter && target.movingTarget)
                        {
                            Arrow arrow = Instantiate(arrowPrefab).GetComponent<Arrow>();
                            NetworkServer.Spawn(arrow.gameObject);
                            arrow.hunter = this;
                            arrow.target = (Animal)target;
                        }
                        else progress = target.Progress(this);
                        if ((job == VillagerJob.Builder && !progress) || (job != VillagerJob.Builder && totalResources >= SimVars.VARS.villagerCarryCapacity)) ReturnToHub();
                        else if (job == VillagerJob.Hunter)
                        {
                            SetNewTarget(target);
                            Vector3 towards = target.transform.position - transform.position;
                            towards.y = 0;
                            transform.rotation = Quaternion.LookRotation(towards);
                        }
                    }
                }
                else if (state == VillagerState.Returning) // If returning, deposit resources
                {
                    hub.Deposit(this);
                    damaged = false;
                    if (job == VillagerJob.Builder)
                    {
                        hub.Request(this, ResourceType.Wood);
                    }
                    ChangeState(VillagerState.Pending);
                    DisableBackVisuals();
                }
            }
        }
        else // If the target has not been reached
        {
            if (target != null && target.movingTarget) SetNewTarget(target);
            if (state == VillagerState.Working) state = VillagerState.Walking;
        }

        // Decrease hunger:
        if (state == VillagerState.Working) vitality -= SimVars.VARS.villagerHungerRate * Time.deltaTime;
        else
        {
            float distance = Vector3.Distance(previousPosition, transform.position);
            previousPosition = transform.position;
            if (distance < agent.speed * Time.deltaTime * 2) vitality -= SimVars.VARS.villagerHungerRate * distance / agent.speed;
        }

        if (vitality <= vitalityThreshold && (state == VillagerState.Working || state == VillagerState.Walking))
        {
            GetClosestHub();
            if (hub.resources[(int)ResourceType.Food] > 0) ReturnToHub();
        }

        // If starved to death
        if (vitality <= 0)
        {
            if (inventory[(int)ResourceType.Food] > 0)
            {
                inventory[(int)ResourceType.Food]--;
                vitality += SimVars.VARS.vitalityPerFood;
            }
            else
            {
                vitality = 0;
                Die();
            }
        }
    }

    public bool TakeDamage(float damage)
    {
        vitality -= damage;
        damaged = true;
        if (vitality <= 0)
        {
            vitality = 0;
            Die();
            return true;
        }
        return false;
    }

    // Like it says on the tin.
    private void Die()
    {
        // Set alive to false and trigger the death animation.
        anim.SetBool("Working",false);
        anim.SetFloat("Walking",0);
        anim.SetBool("Dead",true);
        float deathValue = UnityEngine.Random.Range(-1f, 1f);
        anim.SetFloat("Death anim", deathValue);
        alive = false;
        if (damaged) causeOfDeath = "Wild Animal";
        else causeOfDeath = "Starvation";
        name += " [" + causeOfDeath + "]";
        TownCenter.TC.RemoveVillager(this);
        transform.SetParent(TownCenter.TC.deadVillagerParent.transform);
        if (target != null) target.ReturnTargetPosition(this);
        if (!isServer) return;
        hub.villagers.Remove(this);
        agent.enabled = false;
    }

    public void AliveHook(bool oldValue, bool newValue)
    {
        if (!alive && !isServer) Die();
    }

    public void FindNewDestination()
    {
        // If the villager has no job, wander randomly.
        if (job == VillagerJob.Nitwit)
        {
            if (RandomNavmeshPoint.RandomPoint(transform.position, selectionRange, out Vector3 targetPosition))
            {
                agent.SetDestination(targetPosition);
            }
        }
        else
        {
            // Get list of possible targets based on role
            List<Targetable> candidates = new List<Targetable>();
            if (job == VillagerJob.Builder) candidates = BuildingGenerator.GetPendingBuildings();
            else if (job == VillagerJob.Lumberjack) candidates = ResourceGenerator.GetTrees();
            else if (job == VillagerJob.Gatherer) candidates = ResourceGenerator.GetBerries();
            else if (job == VillagerJob.Hunter) candidates = ResourceGenerator.GetAnimals();
            Targetable bestCandidate = null;
            float lowestDistance = float.MaxValue;  // Lowest distance of all candidates
            // For each candidate, if they exist:
            foreach (Targetable candidate in candidates)
            {
                if (candidate != null && candidate.priority > 0 && candidate.HasValidPositions())
                {
                    // Find their distance and store the lowest
                    // With high priority objects appearing closer
                    float distance = Vector3.Distance(transform.position, candidate.transform.position);
                    distance /= candidate.priority;
                    if (distance < lowestDistance)
                    {
                        lowestDistance = distance;
                        bestCandidate = candidate;
                    }
                }
            }

            // Use this new distance (without priority) to find the hunger threshold
            if (bestCandidate != null)
            {
                // outpost check
                bool inRange = TargetInRange(bestCandidate, SimVars.VARS.GetMaxVillagerRange() / 2f, out Targetable nearestHub);
                if (!inRange)
                {
                    // means unreachable target
                    // need to spawn an outpost
                    Vector3 outpostPos = nearestHub.transform.position + ((bestCandidate.transform.position - nearestHub.transform.position).normalized * (SimVars.VARS.GetMaxVillagerRange() / 2f));
                    Building building = TownCenter.TC.buildingGenerator.PlaceBuilding(BuildingType.Outpost, outpostPos, TownCenter.TC.GetComponent<Storage>());
                }
                SetNewTarget(bestCandidate);
            }
            else SetNewTarget(bestCandidate);
        }
        ChangeState(VillagerState.Walking);
    }

    private bool TargetInRange(Targetable target, float distance, out Targetable nearestHub)
    {
        List<Storage> hubs = BuildingGenerator.GetHubs();
        float nearestDistance = float.MaxValue;
        nearestHub = null;
        foreach (Storage hub in hubs)
        {
            float hubDistance = Vector3.Distance(hub.transform.position, target.transform.position);
            if (hubDistance < nearestDistance)
            {
                nearestDistance = hubDistance;
                nearestHub = hub;
            }
        }
        List<Targetable> pendingOutposts = BuildingGenerator.GetPendingOutposts();
        foreach (Targetable outpost in pendingOutposts)
        {
            float hubDistance = Vector3.Distance(outpost.transform.position, target.transform.position);
            if (hubDistance < nearestDistance)
            {
                nearestDistance = hubDistance;
                nearestHub = outpost;
            }
        }
        return nearestDistance <= distance;
    }

    private Storage GetClosestHub()
    {
        return GetClosestHub(transform.position);
    }

    private Storage GetClosestHub(Vector3 position)
    {
        Storage nearestHub = TownCenter.TC.GetComponent<Storage>();
        float lowestDistance = float.MaxValue;
        List<Storage> hubs = BuildingGenerator.GetHubs();
        foreach (Storage storage in hubs)
        {
            float distance = Vector3.Distance(position, storage.transform.position);
            distance /= storage.priority;
            if (distance < lowestDistance && storage.HasValidPositions())
            {
                lowestDistance = distance;
                nearestHub = storage;
            }
        }
        hub.villagers.Remove(this);
        hub = nearestHub;
        hub.villagers.Add(this);
        return nearestHub;
    }

    // Change villager state via hook
    private void StateHook(VillagerState oldState, VillagerState newState)
    {
        ChangeState(newState);
    }

    // Update the villager's state to newState
    public void ChangeState(VillagerState newState)
    {
        state = newState;
        // Can update the Animator here
        if (newState == VillagerState.Pending)
        {
            agent.updateRotation = true;
            anim.SetBool("Working", false);
            anim.SetBool("Archer", false);
            anim.SetFloat("Walking",0);
        }
        else if (newState == VillagerState.Walking || newState == VillagerState.Returning)
        {
            agent.updateRotation = true;
            anim.SetBool("Working", false);
            anim.SetBool("Archer", false);
            anim.SetFloat("Walking", 1);
            if (state == VillagerState.Returning)
            {
                if (job == VillagerJob.Lumberjack) backWood.SetActive(true);
                else if (job == VillagerJob.Gatherer) backFood.SetActive(true);
            }
            else if (state == VillagerState.Walking)
            {
                if (job == VillagerJob.Builder) backWood.SetActive(true);
            }
            else DisableBackVisuals();
        }
        else if (newState == VillagerState.Working)
        {
            agent.updateRotation = false;
            if (target != null && target != TownCenter.TC)
            {
                Vector3 towards = target.transform.position - transform.position;
                towards.y = 0;
                transform.rotation = Quaternion.LookRotation(towards);
            }
            if (isServer && job == VillagerJob.Hunter && target.movingTarget)
            {
                anim.SetBool("Archer", true);
            }
            else
            {
                anim.SetBool("Working", true);
            }
            anim.SetFloat("Walking", 0);
        }
    }

    private void DisableBackVisuals()
    {
        backWood.SetActive(false);
        backFood.SetActive(false);
        backOther.SetActive(false);
    }

    // Find opening at new target
    public void SetNewTarget(Targetable newTarget)
    {
        if (newTarget == null) ReturnToHub();
        else
        {
            if (target != null) target.ReturnTargetPosition(this);
            target = newTarget;
            if (target != null && target.movingTarget) agent.stoppingDistance = huntingRange;
            else agent.stoppingDistance = 0.1f;
            if (target != null) agent.SetDestination(target.GetTargetPosition(this));
        }
        float distanceHome = Vector3.Distance(target.transform.position, GetClosestHub(target.transform.position).transform.position);
        vitalityThreshold = SimVars.VARS.vitalityPerFood + distanceHome / agent.speed;
    }

    public void ReturnToHub()
    {
        GetClosestHub();
        SetNewTarget(hub);
        ChangeState(VillagerState.Returning);
    }

    // Change villager job via hook
    private void JobHook(VillagerJob oldJob, VillagerJob newJob)
    {
        ChangeJob(newJob);
    }

    // Update the villager's job
    public void ChangeJob(VillagerJob job)
    {
        this.job = job;
        axe.SetActive(job == VillagerJob.Lumberjack);
        hammer.SetActive(job == VillagerJob.Builder);
        bow.SetActive(job == VillagerJob.Hunter);
        quiver.SetActive(job == VillagerJob.Hunter);
        name = "Villager (" + job + ")";
		JobColor(job);
    }

	// change villager outline color
	public void JobColor(VillagerJob job) {
		//this.job = job;
        if(job == VillagerJob.Lumberjack) {
			outlineS.OutlineColor = Color.green;
		}
        if(job == VillagerJob.Builder) {
			outlineS.OutlineColor = Color.cyan;
		}
        if(job == VillagerJob.Hunter) {
			outlineS.OutlineColor = Color.red;
		}
		if(job == VillagerJob.Gatherer) {
			outlineS.OutlineColor = Color.yellow;
		}
	}

    public void OnSelect()
    {
		outlineS.enabled = true;
    }

    public void OnDeselect()
    {
		outlineS.enabled = false;
    }

    public void DisplayHUD()
    {
        GameObject HUD = UnitHUD.HUD.AddUnitHUD(gameObject, UnitHUD.HUD.villagerHUD, 1f);
        HUD.GetComponent<VillagerDisplay>().villager = this;
    }

    public void RemoveHUD()
    {
        UnitHUD.HUD.RemoveUnitHUD(gameObject);
    }

    // When destroyed, remove villager from other objects
    public void OnDestroy()
    {
        if (TownCenter.TC != null) TownCenter.TC.RemoveVillager(this);
        Selection.Selector.RemoveSelectable(this);
        UnitHUD.HUD.RemoveUnitHUD(gameObject);
    }
}

public enum VillagerState
{
    Pending,
    Walking,
    Working,
    Returning,
}
