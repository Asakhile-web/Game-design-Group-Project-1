using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string interactionMessage = "You found a mysterious letter.";

    public void Interact()
    {
        Debug.Log(interactionMessage);
    }
}