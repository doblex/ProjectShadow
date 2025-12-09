using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] public PlayerVariables playerVariables;
    [SerializeField,Layer] public LayerMask terrainLayer;

    [HideInInspector] public NavMeshAgent navMeshAgent;
    PlayerState currentState;

    bool isCrouching = false;
    bool isInBush = false;

    bool isCasting = false;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        ActionManager.Instance.onPlayerMovement += HandlePlayerMovement;
        ActionManager.Instance.onPlayerCrouch += HandleCrouch;
    }

    private void OnDisable()
    {
        ActionManager.Instance.onPlayerMovement -= HandlePlayerMovement;
        ActionManager.Instance.onPlayerCrouch -= HandleCrouch;
    }

    private void Start()
    {
        UpdateStates(new IdlePlayerState(this, playerVariables));
    }

    private void Update()
    {
        UpdateStates();
    }

    private void UpdateStates(PlayerState forcedState = null)
    {
        // TODO Force casting state first

        if (forcedState != null)
        {
            currentState?.Exit();
            currentState = forcedState;
            currentState.Enter();
        }
        else
        {
            if (currentState.CanExit())
            {
                currentState?.Exit();
                currentState = currentState.GetNextState();
                currentState.Enter();
            }
        }
        
        currentState?.Update();
    }

    public void SetCast(bool value)
    {
        isCasting = value;
    }

    private void HandleCrouch()
    {
        if (isCasting) return;

        isCrouching = !isCrouching;

        Debug.Log("Crouch toggled. Now crouching: " + isCrouching);

        SoundManager.Instance?.ChangeOstOnCrouch(isCrouching);
        UpdateStates( new IdlePlayerState(this, playerVariables, isCrouching));
    }

    private void HandlePlayerMovement(Vector2 mousePos, bool dash)
    {
        if (isCasting) return;

        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        RaycastHit hit;

        if(!Physics.Raycast(ray, out hit, 100f, terrainLayer)) return;

        if (dash)
        {
            UpdateStates(new DashMovePlayerState(this, hit.point, playerVariables,isCrouching));
        }
        else
        {
            if (isCrouching)
            {
                UpdateStates(new CrouchingPlayerState(this, hit.point, playerVariables));
            }
            else
            {
                UpdateStates(new WalkMovePlayerState(this, hit.point, playerVariables));
            }
        }
    }
}
