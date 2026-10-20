
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
            useAction.action.performed += OnUse;
            useAction.action.Enable();
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
        Debug.Log("F KEY PRESSED");

        if (cameraTransform == null)
        {
            Debug.LogError("Camera Transform is not assigned!");
            return;
        }

        Ray ray = new Ray(
            cameraTransform.position,
            cameraTransform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, useRange))
        {
            Debug.Log("F is hitting: " + hit.collider.name);

            UsableDrawer drawer =
                hit.collider.GetComponentInParent<UsableDrawer>();

            if (drawer != null)
            {
                drawer.Use();
                Debug.Log("USED DRAWER: " + drawer.gameObject.name);
            }
            else
            {
                Debug.Log("No UsableDrawer found on the hit object or its parents.");
            }
        }
        else
        {
            Debug.Log("F raycast hit nothing. Look closer at the drawer.");
        }
    }
}
