using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
 {
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float interactionDistance = 3f;

    public Transform playerCamera;

    private CharacterController controller;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction interAction;

    private float xRotation = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        PlayerInputActions input = new PlayerInputActions();

        moveAction = input.Player.Move;
        lookAction = input.Player.Look;
        interAction = input.Player.Interact;


        input.Player.Enable();

        if (interAction.WasPressedThisFrame())
        {
            Ray ray = new Ray(playerCamera.position, playerCamera.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionDistance))
            {
                Interactable interactable = hit.collider.GetComponent<Interactable>();

                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }
    }

    void Update()
    {
        Move();
        Look();

    if (interAction.WasPressedThisFrame())
        {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
          {
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable != null)
            {
                interactable.Interact();
            }
          }
        }

    }

void Move()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        Vector3 movement =
            transform.right * input.x +
            transform.forward * input.y;

        controller.Move(movement * moveSpeed * Time.deltaTime);
    }

    void Look()
    {
        Vector2 mouse = lookAction.ReadValue<Vector2>();

        float mouseX = mouse.x * mouseSensitivity;
        float mouseY = mouse.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation =
            Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }
}