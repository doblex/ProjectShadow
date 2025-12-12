using UnityEditor;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] public PlayerVariables playerVariables;
    [SerializeField,Layer] public LayerMask terrainLayer;
    [SerializeField] NoiseOptions whistleSound;

    [HideInInspector] public NavMeshAgent navMeshAgent;
    PlayerState currentState;

    [Header("Cover")]
    [SerializeField] float coverCheckDistance = 1.0f;
    [SerializeField] float coverCheckHeight = 1.0f;
    [SerializeField,Layer] LayerMask halfCoverLayer;
    [SerializeField,Layer] LayerMask bushLayer;


    bool isCrouching = false;

    int[,] halfCoverTable = {
        {0,0,0},
        {0,0,0},
        {0,0,0}
    };

    Vector2[,] directions = {
        {  new Vector2(-1, 1), new Vector2(0, 1), new Vector2(1, 1) },
        { new Vector2(-1, 0), new Vector2(0, 0), new Vector2(1, 0) },
        { new Vector2(-1, -1), new Vector2(0, -1), new Vector2(1, -1) }
    };
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
        ActionManager.Instance.onInteract += HandleInteract;
    }

    private void OnDisable()
    {
        ActionManager.Instance.onPlayerMovement -= HandlePlayerMovement;
        ActionManager.Instance.onPlayerCrouch -= HandleCrouch;
        ActionManager.Instance.onInteract -= HandleInteract;
    }

    private void Start()
    {
        UpdateStates(new IdlePlayerState(this, playerVariables));
    }

    private void Update()
    {
        UpdateStates();
        UpdateCover();
    }

    private void UpdateCover()
    {
        CheckForPlayerInBush();
        CheckCoverAroundPlayer();
    }

    private void CheckForPlayerInBush()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.5f, bushLayer, QueryTriggerInteraction.Collide);

        bool isInBush = colliders.Length > 0;

        SetHiding(isInBush);
    }

    private void CheckCoverAroundPlayer()
    {
        Vector3 startPos;
        Vector3 dir;

        string debugMsg = "Half Cover Table:\n";

        Vector2 direction;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                direction = directions[i, j];

                if (direction == Vector2.zero)
                {
                    continue;
                }

                startPos = transform.position + new Vector3(0, coverCheckHeight, 0);
                dir = new Vector3(direction.x, 0, direction.y);

                if (Physics.Raycast(startPos, dir, out RaycastHit hit, coverCheckDistance, halfCoverLayer))
                {
                    halfCoverTable[i, j] = 1;
                }
                else
                {
                    halfCoverTable[i, j] = 0;
                }

                debugMsg += halfCoverTable[i, j] + " ";
            }
            debugMsg += "\n";
        }

        Debug.Log(debugMsg);
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

    private void HandleInteract()
    {
        Collider[] objectsInRadius = Physics.OverlapSphere(transform.position, playerVariables.maxInteractDistance, ~0);

        Interactable closestInteractable = null;
        float shortestDistance = playerVariables.maxInteractDistance;

        foreach (Collider obj in objectsInRadius)
        {
            Interactable interactable = obj.GetComponent<Interactable>();
            if (interactable != null)
            {
                float objDistance = (obj.transform.position - transform.position).sqrMagnitude;
                if (objDistance < shortestDistance)
                {
                    shortestDistance = objDistance;
                    closestInteractable = interactable;
                }
            }
        }

        if (closestInteractable != null) 
        { closestInteractable.Interact(); }
    }

    public bool IsHidingInHalfCover(Vector3 enemyPos)
    {
        // check se il personaggio � completamente coperto
        if (halfCoverTable[1, 1] == 1)
        { 
            return true;
        }

        // calcola la direzione 2D verso il nemico
        Vector3 directionToEnemy = (enemyPos - transform.position).normalized;
        Vector2 direction2D = new Vector2(directionToEnemy.x, directionToEnemy.z);

        int dx = Mathf.RoundToInt(direction2D.x);
        int dy = Mathf.RoundToInt(direction2D.y);

        int col = 1 + dx;
        int row = 1 + (-dy);

        col = Mathf.Clamp(col, 0, 2);
        row = Mathf.Clamp(row, 0, 2);

        return halfCoverTable[row, col] == 1;
    }

    public void SetHiding(bool isHiding)
    {
        halfCoverTable[1,1] = isHiding ? 1 : 0;
        GlobalVolumeManager.Instance?.SetHiding(isHiding);
        Debug.Log("Set hiding to " + isHiding);
    }

    private void OnDrawGizmos()
    {
        int halfCoverData = 0;
        Vector2 direction;

        Vector3 startPos;
        Vector3 endPos;

        if(halfCoverTable == null) return;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                halfCoverData = halfCoverTable[i,j];
                direction = directions[i,j];

                Gizmos.color = halfCoverData == 0 ? Color.red : Color.green;

                startPos = transform.position + new Vector3(0, coverCheckHeight, 0);
                endPos = new Vector3(direction.x, 0, direction.y) * coverCheckDistance;

                Gizmos.DrawRay(startPos, endPos);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, playerVariables.maxInteractDistance);
    }
}
