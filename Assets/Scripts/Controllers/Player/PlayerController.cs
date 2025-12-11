using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] public PlayerVariables playerVariables;
    [SerializeField, Layer] public LayerMask terrainLayer;

    [HideInInspector] public NavMeshAgent navMeshAgent;
    PlayerState currentState;

    [Header("Half Cover")]
    [SerializeField] float coverCheckDistance = 1.0f;
    [SerializeField] float coverCheckHeight = 1.0f;
    [SerializeField,Layer] LayerMask halfCoverLayer;


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
        UpdateCover();
    }

    private void UpdateCover()
    {
        Vector3 startPos;
        Vector3 endPos;

        string debugMsg = "Half Cover Table:\n";

        Vector2 direction;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                direction = directions[i,j];

                if (direction == Vector2.zero)
                {
                    continue;
                }

                startPos = transform.position + new Vector3(0, coverCheckHeight, 0);
                endPos = new Vector3(direction.x, 0, direction.y);

                if (Physics.Raycast(startPos, endPos, out RaycastHit hit, coverCheckDistance, halfCoverLayer))
                {
                    halfCoverTable[i, j] = 1;
                }
                else
                {
                    halfCoverTable[i, j] = 0;
                }

                debugMsg += halfCoverTable[i,j] + " ";
            }
            debugMsg += "\n";
        }
    }

    private void UpdateStates(PlayerState forcedState = null)
    {
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

    private void HandleCrouch()
    {
        isCrouching = !isCrouching;

        Debug.Log("Crouch toggled. Now crouching: " + isCrouching);

        SoundManager.Instance?.ChangeOstOnCrouch(isCrouching);
        UpdateStates( new IdlePlayerState(this, playerVariables, isCrouching));
    }

    private void HandlePlayerMovement(Vector2 mousePos, bool dash)
    {
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

    public bool IsHidingInHalfCover(Vector3 enemyPos)
    {
        // check se il personaggio è completamente coperto
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
}
