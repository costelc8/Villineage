using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Unity.VisualScripting.Member;

public class PassiveAnimal : Resource
{
    Animator anim;
    public Vector3 wanderOrigin;
    public float originTetherStrength;
    public float maxWanderCooldown;
    public float maxHealth;
    public float health;
    public bool dead;
    private Transform model;
    private NavMeshAgent agent;
    //private NavMeshObstacle obstacle;
    private float wanderCooldown;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        //obstacle = GetComponent<NavMeshObstacle>();
        model = transform.GetChild(0);
        maxWanderCooldown = Mathf.Max(1f, maxWanderCooldown);
        wanderCooldown = Random.Range(1f, maxWanderCooldown);
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (!dead) Wander();
        else if (model.localRotation.eulerAngles.z < 90) model.Rotate(new Vector3(0f, 0f, 180f * Time.deltaTime));
    }

    private void Wander()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            wanderCooldown -= Time.deltaTime;
            if (wanderCooldown <= 0)
            {
                wanderCooldown = Random.Range(1f, maxWanderCooldown);
                if (RandomNavmeshPoint.RandomPointFromCenterCapsule(transform.position, 1.5f, 1f, out Vector3 target, 5f, 1f, 10f))
                {
                    Vector3 towardsOrigin = wanderOrigin - target;
                    agent.SetDestination(target + (towardsOrigin * originTetherStrength));
                    agent.speed = 1f;
                }
                anim.Play("walk_forward");
            }
            else anim.Play("idle");
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
                Vector3 towardsSource = villager.transform.position - transform.position;
                agent.SetDestination(transform.position - towardsSource);
                agent.speed = 2f;
                wanderCooldown = 5f;
                anim.Play("run_forward");
            }
            return false;
        }
        else return base.Progress(villager, progressValue);
    }

    private void Die()
    {
        dead = true;
        agent.enabled = false;
        liveAnimal = false;
        agent.enabled = false;
        obstacle.enabled = true;
        anim.Play("stand_to_sit");
        priority *= 100;
        anim.speed = 5f;
    }
}
