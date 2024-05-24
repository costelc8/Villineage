using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static Unity.VisualScripting.Member;

public class Animal : Resource
{
    Animator anim;
    public Vector3 wanderOrigin;
    public float originTetherStrength;
    public float maxWanderCooldown;
    public int maxHealth;
    public int health;
    private Transform model;
    private NavMeshAgent agent;
    public float wanderCooldown;
    [SyncVar(hook = nameof(StateHook))]
    public AnimalState state;
    public AnimalType type;
    private float attackCooldown;
    private Villager targetVillager;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        model = transform.GetChild(0);
        maxWanderCooldown = Mathf.Max(1f, maxWanderCooldown);
        wanderCooldown = Random.Range(1f, maxWanderCooldown);
        health = maxHealth;
        isAnimal = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == AnimalState.Dead && model.localRotation.eulerAngles.z < 90) model.Rotate(new Vector3(0f, 0f, 180f * Time.deltaTime)); // Rotate on its side to "die"
        if (!isServer) return;
        if (targetVillager != null && !targetVillager.alive) targetVillager = null;
        if (state != AnimalState.Dead) Wander();
    }

    private void Wander()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            wanderCooldown -= Time.deltaTime;
            if (wanderCooldown <= 0)
            {
                wanderCooldown = Random.Range(1f, maxWanderCooldown);
                Vector3 randomDirection = (Random.onUnitSphere * 100 + ((wanderOrigin - transform.position) * originTetherStrength)).normalized;
                Vector3 targetPos = transform.position + (randomDirection * (Random.Range(4f, 8f) * agent.speed));
                targetPos.y = transform.position.y;
                if (RandomNavmeshPoint.RandomPointFromCenterCapsule(targetPos, 1.5f, 1f, out Vector3 target, 0, 1f, 10f))
                {
                    agent.SetDestination(target);
                    ChangeSpeed(false);
                    ChangeState(AnimalState.Wandering);
                    targetVillager = null;
                }
            }
            else if (type != AnimalType.Passive && targetVillager != null && targetVillager.alive)
            {
                agent.updateRotation = false;
                transform.rotation = Quaternion.LookRotation(targetVillager.transform.position - transform.position);
                Vector3 towardsSource = targetVillager.transform.position - transform.position;
                agent.SetDestination(transform.position + towardsSource);
                attackCooldown -= Time.deltaTime;
                if (attackCooldown <= 0)
                {
                    attackCooldown = 1f;
                    targetVillager.TakeDamage(Random.Range(20f, 40f));
                    if (!targetVillager.alive) targetVillager = null;
                }
            }
            else
            {
                agent.updateRotation = true;
                ChangeState(AnimalState.Idle);
            }
        }
        else if (type != AnimalType.Passive && targetVillager != null && targetVillager.alive)
        {
            Vector3 towardsSource = targetVillager.transform.position - transform.position;
            agent.SetDestination(transform.position + towardsSource);
        }
    }

    public override bool Progress(Villager villager)
    {
        if (health > 0)
        {
            health--;
            if (health <= 0)
            {
                health = 0;
                Die();
            }
            else
            {
                if (targetVillager == null) targetVillager = villager;
                Vector3 towardsSource = targetVillager.transform.position - transform.position;
                if (type != AnimalType.Passive) agent.SetDestination(transform.position + towardsSource);
                else agent.SetDestination(transform.position - towardsSource);
                ChangeSpeed(true);
                wanderCooldown = 5f;
                ChangeState(AnimalState.Running);
            }
            return false;
        }
        else return base.Progress(villager);
    }

    private void ChangeSpeed(bool run)
    {
        if (run) agent.speed = (type == AnimalType.Passive) ? 2f : 6f;
        else agent.speed = (type == AnimalType.Passive) ? 1f : 3f;
        agent.acceleration = agent.speed * 2;
    }

    private void Die()
    {
        agent.enabled = false;
        movingTarget = false;
        agent.enabled = false;
        obstacle.enabled = true;
        priority *= 10f;
        ChangeState(AnimalState.Dead);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (type == AnimalType.Hostile)
        {
            Villager villager = other.GetComponent<Villager>();
            if (villager != null)
            {
                this.priority += 10.0f;
                if (villager.job != VillagerJob.Hunter && villager.alive) villager.ReturnToHub();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (type == AnimalType.Hostile)
        {
            Villager villager = other.GetComponent<Villager>();
            if (villager != null && (targetVillager == null || targetVillager == villager) && villager.alive)
            {
                targetVillager = villager;
                Vector3 towardsSource = targetVillager.transform.position - transform.position;
                agent.SetDestination(transform.position + towardsSource);
                ChangeSpeed(true);
                wanderCooldown = 5f;
                ChangeState(AnimalState.Running);
            }
        }
    }

    private void StateHook(AnimalState oldState, AnimalState newState)
    {
        ChangeState(newState);
    }

    private void ChangeState(AnimalState state)
    {
        if (this.state == state) return;
        this.state = state;
        if (type != AnimalType.Hostile)
        {
            if (state == AnimalState.Idle) anim.Play("idle");
            if (state == AnimalState.Wandering) anim.Play("walk_forward");
            if (state == AnimalState.Running) anim.Play("run_forward");
            if (state == AnimalState.Dead)
            {
                anim.Play("stand_to_sit");
                anim.speed = 5f;
            }
        }
        else
        {
            // Wolf animations
        }
    }
}

public enum AnimalType
{
    Passive,
    Neutral,
    Hostile,
}

public enum AnimalState
{
    Idle,
    Wandering,
    Running,
    Dead,
}