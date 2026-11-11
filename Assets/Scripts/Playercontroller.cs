
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] float gravity = -18f;
    [SerializeField] float crouchSpeedMultiplier = 0.5f;
    [SerializeField] float crouchHeight = 1.2f;
    [SerializeField] float crouchCameraHeight = 0.9f;
    [SerializeField] float standCameraHeight = 1.6f;
    [SerializeField] float crouchLerpSpeed = 10f;

    [Header("Look")]
    [SerializeField] float mouseSensitivity = 0.12f;
    [SerializeField] float minPitch = -80f;
    [SerializeField] float maxPitch = 80f;
    [FormerlySerializedAs("playerCamera")]
    [SerializeField] Transform cameraTransform;

    [Header("Interaction")]
    [SerializeField] Transform holdPoint;
    [SerializeField] float pickupRange = 3f;
    [SerializeField] InteractionRaycaster interactionRaycaster;
    [SerializeField] InventorySystem inventory;

    [Header("Pickup and Drop")]
    [SerializeField] float pickupDistance = 3f;
    [SerializeField] AudioSource pickupDropAudio;
    [SerializeField] AudioClip pickupSound;
    [SerializeField] AudioClip dropSound;

    Rigidbody heldObject;

    [Header("Stage 1 convenience")]
    [SerializeField] bool spawnFallbackGreybox = true;
    [SerializeField] bool lockCursorOnStart = true;

    CharacterController controller;
    PlayerInputActions inputActions;
    HeadBob headBob;

    Vector2 moveInput;
    Vector2 lookInput;
    bool sprintHeld;
    bool crouchHeld;
    float pitch;
    float verticalVelocity;
    float standingHeight;
    Vector3 standingCenter;
    float cameraHeightCurrent;
    bool controlEnabled = true;

    public bool ControlEnabled
    {
        get => controlEnabled;
        set => controlEnabled = value;
    }

    public bool IsGrounded => controller != null && controller.isGrounded;
    public bool IsSprinting => sprintHeld && !crouchHeld && IsMoving;
    public bool IsMoving => moveInput.sqrMagnitude > 0.01f;
    public Transform CameraTransform => cameraTransform;
    public Transform HoldPoint => holdPoint;
    public InventorySystem Inventory => inventory;

    public float MouseSensitivity
    {
        get => mouseSensitivity >= 1f ? mouseSensitivity * 0.06f : mouseSensitivity;
        set => mouseSensitivity = Mathf.Clamp(value, 0.01f, 0.99f);
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        standingHeight = controller.height;
        standingCenter = controller.center;
        cameraHeightCurrent = standCameraHeight;
        inputActions = new PlayerInputActions();

        ResolveCamera();
        EnsureHoldPoint();
        EnsureInteraction();
        EnsureInventory();
        EnsureHeadBobAndFootsteps();
        EnsureGameUi();

        SystemsBootstrap.Ensure(this);

        if (spawnFallbackGreybox)
        {
            Stage1Greybox.Ensure(this);
        }
    }

    void OnEnable()
    {
        if (inputActions == null) return;

        inputActions.Enable();

        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;
        inputActions.Player.Interact.performed += OnInteract;
        inputActions.Player.Sprint.performed += OnSprint;
        inputActions.Player.Sprint.canceled += OnSprint;
        inputActions.Player.Crouch.performed += OnCrouch;
        inputActions.Player.Crouch.canceled += OnCrouch;
        inputActions.Player.UseItem.performed += OnUseItem;
        inputActions.Player.Hotbar1.performed += OnHotbar;
        inputActions.Player.Hotbar2.performed += OnHotbar;
        inputActions.Player.Hotbar3.performed += OnHotbar;
        inputActions.Player.Hotbar4.performed += OnHotbar;
        inputActions.Player.Hotbar5.performed += OnHotbar;
    }

    void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Player.Move.performed -= OnMove;
            inputActions.Player.Move.canceled -= OnMove;
            inputActions.Player.Look.performed -= OnLook;
            inputActions.Player.Look.canceled -= OnLook;
            inputActions.Player.Interact.performed -= OnInteract;
            inputActions.Player.Sprint.performed -= OnSprint;
            inputActions.Player.Sprint.canceled -= OnSprint;
            inputActions.Player.Crouch.performed -= OnCrouch;
            inputActions.Player.Crouch.canceled -= OnCrouch;
            inputActions.Player.UseItem.performed -= OnUseItem;
            inputActions.Player.Hotbar1.performed -= OnHotbar;
            inputActions.Player.Hotbar2.performed -= OnHotbar;
            inputActions.Player.Hotbar3.performed -= OnHotbar;
            inputActions.Player.Hotbar4.performed -= OnHotbar;
            inputActions.Player.Hotbar5.performed -= OnHotbar;
            inputActions.Disable();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnDestroy()
    {
        inputActions?.Dispose();
    }

    void Start()
    {
        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (!controlEnabled)
        {
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
        }

        Look();
        Move();
        UpdateCrouch();
        TickInteraction();
    }

    void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    void OnSprint(InputAction.CallbackContext context)
    {
        sprintHeld = context.ReadValueAsButton();
    }

    void OnCrouch(InputAction.CallbackContext context)
    {
        crouchHeld = context.ReadValueAsButton();
    }

    // E picks up an object or drops the currently held object.
    void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed || !controlEnabled)
            return;

        // If already holding an object, E drops it.
        if (heldObject != null)
        {
            DropObject();
            return;
        }

        if (cameraTransform == null || holdPoint == null)
            return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance))
        {
            Rigidbody rb = hit.collider.GetComponentInParent<Rigidbody>();

            // Pick up only objects tagged Pickup.
            if (rb != null && rb.CompareTag("Pickup"))
            {
                PickupObject(rb);
                return;
            }
        }

        // If no pickup object was found, use the existing interaction system.
        InteractionContext interaction = BuildContext();

        if (interactionRaycaster != null)
        {
            if (!interactionRaycaster.TryInteract(interaction))
            {
                Debug.Log("[PlayerController] Interact pressed, nothing in range.", this);
            }
        }
        else
        {
            Debug.LogWarning("[PlayerController] InteractionRaycaster is missing.", this);
        }
    }

    void PickupObject(Rigidbody rb)
    {
        if (rb == null || holdPoint == null)
            return;

        heldObject = rb;

        heldObject.transform.SetParent(holdPoint);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        heldObject.isKinematic = true;
        heldObject.useGravity = false;

        heldObject.linearVelocity = Vector3.zero;
        heldObject.angularVelocity = Vector3.zero;

        if (pickupDropAudio != null && pickupSound != null)
        {
            pickupDropAudio.PlayOneShot(pickupSound);
        }

        Debug.Log("[PlayerController] Object picked up.", this);
    }

    void DropObject()
    {
        if (heldObject == null)
            return;

        Rigidbody objectToDrop = heldObject;

        heldObject = null;

        objectToDrop.transform.SetParent(null);
        objectToDrop.isKinematic = false;
        objectToDrop.useGravity = true;

        if (pickupDropAudio != null && dropSound != null)
        {
            pickupDropAudio.PlayOneShot(dropSound);
        }

        Debug.Log("[PlayerController] Object dropped.", this);
    }

    void OnUseItem(InputAction.CallbackContext context)
    {
        if (!context.performed || !controlEnabled)
            return;

        if (interactionRaycaster == null)
        {
            Debug.LogWarning("[PlayerController] InteractionRaycaster is missing.", this);
            return;
        }

        interactionRaycaster.TryUseSelectedItem(BuildContext());
    }

    void OnHotbar(InputAction.CallbackContext context)
    {
        if (!context.performed || !controlEnabled || inventory == null)
            return;

        string actionName = context.action.name;

        if (actionName.Length > 6 &&
            int.TryParse(actionName.Substring(6), out int slot))
        {
            inventory.SelectSlot(slot - 1);
        }
    }

    void Move()
    {
        if (controller == null)
            return;

        float speed = moveSpeed;

        if (sprintHeld && !crouchHeld)
        {
            speed = sprintSpeed;
        }
        else if (crouchHeld)
        {
            speed *= crouchSpeedMultiplier;
        }

        Vector3 planar = transform.right * moveInput.x +
                         transform.forward * moveInput.y;

        if (planar.sqrMagnitude > 1f)
            planar.Normalize();

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 velocity = planar * speed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    void Look()
    {
        if (cameraTransform == null || !controlEnabled)
            return;

        float lookScale = mouseSensitivity >= 1f
            ? mouseSensitivity * 0.06f
            : mouseSensitivity;

        float yaw = lookInput.x * lookScale;

        pitch -= lookInput.y * lookScale;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.Rotate(Vector3.up * yaw);
        cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    void UpdateCrouch()
    {
        if (controller == null || cameraTransform == null)
            return;

        float targetHeight = crouchHeld ? crouchHeight : standingHeight;
        float targetCamY = crouchHeld ? crouchCameraHeight : standCameraHeight;

        controller.height = Mathf.Lerp(
            controller.height,
            targetHeight,
            crouchLerpSpeed * Time.deltaTime);

        Vector3 center = standingCenter;
        center.y = controller.height * 0.5f;
        controller.center = center;

        cameraHeightCurrent = Mathf.Lerp(
            cameraHeightCurrent,
            targetCamY,
            crouchLerpSpeed * Time.deltaTime);

        Vector3 rest = new Vector3(0f, cameraHeightCurrent, 0f);

        if (headBob != null)
        {
            headBob.SetRestLocalPosition(rest);
        }
        else
        {
            Vector3 camLocal = cameraTransform.localPosition;
            camLocal.y = cameraHeightCurrent;
            cameraTransform.localPosition = camLocal;
        }
    }

    void TickInteraction()
    {
        if (interactionRaycaster == null)
            return;

        interactionRaycaster.Tick(BuildContext());
    }

    InteractionContext BuildContext()
    {
        return new InteractionContext(
            this,
            cameraTransform,
            holdPoint,
            inventory);
    }

    void ResolveCamera()
    {
        if (cameraTransform == null)
        {
            Camera childCam = GetComponentInChildren<Camera>();

            if (childCam != null)
            {
                cameraTransform = childCam.transform;
            }
            else
            {
                Debug.LogWarning(
                    "[PlayerController] Camera Transform is not assigned on " +
                    name + ".", this);
            }
        }
    }

    void EnsureHoldPoint()
    {
        if (holdPoint != null)
            return;

        Transform existing = transform.Find("HoldPoint");

        if (existing != null)
        {
            holdPoint = existing;
            return;
        }

        if (cameraTransform == null)
            return;

        GameObject point = new GameObject("HoldPoint");
        point.transform.SetParent(cameraTransform);
        point.transform.localPosition = new Vector3(0.35f, -0.2f, 0.6f);
        point.transform.localRotation = Quaternion.identity;

        holdPoint = point.transform;
    }

    void EnsureInteraction()
    {
        if (interactionRaycaster == null)
            interactionRaycaster = GetComponent<InteractionRaycaster>();

        if (interactionRaycaster == null)
            interactionRaycaster = gameObject.AddComponent<InteractionRaycaster>();

        interactionRaycaster.RayOrigin = cameraTransform;
        interactionRaycaster.Range = pickupRange;

        InteractionHUD hud = FindFirstObjectByType<InteractionHUD>();

        if (hud == null)
        {
            GameObject hudObject = new GameObject("InteractionHUD");
            hud = hudObject.AddComponent<InteractionHUD>();
        }

        hud.Raycaster = interactionRaycaster;
    }

    void EnsureInventory()
    {
        if (inventory == null)
            inventory = GetComponent<InventorySystem>();

        if (inventory == null)
            inventory = gameObject.AddComponent<InventorySystem>();

        InteractionHUD hud = FindFirstObjectByType<InteractionHUD>();

        if (hud == null)
            return;

        HotbarHUD hotbar = hud.GetComponent<HotbarHUD>();

        if (hotbar == null)
            hotbar = hud.gameObject.AddComponent<HotbarHUD>();

        hotbar.Inventory = inventory;
    }

    void EnsureHeadBobAndFootsteps()
    {
        if (cameraTransform == null)
            return;

        headBob = cameraTransform.GetComponent<HeadBob>();

        if (headBob == null)
            headBob = cameraTransform.gameObject.AddComponent<HeadBob>();

        if (GetComponent<FootstepEmitter>() == null)
            gameObject.AddComponent<FootstepEmitter>();
    }

    void EnsureGameUi()
    {
        if (GetComponent<ClueJournal>() == null)
            gameObject.AddComponent<ClueJournal>();

        if (GetComponent<ObjectiveTracker>() == null)
            gameObject.AddComponent<ObjectiveTracker>();

        InteractionHUD hud = FindFirstObjectByType<InteractionHUD>();

        if (hud == null)
            return;

        GameMenu menu = FindFirstObjectByType<GameMenu>();

        if (menu == null)
            menu = new GameObject("GameMenu").AddComponent<GameMenu>();

        menu.Bind(this);

        GameHUD gameHud = hud.GetComponent<GameHUD>();

        if (gameHud == null)
            gameHud = hud.gameObject.AddComponent<GameHUD>();

        gameHud.Bind(this, menu);
    }
}
