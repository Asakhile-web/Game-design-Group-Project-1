
using UnityEngine;

public class UsableDrawer : MonoBehaviour
{
    [Header("Drawer Settings")]
    public Vector3 openOffset = new Vector3(0.4f, 0f, 0f);
    public float openSpeed = 3f;

    [Header("Audio")]
    public AudioSource drawerAudio;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen;
    private bool isMoving;

    private void Start()
    {
        closedPosition = transform.localPosition;
        openPosition = closedPosition + openOffset;

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

        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition,
            targetPosition,
            openSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.localPosition, targetPosition) < 0.001f)
        {
            transform.localPosition = targetPosition;
            isMoving = false;
        }
    }
}
