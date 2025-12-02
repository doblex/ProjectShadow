using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class ActionManager : MonoBehaviour
{
    public static ActionManager Instance;

    public delegate void OnMovementChanged(Vector3 movement);
    public delegate void OnRotationChanged(Quaternion rotation);
    public delegate void OnInteract();
    public delegate void OnPlayerMovement(Vector2 mousePos, bool dash);

    public OnMovementChanged onMovementChanged;
    public OnRotationChanged onRotationChanged;
    public OnInteract onInteract;
    public OnPlayerMovement onPlayerMovement;

    [SerializeField] private PlayerOptions options;
    [SerializeField] private GameObject CameraPrefab;

    private InputAction moveVisual;
    private InputAction rotateVisual;

    private InputAction InteractAction;

    private InputAction PlayerMovementAction;
    private InputAction PlayerDashAction;
    private InputAction mousePositionAction;


    Coroutine MovementCoroutine;
    Coroutine RotationCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    private void Start()
    {
        SetupCommands();

        if (CameraPrefab != null)
        { 
            CameraPrefab.SetActive(true);
        }
    }

    private void SetupCommands()
    {
        moveVisual = InputSystem.actions.FindAction("MoveCamera");
        moveVisual.performed += OnMoveDown;
        moveVisual.canceled += OnMoveUp;

        rotateVisual = InputSystem.actions.FindAction("RotateCamera");
        rotateVisual.performed += OnRotateDown;
        rotateVisual.canceled += OnRotateUp;

        InteractAction = InputSystem.actions.FindAction("Interact");
        InteractAction.performed += OnInteractInput;

        PlayerMovementAction = InputSystem.actions.FindAction("MovePlayer");
        PlayerMovementAction.performed += OnMovePlayer;

        mousePositionAction = InputSystem.actions.FindAction("MousePosition");
    }

    // -------------------------------
    //   CALLBACKS
    // -------------------------------

    private void OnMoveDown(InputAction.CallbackContext ctx)
    {
        Vector2 movement2D = ctx.ReadValue<Vector2>();
        Vector3 converted = new Vector3(movement2D.x, 0, movement2D.y);

        if (MovementCoroutine != null)
        {
            StopCoroutine(MovementCoroutine);
        }
        MovementCoroutine = StartCoroutine(OnMove(converted));
    }

    private void OnMoveUp(InputAction.CallbackContext ctx)
    {
        Vector2 movement2D = ctx.ReadValue<Vector2>();
        Vector3 converted = new Vector3(movement2D.x, 0, movement2D.y);

        if (MovementCoroutine != null)
        { 
            StopCoroutine(MovementCoroutine);
        }

        MovementCoroutine = StartCoroutine(OnMove(converted));
    }

    private IEnumerator OnMove(Vector3 movement)
    {
        while (true)
        {
            Vector3 move = movement * Time.deltaTime * options.MoveSpeed;
            onMovementChanged?.Invoke(move);
            yield return null;
        }
    }



    private void OnRotateDown(InputAction.CallbackContext ctx)
    {
        float rotationValue = ctx.ReadValue<float>();


        if (RotationCoroutine != null)
        {
            StopCoroutine(RotationCoroutine);
        }

        RotationCoroutine = StartCoroutine(OnRotate(rotationValue));
    }

    private void OnRotateUp(InputAction.CallbackContext ctx)
    {
        float rotationValue = ctx.ReadValue<float>();


        if (RotationCoroutine != null)
        {
            StopCoroutine(RotationCoroutine);
        }

        RotationCoroutine = StartCoroutine(OnRotate(rotationValue));
    }

    private IEnumerator OnRotate(float rotation)
    {
        while (true)
        {
            Quaternion turn = Quaternion.AngleAxis(
            rotation * Time.deltaTime * options.AngleSpeed,
            Vector3.up
        );

            onRotationChanged?.Invoke(turn);
            yield return null;
        }
    }

    private void OnInteractInput(InputAction.CallbackContext ctx)
    {
        onInteract?.Invoke();
    }

    private void OnMovePlayer(InputAction.CallbackContext ctx)
    {
        Vector2 v = mousePositionAction.ReadValue<Vector2>();
        bool isDoubleClick = false;

        if (ctx.interaction is MultiTapInteraction)
        { 
            isDoubleClick = true; 
        }

        if (isDoubleClick)
        {
            Debug.Log("Dash Move");
        }
        else
        {
            Debug.Log("Normal Move");
        }

        onPlayerMovement?.Invoke(v, isDoubleClick);
    }
}
