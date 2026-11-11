
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class UseInteraction : MonoBehaviour
{
    [Header("Interaction")]
    public InputActionReference useAction;
    public Transform cameraTransform;
    public float useRange = 3f;

    [Header("Interaction Prompt")]
    public TextMeshProUGUI interactionPrompt;

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

        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        HidePrompt();
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        CheckForDrawer();
    }

    private void CheckForDrawer()
    {
        if (cameraTransform == null || interactionPrompt == null)
            return;

        Ray ray = new Ray(
            cameraTransform.position,
            cameraTransform.forward
        );

        bool lookingAtDrawer = false;

        if (Physics.Raycast(ray, out RaycastHit hit, useRange))
        {
            UsableDrawer drawer =
                hit.collider.GetComponentInParent<UsableDrawer>();

            if (drawer != null)
            {
                lookingAtDrawer = true;
            }
        }

        interactionPrompt.gameObject.SetActive(lookingAtDrawer);
    }

    private void OnUse(InputAction.CallbackContext context)
    {
        if (cameraTransform == null)
            return;

        Ray ray = new Ray(
            cameraTransform.position,
            cameraTransform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, useRange))
        {
            UsableDrawer drawer =
                hit.collider.GetComponentInParent<UsableDrawer>();

            if (drawer != null)
            {
                drawer.Use();

                Debug.Log("USED DRAWER: " + drawer.gameObject.name);
            }
        }
    }

    private void HidePrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(false);
        }
    }
}
