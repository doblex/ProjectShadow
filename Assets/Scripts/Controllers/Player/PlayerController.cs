using System;
using System.Collections;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AnimatorController))]
public class PlayerController : MonoBehaviour, ISaveable
{
    // save ID
    private string id;
    public string ID => id;

    string ISaveable.ID { get => ID; }

    [Header("Debug")]
    public bool debug;

    [Header("Options")]
    [SerializeField] public PlayerVariables playerVariables;
    [SerializeField,Layer] public LayerMask terrainLayer;

    [HideInInspector] public NavMeshAgent navMeshAgent;
    PlayerState currentState;

    [Header("Cover")]
    [SerializeField] float coverCheckDistance = 1.0f;
    [SerializeField] float coverCheckHeight = 1.0f;
    [SerializeField,Layer] LayerMask halfCoverLayer;
    [SerializeField,Layer] LayerMask bushLayer;

    [Header("Skills")]
    [SerializeField] NoiseOptions whistleSound;
    [SerializeField] Transform throwOrigin;

    [Header("Animations")]
    [SerializeField] Animator animatorController;
    [SerializeField] float animNormalSpeed = 1.0f;
    [SerializeField] float animRunSpeed = 2.5f;
    string animIsCrouch = "IsCrouching";
    string animIsWalking = "IsWalking";
    string animSpeedMult = "Speed";

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
        // REGISTER AS SAVEABLE
        id = System.Guid.NewGuid().ToString();
        //PersistenceManager.Instance?.RegisterSaveable(this);

        navMeshAgent = GetComponent<NavMeshAgent>();
        isCasting = false;

        animatorController.SetFloat(animSpeedMult, animNormalSpeed);

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

        if(debug) Debug.Log(debugMsg);
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
            animatorController.SetBool(animIsWalking, false);
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

        SetAnimatorOnCrouch(isCrouching);

        if(debug) Debug.Log("Crouch toggled. Now crouching: " + isCrouching);

        SoundManager.Instance?.ChangeOstOnCrouch(isCrouching);
        UpdateStates( new IdlePlayerState(this, playerVariables, isCrouching));
    }

    private void SetAnimatorOnCrouch(bool isCrouching)
    {
        SetIntoIdle(isCrouching);
    }

    private void HandlePlayerMovement(Vector2 mousePos, bool dash)
    {
        if (isCasting) return;

        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        RaycastHit hit;

        if(!Physics.Raycast(ray, out hit, 100f, terrainLayer)) return;

        if (dash)
        {
            animatorController.SetBool(animIsCrouch, false);
            animatorController.SetFloat(animSpeedMult, animRunSpeed);
            animatorController.SetBool(animIsWalking, true);
            UpdateStates(new DashMovePlayerState(this, hit.point, playerVariables,isCrouching));
        }
        else
        {
            if (isCrouching)
            {
                UpdateStates(new CrouchingPlayerState(this, hit.point, playerVariables));
                animatorController.SetBool(animIsWalking, true);
            }
            else
            {
                UpdateStates(new WalkMovePlayerState(this, hit.point, playerVariables));
                animatorController.SetFloat(animSpeedMult, animNormalSpeed);
                animatorController.SetBool(animIsWalking, true);
            }
        }
    }

    public void SetIntoIdle(bool isCrouching)
    {
        animatorController.SetFloat(animSpeedMult, animNormalSpeed);
        animatorController.SetBool(animIsWalking, false);
        animatorController.SetBool(animIsCrouch, isCrouching);
    }

    public void ThrowStone(Vector3 destination, float speed, float throwHeight)
    {
        Stone stone = Instantiate(GetComponent<AbilityController>().throwableStonePrefab, throwOrigin).
            GetComponent<Stone>().SetDestination(destination).SetSpeed(speed).SetThrowHeight(throwHeight);

        stone.transform.parent = null;
    }

    public void Whistle()
    {
        NoiseSpawnerManager.Instance.SpawnNoiseOrigin(throwOrigin.position, whistleSound);
    }

    public void ThrowIBait(Vector3 destination, float speed, float throwHeight)
    {
       IBait iBait = Instantiate(GetComponent<AbilityController>().iBaitPrefab, throwOrigin).
            GetComponent<IBait>().SetDestination(destination).SetSpeed(speed).SetThrowHeight(throwHeight);

        iBait.transform.parent = null;
    }

    public void DropRBait()
    {
        GameObject rBait = Instantiate(GetComponent<AbilityController>().rBaitPrefab, 
            new Vector3(throwOrigin.position.x, throwOrigin.position.y, throwOrigin.position.z), throwOrigin.rotation);
    }

    private void HandleInteract()
    {
        Collider[] objectsInRadius = Physics.OverlapSphere(transform.position, playerVariables.maxInteractDistance, ~0);

        IInteractable closestInteractable = null;
        float shortestDistance = playerVariables.maxInteractDistance;

        foreach (Collider obj in objectsInRadius)
        {
            IInteractable interactable = obj.GetComponent<IInteractable>();
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

        if(debug) Debug.Log("Set hiding to " + isHiding);
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


    // Save data
    [Serializable]
    private struct PlayerControllerData
    {
        public Vector3 position;
        public Vector3 rotation;
        public bool isCrouching;
    }
    public object Save()
    {
        return new PlayerControllerData
        {
            position = this.transform.position,
            rotation = this.transform.eulerAngles,
            isCrouching = this.isCrouching
        };
    }

    public void Load(string stateJson)
    {
        PlayerControllerData data = JsonUtility.FromJson<PlayerControllerData>(stateJson);

        // stop navmesh agent
        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();

        // apply variables
        transform.position = data.position;
        transform.eulerAngles = data.rotation;
        this.isCrouching = data.isCrouching;
    }

}
