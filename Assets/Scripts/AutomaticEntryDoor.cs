
using UnityEngine;

public class AutomaticEntryDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] float closeAngle = 90f;
    [SerializeField] float closeSpeed = 2f;

    [Header("Lock Connection")]
    [SerializeField] LockedOpenable doorLock;

    Quaternion openRotation;
    Quaternion closedRotation;
    bool shouldClose;

    void Start()
    {
        openRotation = transform.localRotation;
        closedRotation = openRotation * Quaternion.Euler(0f, closeAngle, 0f);
    }

    public void CloseDoor()
    {
        shouldClose = true;
    }

    void Update()
    {
        if (!shouldClose)
            return;

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            closedRotation,
            closeSpeed * Time.deltaTime
        );

        if (Quaternion.Angle(transform.localRotation, closedRotation) < 0.5f)
        {
            transform.localRotation = closedRotation;
            shouldClose = false;

            if (doorLock != null)
            {
                doorLock.SetClosedPoseNow();
            }
        }
    }
}