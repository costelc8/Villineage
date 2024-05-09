using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Unity.VisualScripting.Member;

public class PassiveAnimal : Targetable
{
    Animator anim;
    public Vector3 wanderOrigin;
    public float originTetherStrength;
    public float maxWanderCooldown;
    public float maxHealth;
    public float health;
    public bool dead;
    private NavMeshAgent agent;
    private float wanderCooldown;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        maxWanderCooldown = Mathf.Max(1f, maxWanderCooldown);
        wanderCooldown = Random.Range(1f, maxWanderCooldown);
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (!dead) Wander();
        else if (transform.rotation.eulerAngles.x < 90) transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 90), Time.deltaTime * 90);
    }

    private void Wander()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            wanderCooldown -= Time.deltaTime;
            if (wanderCooldown <= 0)
            {
                wanderCooldown = Random.Range(1f, maxWanderCooldown);
                if (RandomNavmeshPoint.RandomPointFromCenter(transform.position, 10f, out Vector3 target))
                {
                    Vector3 towardsOrigin = wanderOrigin - target;
                    agent.SetDestination(target + (towardsOrigin * originTetherStrength));
                    agent.speed = 2f;
                }
                anim.Play("walk_forward");
            }
            else
            {
                Debug.Log("Idle");
                anim.Play("idle");
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
                Vector3 towardsSource = villager.transform.position - transform.position;
                agent.SetDestination(transform.position - towardsSource);
                agent.speed = 3f;
                wanderCooldown = 5f;
            }
            return false;
        }
        else
        {
            return true;
        }
    }

    private void Die()
    {
        dead = true;
        agent.enabled = false;
        anim.Play("stand_to_sit");
    }
}
