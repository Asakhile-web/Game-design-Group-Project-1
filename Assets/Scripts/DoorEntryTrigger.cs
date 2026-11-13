
using UnityEngine;

public class DoorEntryTrigger : MonoBehaviour
{
    [SerializeField] AutomaticEntryDoor door;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered the door trigger: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player detected! Closing door.");

            if (door != null)
                door.CloseDoor();
        }
    }
}
