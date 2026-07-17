using UnityEngine;

/// <summary>
/// Converts the next direction supplied by ExitFinder into four-motor belt cues.
/// North is the wearer's forward direction. Diagonal cues run both adjacent motors.
/// </summary>
[RequireComponent(typeof(ExitFinder))]
public class VibrotactileNavigationController : MonoBehaviour
{
    [System.Flags]
    private enum MotorMask
    {
        None = 0,
        North = 1 << 0,
        East = 1 << 1,
        South = 1 << 2,
        West = 1 << 3
    }

    [Header("References")]
    [SerializeField] private ExitFinder exitFinder;
    [SerializeField] private VibraForge sender;
    [Tooltip("The transform whose forward direction represents North. Defaults to this transform.")]
    [SerializeField] private Transform headingSource;

    [Header("Motor addresses")]
    [SerializeField] private int northAddress = 0;
    [SerializeField] private int eastAddress = 1;
    [SerializeField] private int southAddress = 2;
    [SerializeField] private int westAddress = 3;

    [Header("Vibration command")]
    [SerializeField] private int onMode = 1;
    [SerializeField] private int offMode = 0;
    [SerializeField] private int duty = 7;
    [SerializeField] private int frequency = 2;

    [Header("Direction stability")]
    [Tooltip("Total transition band between neighbouring directions. 10 degrees means a cue must cross 5 degrees beyond the normal boundary before changing.")]
    [SerializeField, Range(0f, 30f)] private float hysteresisDegrees = 10f;

    private int currentSector = -1;
    private MotorMask activeMotors = MotorMask.None;

    private static readonly MotorMask[] SectorMotors =
    {
        MotorMask.North,
        MotorMask.North | MotorMask.East,
        MotorMask.East,
        MotorMask.South | MotorMask.East,
        MotorMask.South,
        MotorMask.South | MotorMask.West,
        MotorMask.West,
        MotorMask.North | MotorMask.West
    };

    private void Reset()
    {
        exitFinder = GetComponent<ExitFinder>();
        headingSource = transform;
    }

    private void Awake()
    {
        if (exitFinder == null)
        {
            exitFinder = GetComponent<ExitFinder>();
        }

        if (headingSource == null)
        {
            headingSource = transform;
        }
    }

    private void Update()
    {
        if (exitFinder == null || sender == null ||
            !exitFinder.TryGetDirectionToNextCorner(out Vector3 worldDirection))
        {
            currentSector = -1;
            SetActiveMotors(MotorMask.None);
            return;
        }

        Vector3 forward = Vector3.ProjectOnPlane(headingSource.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.001f)
        {
            return;
        }

        float signedAngle = Vector3.SignedAngle(forward, worldDirection, Vector3.up);
        int sector = SelectSector(signedAngle);
        SetActiveMotors(SectorMotors[sector]);
    }

    private int SelectSector(float signedAngle)
    {
        if (currentSector >= 0)
        {
            float currentCentre = currentSector * 45f;
            float distanceFromCurrentCentre = Mathf.Abs(Mathf.DeltaAngle(currentCentre, signedAngle));
            float exitThreshold = 22.5f + hysteresisDegrees * 0.5f;

            if (distanceFromCurrentCentre <= exitThreshold)
            {
                return currentSector;
            }
        }

        currentSector = Mathf.RoundToInt(Mathf.Repeat(signedAngle, 360f) / 45f) % 8;
        return currentSector;
    }

    private void SetActiveMotors(MotorMask requestedMotors)
    {
        if (requestedMotors == activeMotors || sender == null)
        {
            return;
        }

        UpdateMotor(MotorMask.North, northAddress, requestedMotors);
        UpdateMotor(MotorMask.East, eastAddress, requestedMotors);
        UpdateMotor(MotorMask.South, southAddress, requestedMotors);
        UpdateMotor(MotorMask.West, westAddress, requestedMotors);
        activeMotors = requestedMotors;
    }

    private void UpdateMotor(MotorMask motor, int address, MotorMask requestedMotors)
    {
        bool wasActive = (activeMotors & motor) != 0;
        bool shouldBeActive = (requestedMotors & motor) != 0;

        if (wasActive != shouldBeActive)
        {
            sender.SendCommand(address, shouldBeActive ? onMode : offMode, duty, frequency);
        }
    }

    private void OnDisable()
    {
        SetActiveMotors(MotorMask.None);
        currentSector = -1;
    }
}
