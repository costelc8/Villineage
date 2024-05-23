using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Targetable : NetworkBehaviour
{
    [Header("Targetable Settings")]
    private List<TargetPosition> targetPositions;
    public bool enforceMaxVillagers;
    public int maxAssignedVillagers;
    public int assignedVillagers;
    public float priority = 1;
    public bool movingTarget = false;
    protected NavMeshObstacle obstacle;

    public virtual bool Progress(Villager villager)
    {
        Debug.LogWarning("This method should be overwritten and never called directly!");
        return false;
    }

    private void GenerateValidPositions()
    {
        targetPositions = new List<TargetPosition>();
        obstacle = GetComponent<NavMeshObstacle>();
        if (obstacle.enabled == false) return;
        if (obstacle.shape == NavMeshObstacleShape.Capsule)
        {
            float radius = obstacle.radius + 0.5f;
            int count = Mathf.FloorToInt(radius * 6);
            float interval = 2 * Mathf.PI / count;
            for (int i = 0; i < count; i++)
            {
                Vector3 position = new Vector3(Mathf.Cos(interval * i), 0, Mathf.Sin(interval * i)) * radius;
                targetPositions.Add(new TargetPosition(position + transform.position));
            }
        }
        else if (obstacle.shape == NavMeshObstacleShape.Box)
        {
            float sizeX = (obstacle.size.x - 1f) / 2;
            float sizeZ = (obstacle.size.z - 1f) / 2;
            for (float x = -sizeX; x <= sizeX; x++)
            {
                Vector3 position1 = new Vector3(x, 0, sizeZ + 1f);
                Vector3 position2 = new Vector3(x, 0, -sizeZ - 1f);
                targetPositions.Add(new TargetPosition(position1 + transform.position));
                targetPositions.Add(new TargetPosition(position2 + transform.position));
            }
            for (float z = -sizeZ; z <= sizeZ; z++)
            {
                Vector3 position1 = new Vector3(sizeX + 1f, 0, z);
                Vector3 position2 = new Vector3(-sizeX - 1f, 0, z);
                targetPositions.Add(new TargetPosition(position1 + transform.position));
                targetPositions.Add(new TargetPosition(position2 + transform.position));
            }
        }
    }

    public bool HasValidPositions()
    {
        if (targetPositions == null || targetPositions.Count == 0) return true;
        if (targetPositions.Count == 0)
        {
            if (enforceMaxVillagers) return assignedVillagers < maxAssignedVillagers;
            else return true;
        }
        else
        {
            if (enforceMaxVillagers) return assignedVillagers < targetPositions.Count && assignedVillagers < maxAssignedVillagers;
            else return assignedVillagers < targetPositions.Count;
        }
    }

    public Vector3 GetTargetPosition(Villager villager)
    {
        if (targetPositions == null || targetPositions.Count == 0) GenerateValidPositions();
        assignedVillagers++;
        if (targetPositions.Count == 0) return transform.position;
        TargetPosition nearest = null;
        float minMag = float.MaxValue;
        foreach (TargetPosition targetPos in targetPositions)
        {
            if (targetPos.assignedVillager != null) continue;
            float sqrMag = Vector3.SqrMagnitude(villager.transform.position - targetPos.position);
            if (sqrMag < minMag)
            {
                minMag = sqrMag;
                if (nearest != null) nearest.assignedVillager = null;
                targetPos.assignedVillager = villager;
                nearest = targetPos;
            }
        }
        if (nearest != null) return nearest.position;
        else if(!enforceMaxVillagers) // Do the check again, allowing already-reserved spots
        {
            foreach (TargetPosition targetPos in targetPositions)
            {
                float sqrMag = Vector3.SqrMagnitude(villager.transform.position - targetPos.position);
                if (sqrMag < minMag)
                {
                    minMag = sqrMag;
                    nearest = targetPos;
                }
            }
            return nearest.position;
        }
        else return transform.position;
    }

    public void ReturnTargetPosition(Villager villager)
    {
        foreach (TargetPosition targetPos in targetPositions)
        {
            if (targetPos.assignedVillager == villager)
            {
                targetPos.assignedVillager = null;
            }
        }
        assignedVillagers--;
    }

    public void UntargetAll(bool forceReturn)
    {
        foreach (TargetPosition targetPos in targetPositions)
        {
            if (targetPos.assignedVillager != null)
            {
                if (forceReturn) targetPos.assignedVillager.ReturnToHub();
                else targetPos.assignedVillager.target = null;
                targetPos.assignedVillager = null;
            }
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (targetPositions != null)
        {
            foreach (TargetPosition targetPos in targetPositions)
            {
                if (targetPos.assignedVillager == null) Gizmos.color = Color.green;
                else Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(targetPos.position, 0.5f);
            }
        }
    }

    private class TargetPosition
    {
        public Vector3 position;
        public Villager assignedVillager;

        public TargetPosition(Vector3 position)
        {
            this.position = position;
            assignedVillager = null;
        }
    }
}
