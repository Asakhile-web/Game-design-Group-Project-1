
using UnityEngine;

public class DoorEntryTrigger : MonoBehaviour
{
    [SerializeField] AutomaticEntryDoor door;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (door != null)
        {
            door.CloseDoor();
        }
    }
}
