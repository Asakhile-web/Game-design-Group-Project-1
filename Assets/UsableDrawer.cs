using UnityEngine;

public class UsableDrawer : MonoBehaviour
{
    public float openDistance = 0.5f;
    public float openSpeed = 3f;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;

    private void Start()
    {
        closedPosition = transform.localPosition;
        openPosition = closedPosition - Vector3.forward * openDistance;
    }

    public void Use()
    {
        isOpen = !isOpen;
    }

    private void Update()
    {
        Vector3 targetPosition = isOpen ? openPosition : closedPosition;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            openSpeed * Time.deltaTime
        );
    }
}