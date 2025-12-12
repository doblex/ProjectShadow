using System;
using System.Collections;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using static ActionManager;

public class ActionManager : MonoBehaviour
{
    public static ActionManager Instance;

    public delegate void OnCameraMovementChanged(Vector3 movement);
    public delegate void OnCameraRotationChanged(Quaternion rotation);

    public delegate void OnPlayerMovement(Vector2 mousePos, bool dash);

    public delegate void OnInteract();
    public delegate void OnHighlight(bool active);
    public delegate void OnPlayerCrouch();

    public OnCameraMovementChanged onMovementChanged;
    public OnCameraRotationChanged onRotationChanged;

    public OnPlayerMovement onPlayerMovement;

    public OnInteract onInteract;
    public OnHighlight onHighlight;
    public OnPlayerCrouch onPlayerCrouch;

    [SerializeField] private Options Options;

    private InputAction MoveVisual;
    private bool isMoving = false;
    Coroutine playerMovementCoroutine;


    private InputAction RotateVisual;

    private InputAction InteractAction;
    private InputAction HighlightAction;
    private bool HighlightActive = false;

    private InputAction PlayerMovementAction;
    private InputAction PlayerDashAction;
    private InputAction MousePositionAction;

    private InputAction CrouchAction;

    // Skills
    public delegate void OnThrowStone();
    public delegate void OnWhistle();
    public delegate void OnThrowIBait();
    public delegate void OnRBait();
    public delegate void OnCastAbility();
    public delegate void OnCancelSkill();

    public OnThrowStone onThrowStone;
    public OnWhistle onWhistle;
    public OnThrowIBait onThrowIBait;
    public OnRBait onRBait;
    public OnCastAbility onCastAbility;
    public OnCancelSkill onCancelSkill;

    private InputAction ThrowStoneAction;
    private InputAction WhistleAction;
    private InputAction ThrowIBaitAction;
    private InputAction RBaitAction;
    private InputAction CastAbilityAction;
    private InputAction CancelSkillAction;

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
    }

    private void SetupCommands()
    {
        MoveVisual = InputSystem.actions.FindAction("MoveCamera");
        MoveVisual.performed += OnMoveDown;
        MoveVisual.canceled += OnMoveUp;

        RotateVisual = InputSystem.actions.FindAction("RotateCamera");
        RotateVisual.performed += OnRotateDown;
        RotateVisual.canceled += OnRotateUp;

        InteractAction = InputSystem.actions.FindAction("Interact");
        InteractAction.performed += OnInteractInput;

        HighlightAction = InputSystem.actions.FindAction("Highlight");
        HighlightAction.performed += OnHighlightInput;

        PlayerMovementAction = InputSystem.actions.FindAction("MovePlayer");
        PlayerMovementAction.performed += OnMovePlayer;

        MousePositionAction = InputSystem.actions.FindAction("MousePosition");

        CrouchAction = InputSystem.actions.FindAction("Crouch");
        CrouchAction.performed += OnCrouch;

        // Ability bindings
        ThrowStoneAction = InputSystem.actions.FindAction("Throw Stone");
        ThrowStoneAction.performed += OnThrowStoneCall;

        WhistleAction = InputSystem.actions.FindAction("Whistle");
        WhistleAction.performed += OnWhistleCall;

        ThrowIBaitAction = InputSystem.actions.FindAction("Throw IBait");
        ThrowIBaitAction.performed += OnThrowIBaitCall;

        RBaitAction = InputSystem.actions.FindAction("RBait");
        RBaitAction.performed += OnRBaitCall;

        CastAbilityAction = InputSystem.actions.FindAction("FireAbility");
        CastAbilityAction.performed += OnCastAbilityCall;

        CancelSkillAction = InputSystem.actions.FindAction("CancelAbility");
        CancelSkillAction.performed += OnCancelSkillCall;

        // TODO make mouse position tracking delegate for ability targeting
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
            Vector3 move = movement * Time.deltaTime * Options.MoveSpeed;
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
            rotation * Time.deltaTime * Options.AngleSpeed,
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
    private void OnHighlightInput(InputAction.CallbackContext ctx)
    {
        HighlightActive = !HighlightActive;
        onHighlight?.Invoke(HighlightActive);
    }

    private void OnMovePlayer(InputAction.CallbackContext ctx)
    {
        Vector2 v = MousePositionAction.ReadValue<Vector2>();
        bool isDoubleClick = false;

        if (isMoving)
        {
            isDoubleClick = true;

            StopCoroutine(playerMovementCoroutine);
            playerMovementCoroutine = null;

            Debug.Log("Dash Move");
        }
        else
        { 
            isMoving = true;

            Debug.Log("Normal Move");
        }

        onPlayerMovement?.Invoke(v, isDoubleClick);
        playerMovementCoroutine = StartCoroutine(ResetMovementFlag());
    }

    private IEnumerator ResetMovementFlag()
    {
        yield return new WaitForSeconds(0.3f);
        isMoving = false;
    }

    public Vector2 GetMousePosition()
    {
        return MousePositionAction.ReadValue<Vector2>();
    }

    private void OnCrouch(InputAction.CallbackContext ctx)
    {
        onPlayerCrouch?.Invoke();
    }


    private void OnThrowStoneCall(InputAction.CallbackContext ctx)
    {
        onThrowStone?.Invoke();
    }

    private void OnWhistleCall(InputAction.CallbackContext ctx)
    {
        onWhistle?.Invoke();
    }

    private void OnThrowIBaitCall(InputAction.CallbackContext ctx)
    {
        onThrowIBait?.Invoke();
    }

    private void OnRBaitCall(InputAction.CallbackContext ctx)
    {
        onRBait?.Invoke();
    }

    private void OnCancelSkillCall(InputAction.CallbackContext ctx)
    {
        onCancelSkill?.Invoke();
    }

    private void OnCastAbilityCall(InputAction.CallbackContext ctx)
    {
        onCastAbility?.Invoke();
    }
}
