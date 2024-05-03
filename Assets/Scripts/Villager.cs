using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Villager : MonoBehaviour, ISelectable
{
    private NavMeshAgent agent;
    public TownCenter townCenter;
    public bool selected;

    [Header("Jobs")]
    public VillagerJob job;
    public float workSpeed = 1.0f;  // Speed of resource extraction
    public int[] resources = new int[(int)ResourceType.MAX_VALUE]; // Amount of resources being carried
    public int totalResources = 0; //Total number of resources across all types
    public int capacity = 3;  // Maximum amount of resources it can carry
    public Targetable target;

    [Header("Movement")]
    public float selectionRange = 10.0f;  // Selection range for random movement
    private float distance;

    [Header("States")]
    //public bool working = false;  // Are they currently working on something?
    //public bool walking = false;  // Are they wandering around?
    //public bool returning = false;
    public VillagerState state;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == VillagerState.Pending)
        {
            FindNewDestination();
            if (target == null) // Edge case handling
            {
                state = VillagerState.Pending;
                SetNewTarget(townCenter);
            }
        }
        else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (state == VillagerState.Walking)
            {
                state = VillagerState.Working;
            }
            else if (state == VillagerState.Working)
            {
                if (target == null) state = VillagerState.Pending;
                else
                {
                    bool progress = target.Progress(this, workSpeed * Time.deltaTime);
                    if (progress && (job == VillagerJob.Builder || totalResources >= capacity))
                    {
                        state = VillagerState.Returning;
                        SetNewTarget(townCenter);
                    }
                }
            }
            else if (state == VillagerState.Returning)
            {
                townCenter.DepositResources(this, resources);
                Array.Clear(resources, 0, resources.Length);
                totalResources = 0;
                state = VillagerState.Pending;
            }
        }
    }

    public void FindNewDestination()
    {
        if (job == VillagerJob.Nitwit)
        {
            if (RandomNavmeshPoint.RandomPoint(transform.position, selectionRange, out Vector3 targetPosition))
            {
                agent.SetDestination(targetPosition);
            }
        }
        else
        {
            List<Targetable> candidates = new List<Targetable>();
            if (job == VillagerJob.Builder) candidates = BuildingGenerator.GetPendingBuildings();
            else if (job == VillagerJob.Lumberjack) candidates = ResourceGenerator.GetTrees();
            else if (job == VillagerJob.Gatherer) candidates = ResourceGenerator.GetBerries();
            Targetable bestCandidate = null;
            float lowestDistance = float.MaxValue;
            foreach (Targetable candidate in candidates)
            {
                if (candidate != null)
                {
                    distance = Vector3.Distance(transform.position, candidate.transform.position);
                    if (distance < lowestDistance && candidate.HasValidPositions())
                    {
                        lowestDistance = distance;
                        bestCandidate = candidate;
                    }
                }
            }
            SetNewTarget(bestCandidate);
        }
        state = VillagerState.Walking;
    }

    private void SetNewTarget(Targetable newTarget)
    {
        if (target != null) target.ReturnTargetPosition(this);
        target = newTarget;
        if (target != null) agent.SetDestination(target.GetTargetPosition(this));
    }

    public void OnSelect()
    {
        selected = true;
        GameObject HUD = UnitHUD.HUD.AddUnitHUD(gameObject, UnitHUD.HUD.dummyHUD, 1f);
        HUD.GetComponent<DisplayController>().villager = this;
    }

    public void OnDeselect()
    {
        selected = false;
        UnitHUD.HUD.RemoveUnitHUD(gameObject);
    }

    public enum VillagerState
    {
        Pending,
        Walking,
        Working,
        Returning,
    }
}