using UnityEngine;

/// <summary>
/// Subtle camera head-bob driven by planar move speed. Toggle from Settings later via PlayerPrefs.
/// Attach to the player camera (or leave unassigned — PlayerController will add one).
/// </summary>
public class HeadBob : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] bool bobEnabled = true;
    [SerializeField] float walkBobAmount = 0.035f;
    [SerializeField] float sprintBobAmount = 0.05f;
    [SerializeField] float walkBobFrequency = 8f;
    [SerializeField] float sprintBobFrequency = 11f;
    [SerializeField] float returnSpeed = 8f;

    Vector3 restLocalPosition;
    float bobTimer;

    public bool BobEnabled
    {
        get => bobEnabled;
        set => bobEnabled = value;
    }

    void Awake()
    {
        restLocalPosition = transform.localPosition;
        if (player == null)
        {
            player = GetComponentInParent<PlayerController>();
        }
    }

    void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        bool shouldBob = bobEnabled && player.ControlEnabled && player.IsMoving && player.IsGrounded;
        if (!shouldBob)
        {
            bobTimer = 0f;
            transform.localPosition = Vector3.Lerp(transform.localPosition, restLocalPosition, returnSpeed * Time.deltaTime);
            return;
        }

        float amount = player.IsSprinting ? sprintBobAmount : walkBobAmount;
        float frequency = player.IsSprinting ? sprintBobFrequency : walkBobFrequency;
        bobTimer += Time.deltaTime * frequency;
        float offsetY = Mathf.Sin(bobTimer) * amount;
        float offsetX = Mathf.Cos(bobTimer * 0.5f) * amount * 0.5f;
        transform.localPosition = restLocalPosition + new Vector3(offsetX, offsetY, 0f);
    }

    /// <summary>
    /// Call after crouch/stand changes the camera rest height.
    /// </summary>
    public void SetRestLocalPosition(Vector3 localPosition)
    {
        restLocalPosition = localPosition;
    }
}
