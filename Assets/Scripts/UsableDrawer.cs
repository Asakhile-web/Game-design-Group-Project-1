
using UnityEngine;

public class UsableDrawer : MonoBehaviour
{
    [Header("Drawer Settings")]
    public float openDistance = 0.4f;
    public float openSpeed = 3f;

    [Header("Movement Direction")]
    public Transform cabinet;

    [Header("Audio")]
    public AudioSource drawerAudio;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private bool isOpen;
    private bool isMoving;

    private void Start()
    {
        closedPosition = transform.position;

        if (cabinet != null)
        {
            // Move towards the front of the cabinet
            openPosition = closedPosition - cabinet.forward * openDistance;
        }
        else if (transform.parent != null)
        {
            openPosition = closedPosition - transform.parent.forward * openDistance;
        }
        else
        {
            openPosition = closedPosition - transform.forward * openDistance;
        }

        if (drawerAudio == null)
        {
            drawerAudio = GetComponent<AudioSource>();
        }
    }

    public void Use()
    {
        if (isMoving)
            return;

        isOpen = !isOpen;
        isMoving = true;

        if (drawerAudio != null)
        {
            drawerAudio.Play();
        }
    }

    private void Update()
    {
        if (!isMoving)
            return;

        Vector3 targetPosition = isOpen ? openPosition : closedPosition;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            openSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            transform.position = targetPosition;
            isMoving = false;
        }
    }
}
