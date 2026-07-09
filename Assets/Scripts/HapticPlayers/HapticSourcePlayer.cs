using UnityEngine;
using Oculus.Haptics;

/// <summary>
/// Plays an authored Meta Haptics clip (.haptic) through a HapticSource component.
/// </summary>
[AddComponentMenu("Haptics/Haptic Source Player")]
[RequireComponent(typeof(HapticSource))]
public class HapticSourcePlayer : HapticPlayerBase
{
    [Header("Playback")]
    [Tooltip("Which controller should play this haptic clip.")]
    [SerializeField] private Controller controller = Controller.Both;

    [Tooltip("Scales the strength of the haptic clip.")]
    [Range(0f, 5f)]
    [SerializeField] private float amplitude = 1f;

    [Tooltip("Shifts the feel of the haptic clip lower or higher.")]
    [Range(-1f, 1f)]
    [SerializeField] private float frequencyShift = 0f;

    [Tooltip("Automatically play this haptic when the scene starts.")]
    [SerializeField] private bool playOnStart = false;

    private HapticSource m_hapticSource;

    private void Awake()
    {
        m_hapticSource = GetComponent<HapticSource>();
        ApplySettings();
    }

    private void Start()
    {
        if (playOnStart)
        {
            Play();
        }
    }

    private void OnValidate()
    {
        m_hapticSource = GetComponent<HapticSource>();
        ApplySettings();
    }

    public override void Play()
    {
        Play(controller);
    }

    public void PlayLeft()
    {
        Play(Controller.Left);
    }

    public void PlayRight()
    {
        Play(Controller.Right);
    }

    public void PlayBoth()
    {
        Play(Controller.Both);
    }

    public void Play(Controller targetController)
    {
        if (m_hapticSource == null)
        {
            m_hapticSource = GetComponent<HapticSource>();
        }

        if (m_hapticSource == null)
        {
            Debug.LogError("[HapticSourcePlayer] Missing HapticSource component.", this);
            return;
        }

        controller = targetController;
        ApplySettings();
        m_hapticSource.Play();
    }

    public override void Stop()
    {
        m_hapticSource?.Stop();
    }

    public void SetAmplitude(float value)
    {
        amplitude = Mathf.Clamp(value, 0f, 5f);
        ApplySettings();
    }

    public void SetFrequencyShift(float value)
    {
        frequencyShift = Mathf.Clamp(value, -1f, 1f);
        ApplySettings();
    }

    private void ApplySettings()
    {
        if (m_hapticSource == null)
        {
            return;
        }

        m_hapticSource.controller = controller;
        m_hapticSource.amplitude = amplitude;
        m_hapticSource.frequencyShift = frequencyShift;
    }
}
