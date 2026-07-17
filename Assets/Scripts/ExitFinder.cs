using UnityEngine;
using UnityEngine.AI;

public class ExitFinder : MonoBehaviour
{
    private enum RouteState
    {
        Unknown,
        MissingExit,
        WearerOffNavMesh,
        ExitOffNavMesh,
        Invalid,
        Partial,
        Complete
    }

    [Header("Route")]
    public Transform Wearer;
    public Transform Exit;

    [SerializeField, Min(0.02f)] private float routeUpdateInterval = 0.1f;
    [SerializeField, Min(0.01f)] private float cornerReachDistance = 0.25f;
    [Tooltip("Search distance used to project the wearer and exit onto the floor NavMesh. VR headsets usually require at least 2 metres.")]
    [SerializeField, Min(0.1f)] private float navMeshSampleRadius = 3f;
    [SerializeField] private bool logRoute;

    private NavMeshPath path;
    private float nextRouteUpdateTime;
    private RouteState lastRouteState = RouteState.Unknown;

    public NavMeshPath Path => path;
    public bool HasRoute { get; private set; }

    private Transform WearerTransform => Wearer != null ? Wearer : transform;

    private void Awake()
    {
        path = new NavMeshPath();

        // Existing scene components were previously serialized with a 1 metre
        // radius, which is often too short to reach the floor from an XR headset.
        navMeshSampleRadius = Mathf.Max(navMeshSampleRadius, 3f);
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
            Vector3 toCorner = path.corners[i] - WearerTransform.position;
            toCorner.y = 0f;

            if (toCorner.sqrMagnitude > minimumSqrDistance)
            {
                direction = toCorner.normalized;
                return true;
            }
        }

        return false;
    }

    private bool TrySnapToNavMesh(Vector3 targetPosition, out Vector3 snappedPosition)
    {
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            snappedPosition = hit.position;
            return true;
        }

        snappedPosition = targetPosition;
        return false;
    }

    private void CalculateRoute()
    {
        if (Exit == null)
        {
            HasRoute = false;
            ReportRouteStatus(RouteState.MissingExit, "Exit reference is not assigned.", true);
            return;
        }

        Vector3 wearerPosition = WearerTransform.position;
        if (!TrySnapToNavMesh(wearerPosition, out Vector3 snappedStart))
        {
            HasRoute = false;
            ReportRouteStatus(RouteState.WearerOffNavMesh,
                $"Wearer at {wearerPosition} is not within {navMeshSampleRadius:F1}m of a NavMesh.", true);
            return;
        }

        if (!TrySnapToNavMesh(Exit.position, out Vector3 snappedEnd))
        {
            HasRoute = false;
            ReportRouteStatus(RouteState.ExitOffNavMesh,
                $"Exit at {Exit.position} is not within {navMeshSampleRadius:F1}m of a NavMesh.", true);
            return;
        }

        bool pathCalculated = NavMesh.CalculatePath(
            snappedStart, snappedEnd, NavMesh.AllAreas, path);

        HasRoute = pathCalculated && path.status == NavMeshPathStatus.PathComplete;

        if (!pathCalculated || path.status == NavMeshPathStatus.PathInvalid)
        {
            ReportRouteStatus(RouteState.Invalid,
                $"Path is invalid. Start {snappedStart}, exit {snappedEnd}. Check that both points use the same baked NavMeshSurface.",
                true);
            return;
        }

        if (path.status == NavMeshPathStatus.PathPartial)
        {
            ReportRouteStatus(RouteState.Partial,
                $"Path is partial. The wearer and exit are on disconnected NavMesh islands. Start {snappedStart}, exit {snappedEnd}.",
                true);
            return;
        }

        ReportRouteStatus(RouteState.Complete,
            $"Route complete: {path.corners.Length} corners, {GetPathDistance(path):F1} metres.",
            false);
    }

    private void ReportRouteStatus(RouteState state, string status, bool isWarning)
    {
        if (state == lastRouteState)
        {
            return;
        }

        lastRouteState = state;

        if (isWarning)
        {
            Debug.LogWarning($"ExitFinder: {status}", this);
        }
        else if (logRoute)
        {
            Debug.Log($"ExitFinder: {status}", this);
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
