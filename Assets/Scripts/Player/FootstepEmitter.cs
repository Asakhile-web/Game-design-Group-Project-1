using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Fires a footstep event at a cadence based on move speed. Teammates hook AudioManager here later.
/// </summary>
public class FootstepEmitter : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] float walkInterval = 0.48f;
    [SerializeField] float sprintInterval = 0.32f;
    [SerializeField] UnityEvent onFootstep;

    float timer;

    public event System.Action Footstep;

    void Awake()
    {
        if (player == null)
        {
            player = GetComponent<PlayerController>();
        }

        if (player == null)
        {
            Debug.LogWarning("[FootstepEmitter] Missing PlayerController on " + name + ".", this);
        }
    }

    void Update()
    {
        if (player == null || !player.ControlEnabled || !player.IsMoving || !player.IsGrounded)
        {
            timer = 0f;
            return;
        }

        float interval = player.IsSprinting ? sprintInterval : walkInterval;
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            onFootstep?.Invoke();
            Footstep?.Invoke();
        }
    }
}
