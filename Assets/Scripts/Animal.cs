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
    public float maxHealth;
    public float health;
    private Transform model;
    private NavMeshAgent agent;
    private float wanderCooldown;
    [SyncVar(hook = nameof(StateHook))]
    public AnimalState state;
    public bool hostile;
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
    }

    // Update is called once per frame
    void Update()
    {
        if (state == AnimalState.Dead && model.localRotation.eulerAngles.z < 90) model.Rotate(new Vector3(0f, 0f, 180f * Time.deltaTime)); // Rotate on its side to "die"
        if (!isServer) return;
        if (state != AnimalState.Dead) Wander();
    }

    private void Wander()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            wanderCooldown -= Time.deltaTime;
            if (wanderCooldown <= 0)
            {
                wanderCooldown = Random.Range(1f, maxWanderCooldown);
                Vector3 randomDirection = (Random.onUnitSphere * 100 + (wanderOrigin - transform.position) * originTetherStrength).normalized;
                Vector3 targetPos = transform.position + (randomDirection * (Random.Range(4f, 8f) * (hostile ? 2.5f : 1f)));
                targetPos.y = transform.position.y;
                if (RandomNavmeshPoint.RandomPointFromCenterCapsule(targetPos, 1.5f, 1f, out Vector3 target, 0, 1f, 10f))
                {
                    agent.SetDestination(target);
                    agent.speed = hostile ? 2.5f : 1f;
                    ChangeState(AnimalState.Wandering);
                    targetVillager = null;
                }
            }
            else if (hostile && targetVillager != null && targetVillager.alive)
            {
                agent.updateRotation = false;
                transform.rotation = Quaternion.LookRotation(targetVillager.transform.position - transform.position);
                attackCooldown -= Time.deltaTime;
                if (attackCooldown <= 0)
                {
                    attackCooldown = 1f;
                    targetVillager.TakeDamage(Random.Range(20f, 40f));
                }
                Vector3 towardsSource = targetVillager.transform.position - transform.position;
                agent.SetDestination(transform.position + towardsSource);
            }
            else
            {
                agent.updateRotation = true;
                ChangeState(AnimalState.Idle);
            }
        }
    }

    public override bool Progress(Villager villager, float progressValue)
    {
        if (health > 0)
        {
            health -= progressValue;
            if (health <= 0)
            {
                health = 0;
                Die();
            }
            else
            {
                if (targetVillager == null) targetVillager = villager;
                Vector3 towardsSource = targetVillager.transform.position - transform.position;
                if (hostile) agent.SetDestination(transform.position + towardsSource);
                else agent.SetDestination(transform.position - towardsSource);
                agent.speed = hostile ? 5f : 2f;
                wanderCooldown = 5f;
                ChangeState(AnimalState.Running);
            }
            return false;
        }
        else return base.Progress(villager, progressValue);
    }

    private void Die()
    {
        agent.enabled = false;
        movingTarget = false;
        agent.enabled = false;
        obstacle.enabled = true;
        priority *= 100f;
        ChangeState(AnimalState.Dead);
    }

    private void StateHook(AnimalState oldState, AnimalState newState)
    {
        ChangeState(newState);
    }

    private void ChangeState(AnimalState state)
    {
        if (this.state == state) return;
        this.state = state;
        if (state == AnimalState.Idle) anim.Play("idle");
        if (state == AnimalState.Wandering) anim.Play("walk_forward");
        if (state == AnimalState.Running) anim.Play("run_forward");
        if (state == AnimalState.Dead)
        {
            anim.Play("stand_to_sit");
            anim.speed = 5f;
        }
    }
}

public enum AnimalState
{
    Idle,
    Wandering,
    Running,
    Dead,
}