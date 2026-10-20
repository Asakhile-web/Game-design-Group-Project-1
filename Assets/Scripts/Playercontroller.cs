
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

    [Header("Pickup Audio")]
    public AudioSource pickupAudio;

    private CharacterController controller;
    private PlayerInputActions inputActions;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private GameObject heldObject;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();

        if (pickupAudio == null)
        {
            pickupAudio = GetComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;

        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;

        inputActions.Player.Interact.performed += OnInteract;

        inputActions.Enable();
    }

    private void OnDisable()
    {
<<<<<<< Updated upstream
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;

        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Look.canceled -= OnLook;

        inputActions.Player.Interact.performed -= OnInteract;

=======
        if (inputActions == null)
            return;

        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;

        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Look.canceled -= OnLook;

        inputActions.Player.Interact.performed -= OnInteract;

>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
        Debug.Log("E PRESSED");

        if (heldObject == null)
        {
            TryPickup();
        }
        else
        {
            DropObject();
        }
    }

    private void TryPickup()
{
    if (Physics.Raycast(
        cameraTransform.position,
        cameraTransform.forward,
        out RaycastHit hit,
        pickupRange))
    {
        Debug.Log("I hit: " + hit.collider.name);

        // Start with the object that was hit
        Transform pickupRoot = hit.collider.transform;

        // Keep going up through the parents until we find
        // the object tagged "Pickup"
        while (pickupRoot != null)
=======
        if (heldObject == null)
>>>>>>> Stashed changes
        {
            TryPickup();
        }
        else
        {
            DropObject();
        }
    }

    private void TryPickup()
    {
        if (Physics.Raycast(
            cameraTransform.position,
            cameraTransform.forward,
            out RaycastHit hit,
            pickupRange))
        {
            Transform pickupRoot = hit.collider.transform;

            while (pickupRoot != null)
            {
                if (pickupRoot.CompareTag("Pickup"))
                    break;

                pickupRoot = pickupRoot.parent;
            }

            if (pickupRoot != null)
            {
                heldObject = pickupRoot.gameObject;

                heldObject.transform.SetParent(holdPoint);
                heldObject.transform.localPosition = Vector3.zero;
                heldObject.transform.localRotation = Quaternion.identity;

                Rigidbody rb = heldObject.GetComponent<Rigidbody>();

                if (rb == null)
                    rb = heldObject.GetComponentInChildren<Rigidbody>();

                if (rb != null)
                    rb.isKinematic = true;

                // Play sound only when an item is picked up.
                if (pickupAudio != null)
                    pickupAudio.Play();

                Debug.Log("PICKED UP: " + heldObject.name);
            }
            else
            {
                Debug.Log("This object and its parents are not tagged Pickup.");
            }
        }
        else
        {
            Debug.Log("Hit NOTHING - get closer and look directly at the object.");
        }
    }

    private void DropObject()
    {
        heldObject.transform.SetParent(null);

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();

        if (rb == null)
            rb = heldObject.GetComponentInChildren<Rigidbody>();

        if (rb != null)
            rb.isKinematic = false;

        Debug.Log("DROPPED: " + heldObject.name);

        heldObject = null;
    }
}
