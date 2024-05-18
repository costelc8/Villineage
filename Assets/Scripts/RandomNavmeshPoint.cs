using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class RandomNavmeshPoint
{
    public static bool RandomPoint(Vector3 center, float range, out Vector3 result, int attempts = 10)
    {
        for (int i = 0; i < attempts; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }

    public static bool RandomPointFromCenterBox(Vector3 center, Vector3 halfExtents, out Vector3 result, float minDistance, float increment, float maxDistance)
    {
        for (float distance = minDistance; distance <= maxDistance; distance += increment)
        {
            if (RandomPointFromCenter(center, distance, out Vector3 point))
            {
                if (!Physics.CheckBox(point + (Vector3.up * halfExtents.y), halfExtents, Quaternion.identity, ~LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
                {
                    result = point;
                    return true;
                }
            }
        }
        result = Vector2.zero;
        return false;
    }

    public static bool RandomPointFromCenterCapsule(Vector3 center, float radius, float height, out Vector3 result, float minDistance, float increment, float maxDistance)
    {
        height = Mathf.Max(height, radius * 2f);
        for (float distance = minDistance; distance <= maxDistance; distance += increment)
        {
            if (RandomPointFromCenter(center, distance, out Vector3 point))
            {
                Vector3 start = point + (Vector3.up * radius);
                Vector3 end = point + (Vector3.up * (height - (radius * 2)));
                if (!Physics.CheckCapsule(start, end, radius, ~LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
                {
                    result = point;
                    return true;
                }
            }
        }
        result = Vector2.zero;
        return false;
    }

    public static bool RandomPointFromCenterSphere(Vector3 center, float radius, out Vector3 result, float minDistance, float increment, float maxDistance)
    {
        for (float distance = minDistance; distance <= maxDistance; distance += increment)
        {
            if (RandomPointFromCenter(center, distance, out Vector3 point))
            {
                if (!Physics.CheckSphere(point + (Vector3.up * radius), radius, ~LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
                {
                    result = point;
                    return true;
                }
            }
        }
        result = Vector2.zero;
        return false;
    }

    public static bool RandomPointFromCenter(Vector3 center, float distance, out Vector3 point)
    {
        center.y = 100f;
        Vector2 offset = Random.insideUnitCircle.normalized * distance;
        Vector3 randomPoint = center + new Vector3(offset.x, 0, offset.y);
        Debug.DrawRay(randomPoint, Vector3.down * 100f, Color.red, 1f);
        if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hitInfo, 100f, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
        {
            if (NavMesh.SamplePosition(hitInfo.point, out NavMeshHit hit, 0.5f, NavMesh.AllAreas))
            {
                point = hit.position;
                return true;
            }
        }
        point = Vector3.zero;
        return false;
    }
}
