using UnityEngine;

/// <summary>
/// Simple camera follow script for 2D game
/// Attach to Main Camera in the scene (not as child of player)
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // Assign player in inspector

    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
    [SerializeField] private bool followOnStart = true;

    private void Start()
    {
        if (followOnStart && target == null)
        {
            // Auto-find player
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    /// <summary>
    /// Set the target to follow
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}