using UnityEngine;
using UnityEngine.AI;

public class ExitFinder : MonoBehaviour
{
    public Transform Exit;

    void Update()
    {
        CalculateRoute();
    }
    Vector3 SnapToNavMesh(Vector3 targetPosition)
    {
        NavMeshHit hit;
        // Search within 1.0 unit radius for the closest valid NavMesh point
        if (NavMesh.SamplePosition(targetPosition, out hit, 1.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return targetPosition; // Fallback if no NavMesh found nearby
    }
    void CalculateRoute()
    {
        // 1. Create a container to hold the calculated path
        NavMeshPath path = new NavMeshPath();
        Vector3 snappedStart = SnapToNavMesh(transform.position);
        Vector3 snappedEnd = SnapToNavMesh(Exit.position);
        // 2. Calculate the path (returns true if a path is found)
        // NavMesh.AllAreas uses all available NavMesh layers/costs
        if (NavMesh.CalculatePath(snappedStart, snappedEnd, NavMesh.AllAreas, path))
        {
            Debug.Log($"Path calculated successfully! Corner count: {path.corners.Length}");
            Debug.Log($"Total path distance: {GetPathDistance(path)}");

            // The 'path.corners' array contains all the waypoint positions
            for (int i = 0; i < path.corners.Length; i++)
            {
                Debug.Log($"Waypoint {i}: {path.corners[i]}");
            }
        }
        else
        {
            Debug.LogWarning("No valid path found between the points.");
        }
    }

    float GetPathDistance(NavMeshPath path)
    {
        if (path.corners.Length < 2) return 0f;

        float totalDistance = 0f;
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            totalDistance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }
        return totalDistance;
    }
}
