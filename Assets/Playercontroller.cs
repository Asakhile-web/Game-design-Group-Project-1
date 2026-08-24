using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;

    public Transform playerCamera;

    private CharacterController controller;
    private InputAction moveAction;
    private InputAction lookAction;

    private float xRotation = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        PlayerInputActions input = new PlayerInputActions();

        moveAction = input.Player.Move;
        lookAction = input.Player.Look;

        input.Player.Enable();
    }

    void Update()
    {
        Move();
        Look();
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