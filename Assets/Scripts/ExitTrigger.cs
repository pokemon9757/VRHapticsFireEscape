using UnityEngine;

/// <summary>
/// Wins the game when the VR player enters the exit trigger.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ExitTrigger : MonoBehaviour
{
    [SerializeField]
    private string playerTag = "Player";

    private bool hasTriggered;

    private void Reset()
    {
        Collider triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag(playerTag))
        {
            return;
        }

        hasTriggered = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.WinGame();
        }
    }
}