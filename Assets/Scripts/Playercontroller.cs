
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
    public AudioClip dropSound;

    [Header("Footstep Audio")]
    public AudioSource footstepAudio;
    public float footstepInterval = 0.5f;
    [Range(0f, 1f)]
    public float footstepVolume = 0.5f;

    private float footstepTimer;

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

        if (footstepAudio != null)
        {
            footstepAudio.playOnAwake = false;
            footstepAudio.loop = false;
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
        if (inputActions == null)
            return;

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
        HandleFootsteps();
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
        if (cameraTransform == null)
            return;

        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);
        cameraTransform.Rotate(Vector3.left * mouseY);
    }

    private void HandleFootsteps()
    {
        if (footstepAudio == null || footstepAudio.clip == null)
            return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        bool isGrounded = controller.isGrounded;

        if (isMoving && isGrounded)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                footstepAudio.PlayOneShot(
                    footstepAudio.clip,
                    footstepVolume
                );

                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
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
        if (heldObject == null)
            return;

        if (pickupAudio != null && dropSound != null)
        {
            pickupAudio.PlayOneShot(dropSound);
        }

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
