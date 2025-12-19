using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class ActionManager : MonoBehaviour
{
    public static ActionManager Instance;

    public delegate void OnCameraMovementChanged(Vector3 movement);
    public delegate void OnCameraRotationChanged(Quaternion rotation);

    public delegate void OnPlayerMovement(Vector2 mousePos, bool dash);
    public delegate void OnResetCamera();

    public delegate void OnInteract();
    public delegate void OnHighlight(bool active);
    public delegate void OnPlayerCrouch();

    public delegate void OnPauseGame(bool isPaused);

    public OnCameraMovementChanged onMovementChanged;
    public OnCameraRotationChanged onRotationChanged;
    public OnResetCamera onResetCamera;

    public OnPlayerMovement onPlayerMovement;

    public OnInteract onInteract;
    public OnHighlight onHighlight;
    public OnPlayerCrouch onPlayerCrouch;

    public OnPauseGame onPauseGame;

    [SerializeField] private CameraOptions Options;

    private InputAction MoveVisual;
    private bool isMoving = false;
    Coroutine playerMovementCoroutine;


    private InputAction RotateVisual;
    private InputAction ResetCamera;

    private InputAction InteractAction;

    private InputAction HighlightAction;
    private bool HighlightActive = false;

    private InputAction PlayerMovementAction;
    private InputAction PlayerDashAction;
    private InputAction MousePositionAction;

    private InputAction CrouchAction;

    private InputAction PauseAction;
    private bool isPaused = false;

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
    private InputAction SelectEnemyAction;


    Coroutine MovementCoroutine;
    Coroutine RotationCoroutine;

    // Save system
    public delegate void OnSaveRequested(SaveSlot saveSlot);
    public delegate void OnLoadRequested(SaveSlot saveSlot);

    public OnSaveRequested onSaveRequested;
    public OnLoadRequested onLoadRequested;

    private InputAction SaveAction;
    private InputAction LoadAction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            //DontDestroyOnLoad(this);
        }
    }

    private void Start()
    {
        SetupCommands();
    }

    private void OnDisable()
    {
        MoveVisual.performed -= OnMoveDown;
        MoveVisual.canceled -= OnMoveUp;

        RotateVisual.performed -= OnRotateDown;
        RotateVisual.canceled -= OnRotateUp;
        ResetCamera.performed -= OnCameraReset;

        InteractAction.performed -= OnInteractInput;

        HighlightAction.performed -= OnHighlightInput;

        PlayerMovementAction.performed -= OnMovePlayer;

        CrouchAction.performed -= OnCrouch;

        PauseAction.performed -= OnPause;


        ThrowStoneAction.performed -= OnThrowStoneCall;

        WhistleAction.performed -= OnWhistleCall;

        ThrowIBaitAction.performed -= OnThrowIBaitCall;
            
        RBaitAction.performed -= OnRBaitCall;

        CastAbilityAction.performed -= OnCastAbilityCall;

        CancelSkillAction.performed -= OnCancelSkillCall;

        SaveAction.performed -= OnSaveRequestedCall;

        LoadAction.performed -= OnLoadRequestedCall;
    }

    private void SetupCommands()
    {
        MoveVisual = InputSystem.actions.FindAction("MoveCamera");
        MoveVisual.performed += OnMoveDown;
        MoveVisual.canceled += OnMoveUp;

        RotateVisual = InputSystem.actions.FindAction("RotateCamera");
        RotateVisual.performed += OnRotateDown;
        RotateVisual.canceled += OnRotateUp;

        ResetCamera = InputSystem.actions.FindAction("ResetCamera");
        ResetCamera.performed += OnCameraReset;

        InteractAction = InputSystem.actions.FindAction("Interact");
        InteractAction.performed += OnInteractInput;

        HighlightAction = InputSystem.actions.FindAction("Highlight");
        HighlightAction.performed += OnHighlightInput;

        PlayerMovementAction = InputSystem.actions.FindAction("MovePlayer");
        PlayerMovementAction.performed += OnMovePlayer;

        MousePositionAction = InputSystem.actions.FindAction("MousePosition");

        CrouchAction = InputSystem.actions.FindAction("Crouch");
        CrouchAction.performed += OnCrouch;

        PauseAction = InputSystem.actions.FindAction("Pause");
        PauseAction.performed += OnPause;


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

        // Save and Load bindings
        SaveAction = InputSystem.actions.FindAction("Save");
        SaveAction.performed += OnSaveRequestedCall;

        LoadAction = InputSystem.actions.FindAction("Load");
        LoadAction.performed += OnLoadRequestedCall;
    }


    // -------------------------------
    //   CALLBACKS
    // -------------------------------

    private void OnMoveDown(InputAction.CallbackContext ctx)
    {
        if (isPaused) return;

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
        if (isPaused) return;

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

    private void OnCameraReset(InputAction.CallbackContext ctx)
    {
        if (isPaused) return;

        onResetCamera?.Invoke();
    }


    private void OnInteractInput(InputAction.CallbackContext ctx)
    {
        if (isPaused) return;

        onInteract?.Invoke();
    }
    private void OnHighlightInput(InputAction.CallbackContext ctx)
    {
        if (isPaused) return;

        HighlightActive = !HighlightActive;
        onHighlight?.Invoke(HighlightActive);
    }

    private void OnMovePlayer(InputAction.CallbackContext ctx)
    {
        if (isPaused) return;

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
        if (isPaused) return;

        onPlayerCrouch?.Invoke();
    }

    public void OnPause()
    {
        InputAction.CallbackContext ctx = new InputAction.CallbackContext();
        OnPause(ctx);
    }
    private void OnPause(InputAction.CallbackContext ctx)
    {
        isPaused = !isPaused;
        onPauseGame?.Invoke(isPaused);
    }

    public void OnAbility(int AbNumber)
    {
        if (isPaused) return;

        InputAction.CallbackContext ctx = new InputAction.CallbackContext();

        switch (AbNumber)
        {
            case 1:
                OnThrowStoneCall(ctx);
                break;

            case 2:
                OnThrowIBaitCall(ctx);
                break;

            case 3:
                OnWhistleCall(ctx);
                break;

            case 4:
                OnRBaitCall(ctx);
                break;

            default:
                Debug.LogError("L'abilità selezionata non esiste");
                break;
        }
    }

    private void OnThrowStoneCall(InputAction.CallbackContext ctx)
    {
        if (isPaused) return;

        onThrowStone?.Invoke();
    }

    private void OnWhistleCall(InputAction.CallbackContext ctx)
    {
        if (isPaused) return;

        onWhistle?.Invoke();
    }

    private void OnThrowIBaitCall(InputAction.CallbackContext ctx)
    {
        if (isPaused) return;

        onThrowIBait?.Invoke();
    }

    private void OnRBaitCall(InputAction.CallbackContext ctx)
    {
        if (isPaused) return;

        onRBait?.Invoke();
    }

    private void OnCancelSkillCall(InputAction.CallbackContext ctx)
    {
        if (isPaused) return;

        onCancelSkill?.Invoke();
    }

    private void OnCastAbilityCall(InputAction.CallbackContext ctx)
    {
        if (isPaused) return;

        onCastAbility?.Invoke();
    }

    private void OnSaveRequestedCall(InputAction.CallbackContext ctx)
    {
        if (isPaused) return;

        onSaveRequested?.Invoke(SaveSlot.Slot1);
    }

    private void OnLoadRequestedCall(InputAction.CallbackContext ctx)
    {   
        if (isPaused) return;

        onLoadRequested?.Invoke(SaveSlot.Slot1);
    }
}
