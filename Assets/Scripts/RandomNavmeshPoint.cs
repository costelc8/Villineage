using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RandomNavmeshPoint : MonoBehaviour
{
    public bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            Debug.DrawRay(result, Vector3.up, Color.blue, 1.0f);
            return true;
        }
        result = Vector3.zero;
        return false;
    }
}
