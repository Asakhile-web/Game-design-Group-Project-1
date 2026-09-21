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
    private float xRotation = 0f;
    private GameObject heldObject;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();
    }
    void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Interact.performed += OnInteract;
    }
    void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Interact.performed -= OnInteract;
        inputActions.Player.Disable();
    }
    void OnMove(InputAction.CallbackContext c) { moveInput = c.ReadValue<Vector2>(); }
    void Update()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * moveSpeed * Time.deltaTime);
        Vector2 mouse = Mouse.current.delta.ReadValue();
        float mouseX = mouse.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouse.y * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
    void OnInteract(InputAction.CallbackContext context)
    {
        if (heldObject == null) TryPickup(); else DropObject();
    }
    void TryPickup()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
            if (hit.collider.CompareTag("Pickup")) PickupObject(hit.collider.gameObject);
    }
    void PickupObject(GameObject o)
    {
        heldObject = o;
        heldObject.transform.SetParent(holdPoint);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;
        var rb = heldObject.GetComponent<Rigidbody>(); if (rb) rb.isKinematic = true;
    }
    void DropObject()
    {
        heldObject.transform.SetParent(null);
        var rb = heldObject.GetComponent<Rigidbody>(); if (rb) rb.isKinematic = false;
        heldObject = null;
    }
}