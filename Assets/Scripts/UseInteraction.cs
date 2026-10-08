using UnityEngine;
using UnityEngine.InputSystem;

public class UseInteraction : MonoBehaviour
{
    [Header("Interaction")]
    public InputActionReference useAction;
    public Transform cameraTransform;
    public float useRange = 3f;

    private void OnEnable()
    {
        if (useAction != null)
        {
            useAction.action.Enable();
            useAction.action.performed += OnUse;
        }
    }

    private void OnDisable()
    {
        if (useAction != null)
        {
            useAction.action.performed -= OnUse;
            useAction.action.Disable();
        }
    }

    private void OnUse(InputAction.CallbackContext context)
    {
        Ray ray = new Ray(
            cameraTransform.position,
            cameraTransform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, useRange))
        {
            UsableDrawer drawer = hit.collider.GetComponentInParent<UsableDrawer>();

            if (drawer != null)
            {
                drawer.Use();
                Debug.Log("USED: " + drawer.gameObject.name);
            }
            else
            {
                Debug.Log("This object cannot be used.");
            }
        }
    }
}