
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Look")]
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    [Header("Pickup")]
    public Transform holdPoint;
    public float pickupRange = 3f;

    private CharacterController controller;
    private PlayerInputActions inputActions;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;

        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;

        inputActions.Player.Interact.performed += OnInteract;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;

        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Look.canceled -= OnLook;

        inputActions.Player.Interact.performed -= OnInteract;

        inputActions.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        Move();
        Look();
    }

    private void Move()
    {
        Vector3 movement =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        controller.Move(movement * moveSpeed * Time.deltaTime);
    }

    private void Look()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        cameraTransform.Rotate(Vector3.left * mouseY);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("INTERACT BUTTON PRESSED");
        TryPickup();
    }

    private void TryPickup()
    {
        Ray ray = new Ray(
            cameraTransform.position,
            cameraTransform.forward
        );

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            if (hit.collider.CompareTag("Pickup"))
            {
                GameObject item = hit.collider.gameObject;

                item.transform.SetParent(holdPoint);

                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.identity;

                Rigidbody rb = item.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.isKinematic = true;
                }

                Debug.Log("Picked up: " + item.name);
            }
        }
    }
}