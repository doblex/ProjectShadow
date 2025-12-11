using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] public PlayerVariables playerVariables;
    [SerializeField,Layer] public LayerMask terrainLayer;
    [SerializeField] SoundOptions whistleSound;

    [HideInInspector] public NavMeshAgent navMeshAgent;
    PlayerState currentState;

    bool isCrouching = false;
    bool isInBush = false;

    bool isCasting;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        isCasting = false;
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
        else if (isCasting && (currentState.GetType() != typeof(CastingPlayerState)))  // check that currentState is not of type CastingPlayerState
        {
            currentState?.Exit();
            currentState = GetComponent<AbilityController>().currentCast;
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

    public void CallUncastWithDelay(float delay)
    {
        StartCoroutine(UncastWithDelay(delay));
    }

    private IEnumerator UncastWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isCasting = false;
    }

    public void SetCast(bool value)
    {
        isCasting = value;
    }

    public bool IsCrouching()
    {
        return isCrouching;
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

    public void ThrowStone(Vector3 destination, float speed)
    {
        Stone stone = Instantiate(GetComponent<AbilityController>().throwableStonePrefab, transform).
            GetComponent<Stone>().SetDestination(destination).SetSpeed(speed);

        stone.transform.parent = null;
    }

    public void Whistle()
    {
        NoiseSpawnerManager.Instance.SpawnNoiseOrigin(transform.position, whistleSound);
    }

    public void ThrowIBait(Vector3 destination, float speed)
    {
       IBait iBait = Instantiate(GetComponent<AbilityController>().iBaitPrefab, transform).
            GetComponent<IBait>().SetDestination(destination).SetSpeed(speed);

        iBait.transform.parent = null;
    }

    public void DropRBait()
    {
        GameObject rBait = Instantiate(GetComponent<AbilityController>().rBaitPrefab, 
            new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);
    }
}
