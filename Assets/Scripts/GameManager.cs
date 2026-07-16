using System;
using UnityEngine;

/// <summary>
/// Controls the overall fire-escape game flow.
///
/// Responsibilities:
/// - Starts the game.
/// - Progresses through the fire phases.
/// - Tracks the escape timer.
/// - Detects win and lose outcomes.
/// - Notifies other systems when the game state changes.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        WaitingToStart,
        Escaping,
        Won,
        Lost
    }

    [Header("Game Timing")]
    [SerializeField, Min(1f)]
    private float escapeTime = 120f;


    [Header("Optional Scene References")]
    [SerializeField]
    private GameObject startInstructions;

    [SerializeField]
    private GameObject winScreen;

    [SerializeField]
    private GameObject loseScreen;

    public GameState CurrentState { get; private set; } = GameState.WaitingToStart;

    /// <summary>
    /// Remaining time before the player loses.
    /// </summary>
    public float TimeRemaining { get; private set; }

    /// <summary>
    /// Time elapsed since the game started.
    /// </summary>
    public float TimeElapsed => escapeTime - TimeRemaining;

    public bool IsGameRunning =>
        CurrentState == GameState.Escaping;

    public event Action<GameState> OnGameStateChanged;
    public event Action<float> OnTimerUpdated;
    public event Action OnGameStarted;
    public event Action OnGameWon;
    public event Action OnGameLost;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple GameManagers found. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        PrepareGame();
    }

    private void Update()
    {
        if (!IsGameRunning)
        {
            return;
        }

        UpdateTimer();
    }

    /// <summary>
    /// Resets the game to its initial waiting state.
    /// </summary>
    public void PrepareGame()
    {
        TimeRemaining = escapeTime;

        SetState(GameState.WaitingToStart);

        SetActiveSafely(startInstructions, true);
        SetActiveSafely(winScreen, false);
        SetActiveSafely(loseScreen, false);

        OnTimerUpdated?.Invoke(TimeRemaining);
    }

    /// <summary>
    /// Begins the fire escape scenario.
    /// Can be connected to a UI button or called by another script.
    /// </summary>
    public void StartGame()
    {
        if (CurrentState != GameState.WaitingToStart)
            return;

        TimeRemaining = escapeTime;

        SetActiveSafely(startInstructions, false);
        SetActiveSafely(winScreen, false);
        SetActiveSafely(loseScreen, false);

        SetState(GameState.Escaping);

        OnTimerUpdated?.Invoke(TimeRemaining);
        OnGameStarted?.Invoke();
    }

    /// <summary>
    /// Call this when the player reaches the exit.
    /// </summary>
    public void WinGame()
    {
        if (!IsGameRunning)
        {
            return;
        }

        SetState(GameState.Won);
        SetActiveSafely(winScreen, true);

        OnGameWon?.Invoke();

        Debug.Log("Player escaped successfully.");
    }

    /// <summary>
    /// Ends the game in failure.
    /// </summary>
    public void LoseGame()
    {
        if (!IsGameRunning)
        {
            return;
        }

        TimeRemaining = 0f;

        SetState(GameState.Lost);
        SetActiveSafely(loseScreen, true);

        OnTimerUpdated?.Invoke(TimeRemaining);
        OnGameLost?.Invoke();

        Debug.Log("The escape timer expired.");
    }

    /// <summary>
    /// Restarts the current scene state without reloading the scene.
    /// Individual systems should reset themselves when the state returns
    /// to WaitingToStart.
    /// </summary>
    public void RestartGame()
    {
        PrepareGame();
    }

    private void UpdateTimer()
    {
        TimeRemaining -= Time.deltaTime;
        TimeRemaining = Mathf.Max(TimeRemaining, 0f);

        OnTimerUpdated?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0f)
        {
            LoseGame();
        }
    }

    private void SetState(GameState newState)
    {
        if (CurrentState == newState)
        {
            return;
        }

        CurrentState = newState;

        Debug.Log($"Game state changed to: {CurrentState}");
        OnGameStateChanged?.Invoke(CurrentState);
    }

    private static void SetActiveSafely(GameObject target, bool isActive)
    {
        if (target != null)
        {
            target.SetActive(isActive);
        }
    }
}