using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Movement settings")]
    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float VerticalSpeed = 3.0f;

    [Header("Mouse settings")]
    [SerializeField] private float lookSensitivity = 0.25f;

    private Camera cam;
    private FlyCamActions inputActions;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 verticalInput;

    private float xRotation;
    private float yRotation;
    private bool firstMouse = true;
    private void Awake()
    {
        cam = Camera.main;
    }

    private void OnEnable()
    {
        inputActions = new FlyCamActions();
        inputActions.Enable();

        inputActions.FlyCamMap.Move.performed += IAOnMove;
        inputActions.FlyCamMap.Move.canceled += IAOnMove;

        inputActions.FlyCamMap.Look.performed += IAOnLook;
        inputActions.FlyCamMap.Look.canceled += IAOnLook;

        inputActions.FlyCamMap.MoveUp.performed += IAOnMoveUp;
        inputActions.FlyCamMap.MoveUp.canceled += IAOnMoveUp;

        inputActions.FlyCamMap.MoveDown.performed += IAOnMoveDown;
        inputActions.FlyCamMap.MoveDown.canceled += IAOnMoveDown;

        inputActions.FlyCamMap.Exit.performed += IAOnExit;
        inputActions.FlyCamMap.Exit.canceled += IAOnExit;
    }

    private void OnDisable()
    {
        inputActions.Dispose();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        xRotation = transform.localEulerAngles.x;
        yRotation = transform.localEulerAngles.y;

    }

    // Update is called once per frame
    void Update()
    {
        MoveCamera();
        LookCamera();
    }

    public void IAOnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void IAOnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
        if (firstMouse)
        {
            firstMouse = false;
            lookInput = Vector2.zero;
        }
        
    }

    public void IAOnMoveUp(InputAction.CallbackContext context)
    {
        verticalInput = context.performed ? Vector3.up : Vector3.zero;
    }

    public void IAOnMoveDown(InputAction.CallbackContext context)
    {
        verticalInput = context.performed ? Vector3.down : Vector3.zero;
    }

    public void IAOnExit(InputAction.CallbackContext context)
    {
        Application.Quit();
    }

    private void MoveCamera()
    {
        float dt = Time.deltaTime;
        Vector3 forwardMovement = transform.forward * moveInput.y;
        Vector3 rightMovement = Vector3.Normalize(transform.right) * moveInput.x;

        transform.position += dt * speed * (Vector3.Normalize(forwardMovement + rightMovement) + verticalInput);
    }

    private void LookCamera()
    {
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        yRotation += mouseX;
        xRotation = Mathf.Clamp(xRotation - mouseY, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
