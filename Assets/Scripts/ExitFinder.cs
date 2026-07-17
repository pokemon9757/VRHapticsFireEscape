using UnityEngine;
using UnityEngine.AI;

public class ExitFinder : MonoBehaviour
{
    [Header("Route")]
    public Transform Exit;

    [SerializeField, Min(0.02f)] private float routeUpdateInterval = 0.1f;
    [SerializeField, Min(0.01f)] private float cornerReachDistance = 0.25f;
    [SerializeField, Min(0.1f)] private float navMeshSampleRadius = 1f;
    [SerializeField] private bool logRoute;

    private NavMeshPath path;
    private float nextRouteUpdateTime;

    public NavMeshPath Path => path;
    public bool HasRoute { get; private set; }

    void Awake()
    {
        path = new NavMeshPath();
    }
    
    private void Update()
    {
        if (Time.time >= nextRouteUpdateTime)
        {
            CalculateRoute();
            nextRouteUpdateTime = Time.time + routeUpdateInterval;
        }
    }

    /// <summary>
    /// Returns the horizontal direction from the wearer to the next useful path corner.
    /// </summary>
    public bool TryGetDirectionToNextCorner(out Vector3 direction)
    {
        direction = Vector3.zero;

        if (!HasRoute || path.corners == null || path.corners.Length < 2)
        {
            return false;
        }

        float minimumSqrDistance = cornerReachDistance * cornerReachDistance;

        // Corner zero normally represents the snapped start. Skip any other corner
        // already close enough to the wearer and guide toward the first useful one.
        for (int i = 1; i < path.corners.Length; i++)
        {
            Vector3 toCorner = path.corners[i] - transform.position;
            toCorner.y = 0f;

            if (toCorner.sqrMagnitude > minimumSqrDistance)
            {
                direction = toCorner.normalized;
                return true;
            }
        }

        return false;
    }

    private Vector3 SnapToNavMesh(Vector3 targetPosition)
    {
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return targetPosition;
    }

    private void CalculateRoute()
    {
        if (Exit == null)
        {
            HasRoute = false;
            return;
        }

        Vector3 snappedStart = SnapToNavMesh(transform.position);
        Vector3 snappedEnd = SnapToNavMesh(Exit.position);
        HasRoute = NavMesh.CalculatePath(snappedStart, snappedEnd, NavMesh.AllAreas, path)
            && path.status == NavMeshPathStatus.PathComplete;

        if (!logRoute)
        {
            return;
        }

        if (HasRoute)
        {
            Debug.Log($"Route to exit: {path.corners.Length} corners, {GetPathDistance(path):F1} metres.", this);
        }
        else
        {
            Debug.LogWarning("No valid path found between the wearer and the exit.", this);
        }
    }

    private static float GetPathDistance(NavMeshPath route)
    {
        float totalDistance = 0f;

        for (int i = 0; i < route.corners.Length - 1; i++)
        {
            totalDistance += Vector3.Distance(route.corners[i], route.corners[i + 1]);
        }

        return totalDistance;
    }
}
