using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static ActionManager;

public class AIController : MonoBehaviour
{
    #region Variables
    public enum Phase { Patrol, Investigation, Alarm }
    public enum EnemyRole { Patrol, Sentry }
    public enum EnemyType { Sentinel, ParanoidSentinel, Guard }

    [HideInInspector] public Phase phase = Phase.Patrol;

    [Header("Roles")]
    public EnemyRole role = EnemyRole.Patrol;

    [Header("Enemy Type")]
    public EnemyType enemyType = EnemyType.Sentinel;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    [HideInInspector] public int patrolIndex = 0;
    [HideInInspector] public int lastPatrolIndex = 0;

    [Header("Field of View")]
    public float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 110f;
    [Range(0f, 1f)] public float innerRadiusFactor = 0.5f;
    public LayerMask playerAndBaitMask;
    public LayerMask obstacleMask;
    [HideInInspector] public List<Transform> visibleTargets = new List<Transform>();

    [Header("Investigation")]
    public float investigationTime = 20f;
    [HideInInspector] public float investigationTimer;
    [HideInInspector] public Vector3 investigationPosition;
    [HideInInspector] public float searchAreaRadius = 5f;
    [HideInInspector] public float searchPauseTime = 2f;
    [HideInInspector] float searchTimer;
    [HideInInspector] bool isSearchingArea = false;
    [HideInInspector] Vector3 searchCenter;

    [Header("Look Around / Head")]
    [HideInInspector] public float headLookSpeed = 2f;
    [HideInInspector] float headLookTimer;
    [HideInInspector] float headLookAngle;

    [Header("Alarm")]
    [HideInInspector] public float alarmSearchTime = 15f;
    [HideInInspector] public float alarmTimer;
    [HideInInspector] Vector3 lastSeenPlayerPosition;
    [HideInInspector] Vector3 lastAlarmPosition;
    [HideInInspector] public float alarmMinMoveDist = 2f;
    public float alarmRadius = 5f;
    public LayerMask enemyMask;

    [Header("Reaction Priority")]
    [HideInInspector] public float reactionDelay = 2f;
    [HideInInspector] float alarmDelayTimer = 0f;
    [HideInInspector] bool alarmTriggered = false;

    [HideInInspector] float lastDistractionTime = -9999f;
    [HideInInspector] Vector3 lastDistractionPosition;

    [Header("Patrol Look")]
    [HideInInspector] public float lookAroundTime = 2f;
    [HideInInspector] float lookTimer;
    [HideInInspector] bool isLookingAround = false;

    [Header("Sentry Settings")]
    public float sentryLookInterval = 3f;
    [HideInInspector] public float sentryLookAngleRange = 360f;
    [HideInInspector] float sentryLookTimer;
    [HideInInspector] Vector3 sentryOriginalPosition;
    [HideInInspector] Quaternion sentryOriginalRotation;
    [HideInInspector] Quaternion sentryTargetRotation;
    [HideInInspector] bool sentryLookingAround = true;


    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public Transform player;

    [HideInInspector] public bool debugFovVisible;
    [HideInInspector] public bool playerInFOVNow;
    bool _lastPlayerInFOV;

    [Header("Animation")]
    public Animator animator;
    [HideInInspector] public string speedParam = "Speed";

    public bool IsSelected { get; private set; }
    #endregion
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (role == EnemyRole.Sentry)
        {
            sentryOriginalPosition = transform.position;
            sentryOriginalRotation = transform.rotation;
            sentryTargetRotation = transform.rotation;
            agent.SetDestination(sentryOriginalPosition);
        }
        else
        {
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                agent.SetDestination(patrolPoints[patrolIndex].position);
                lastPatrolIndex = patrolIndex;
            }
        }

        StartCoroutine(FindTargetsWithDelay(0.2f));

        alarmTimer = alarmSearchTime;
        sentryLookTimer = sentryLookInterval;
    }


    void Update()
    {
        LookForPlayer();

        switch (phase)
        {
            case Phase.Patrol:
                if (role == EnemyRole.Sentry)
                    SentryBehavior();
                else
                    Patrol();
                break;

            case Phase.Investigation:
                Investigate();
                break;

            case Phase.Alarm:
                Alarm();
                break;
        }
        UpdateAnimation();
    }
    private void OnEnable()
    {
        if (ActionManager.Instance != null)
            ActionManager.Instance.onEnemySelected += OnEnemySelected;
    }

    private void OnDisable()
    {
        if (ActionManager.Instance != null)
            ActionManager.Instance.onEnemySelected -= OnEnemySelected;
    }
    #region look for player
    // Cerca i bersagli visibili ogni tot secondi
    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }
    // Trova i bersagli visibili nell'FOV
    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, playerAndBaitMask);
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            if (!target.CompareTag("Player") && !target.CompareTag("Bait"))
                continue;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
            if (angleToTarget > viewAngle * 0.5f)
                continue;
            float dstToTarget = Vector3.Distance(transform.position, target.position);
            if (Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                continue;
            if (target.CompareTag("Bait"))
            {
                OnBaitSeen(target.position);
                continue;
            }
            PlayerController playerCtrl = target.GetComponent<PlayerController>();
            if (playerCtrl == null)
                continue;
            bool isHiddenFromThisEnemy = playerCtrl.IsHidingInHalfCover(transform.position);
            bool hiddenFromRight = playerCtrl.IsHidingInHalfCover(transform.position + transform.right * 0.5f);
            bool hiddenFromLeft = playerCtrl.IsHidingInHalfCover(transform.position - transform.right * 0.5f);
            bool isInFullCover = isHiddenFromThisEnemy && hiddenFromRight && hiddenFromLeft;
            if (isInFullCover)
                continue;
            float innerRadius = viewRadius * innerRadiusFactor;
            bool inInnerCone = dstToTarget <= innerRadius;
            if (!isHiddenFromThisEnemy || inInnerCone)
            {
                Debug.Log($"[{name}] VEDO il player");
                visibleTargets.Add(target);
            }
        }
    }
    // Gestisce il comportamento in base alla visione del giocatore
    void LookForPlayer()
    {
        if (visibleTargets.Count > 0)
        {
            if (sentryLookingAround)
                sentryLookingAround = false;

            player = visibleTargets[0];
            Vector3 currentPlayerPos = player.position;
            lastSeenPlayerPosition = currentPlayerPos;

            if (phase == Phase.Patrol || phase == Phase.Investigation)
            {
                alarmDelayTimer += Time.deltaTime;

                if (alarmDelayTimer < reactionDelay * 0.5f)
                    agent.isStopped = true;
                else
                    agent.isStopped = false;

                if (alarmDelayTimer >= reactionDelay)
                {
                    bool canRaise =
                        lastAlarmPosition == Vector3.zero ||
                        Vector3.Distance(currentPlayerPos, lastAlarmPosition) >= alarmMinMoveDist;

                    if (canRaise)
                    {
                        phase = Phase.Alarm;
                        alarmDelayTimer = 0f;
                    }
                    else
                    {
                        agent.isStopped = false;
                        alarmDelayTimer = 0f;
                    }
                }
            }
        }
        else
        {
            if (phase == Phase.Patrol && role == EnemyRole.Sentry)
                sentryLookingAround = true;
            player = null;
            alarmDelayTimer = 0f;
            if (phase != Phase.Alarm && agent.isStopped)
                agent.isStopped = false;
        }
        playerInFOVNow = visibleTargets.Count > 0;

        if (playerInFOVNow != _lastPlayerInFOV)
        {
            _lastPlayerInFOV = playerInFOVNow;

            foreach (var fov in GetComponentsInChildren<FieldOfViewMesh>())
            {
                fov.isPlayerInsideFOV = playerInFOVNow;
                fov.UpdateVisibility();
            }

            foreach (var fovColor in GetComponentsInChildren<FOVColorController>())
                fovColor.UpdateVisibility();
        }
    }

    #endregion
    #region patrol
    // Comportamento di pattugliamento
    void Patrol()
    {
        if (Time.time < lastDistractionTime + 1f)
            return;

        if (isLookingAround)
        {
            lookTimer += Time.deltaTime;
            if (lookTimer >= lookAroundTime)
            {
                isLookingAround = false;
                lookTimer = 0f;

                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                lastPatrolIndex = patrolIndex;
                agent.SetDestination(patrolPoints[patrolIndex].position);
            }
        }
        else if (agent.remainingDistance < 0.5f)
        {
            isLookingAround = true;
            lookTimer = 0f;

            float randomAngle = UnityEngine.Random.Range(-45f, 45f);
            transform.Rotate(0f, randomAngle, 0f);
        }
    }
    #endregion
    #region investigation
    // Inizia l'investigazione in un punto specifico
    public void StartInvestigation(Vector3 position)
    {
        investigationPosition = position;
        phase = Phase.Investigation;

        isSearchingArea = false;
        searchCenter = position;
        lastPatrolIndex = patrolIndex;

        lastDistractionTime = Time.time;
        lastDistractionPosition = position;

        alarmTriggered = false;
        alarmDelayTimer = 0f;
    }
    // Comportamento di investigazione
    void Investigate()
    {
        if (!isSearchingArea)
        {
            agent.SetDestination(investigationPosition);

            if (!agent.pathPending && agent.remainingDistance < 1f)
            {
                isSearchingArea = true;
                searchCenter = investigationPosition;
                searchTimer = searchPauseTime;

                investigationTimer = investigationTime;
                agent.velocity = Vector3.zero;
            }
        }
        else
        {
            SearchAroundArea();

            investigationTimer -= Time.deltaTime;
            if (investigationTimer <= 0f)
            {
                if (role == EnemyRole.Sentry)
                {
                    agent.SetDestination(sentryOriginalPosition);
                    StartCoroutine(ReturnSentryToPost());
                }
                else
                {
                    patrolIndex = lastPatrolIndex;
                    if (patrolPoints != null && patrolPoints.Length > 0)
                        agent.SetDestination(patrolPoints[patrolIndex].position);
                    phase = Phase.Patrol;
                }

                isSearchingArea = false;
            }
        }

        if (agent.enabled && agent.velocity.magnitude > 0.1f)
            WanderLook();
    }
    // Cerca un punto casuale nell'area di ricerca
    void SearchAroundArea()
    {
        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0f)
        {
            Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * searchAreaRadius;
            Vector3 candidate = searchCenter + new Vector3(randomPoint.x, 0f, randomPoint.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidate, out hit, searchAreaRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                searchTimer = searchPauseTime;
            }
        }
    }
    // Movimento della testa durante l'investigazione
    void WanderLook()
    {
        headLookTimer += Time.deltaTime;

        if (headLookTimer >= UnityEngine.Random.Range(0.8f, 1.5f))
        {
            if (UnityEngine.Random.value < 0.8f)
            {
                headLookAngle = UnityEngine.Random.Range(-60f, 60f);
            }
            else
            {
                headLookAngle = UnityEngine.Random.Range(-150f, -90f);
                if (UnityEngine.Random.value < 0.5f)
                    headLookAngle = UnityEngine.Random.Range(90f, 150f);
            }

            headLookTimer = 0f;
        }

        Quaternion targetRot = Quaternion.Euler(0f, headLookAngle, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, headLookSpeed * Time.deltaTime);
    }
    #endregion
    #region sentry
    // Comportamento del sentinella
    void SentryBehavior()
    {
        agent.SetDestination(sentryOriginalPosition);

        if (!sentryLookingAround)
            return;

        sentryLookTimer -= Time.deltaTime;
        if (sentryLookTimer <= 0f)
        {
            float targetY = UnityEngine.Random.Range(0f, 360f);
            sentryTargetRotation = Quaternion.Euler(0f, targetY, 0f);
            sentryLookTimer = sentryLookInterval;
        }

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            sentryTargetRotation,
            headLookSpeed * Time.deltaTime
        );
    }

    // Ritorna il sentinella al suo posto originale
    IEnumerator ReturnSentryToPost()
    {
        phase = Phase.Patrol;

        while (Vector3.Distance(transform.position, sentryOriginalPosition) > 0.2f)
            yield return null;

        agent.ResetPath();
        transform.position = sentryOriginalPosition;
        transform.rotation = sentryOriginalRotation;

        alarmTriggered = false;
        alarmDelayTimer = 0f;
    }
    #endregion
    #region Bait
    // Reazione alla vista di un'esca
    public void OnBaitSeen(Vector3 baitPos)
    {
        switch (enemyType)
        {
            case EnemyType.Sentinel:
                StartInvestigation(baitPos);
                break;

            case EnemyType.ParanoidSentinel:
                lastSeenPlayerPosition = baitPos;
                lastAlarmPosition = baitPos;
                alarmTriggered = true;
                RaiseLocalAlarm(baitPos);
                StartInvestigation(baitPos);
                break;

            case EnemyType.Guard:
                break;
        }
    }
    #endregion
    #region Sound
    // Reazione all'udire un suono
    public void OnSoundHeard(Vector3 soundPos)
    {
        switch (enemyType)
        {
            case EnemyType.Sentinel:
                // investiga attorno al punto del suono
                StartInvestigation(soundPos);
                break;

            case EnemyType.ParanoidSentinel:
                // NON va al suono: investiga attorno alla sua posizione attuale
                StartInvestigation(transform.position);
                break;

            case EnemyType.Guard:
                // ignora
                break;
        }
    }
    #endregion
    #region Alarms
    // Comportamento di allarme
    void Alarm()
    {
        if (!alarmTriggered)
        {
            alarmTriggered = true;
            lastAlarmPosition = lastSeenPlayerPosition;
            RaiseLocalAlarm(lastAlarmPosition);
            lastDistractionTime = Time.time;
            lastDistractionPosition = lastSeenPlayerPosition;
        }
        if (playerInFOVNow && player != null)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            if (Vector3.Distance(transform.position, lastSeenPlayerPosition) > 0.5f)
            {
                agent.isStopped = false;
                agent.SetDestination(lastSeenPlayerPosition);
            }
            else
            {
                StartInvestigation(lastSeenPlayerPosition);
                phase = Phase.Investigation;
                alarmTriggered = false;
            }
        }
    }

    // Propaga l'allarme ai nemici vicini
    void RaiseLocalAlarm(Vector3 alarmPos)
    {
        Collider[] hits = Physics.OverlapSphere(alarmPos, alarmRadius, enemyMask);

        for (int i = 0; i < hits.Length; i++)
        {
            AIController otherAI = hits[i].GetComponent<AIController>();
            if (otherAI != null && otherAI != this)
            {
                otherAI.OnAlarmHeard(alarmPos);
            }
        }
    }
    // Reazione all'udire un allarme
    public void OnAlarmHeard(Vector3 alarmPos)
    {
        if (phase == Phase.Alarm)
            return;
        if (Time.time > lastDistractionTime)
        {
            StartInvestigation(alarmPos);
        }
    }
    #endregion
    // Aggiorna i parametri dell'animazione
    void UpdateAnimation()
    {
        if (animator == null || agent == null) return;

        Vector3 vel = agent.velocity;
        vel.y = 0f;
        float speed = vel.magnitude;

        animator.SetFloat(speedParam, speed);
    }
    // Gestisce la selezione del nemico
    private void OnEnemySelected(AIController selected)
    {
        IsSelected = (selected == this);

        foreach (var fov in GetComponentsInChildren<FieldOfViewMesh>())
            fov.UpdateVisibility();
        foreach (var fovColor in GetComponentsInChildren<FOVColorController>())
            fovColor.UpdateVisibility();
    }
    // Imposta lo stato di selezione del nemico
    public void SetSelected(bool selected)
    {
        IsSelected = selected;

        foreach (var fov in GetComponentsInChildren<FieldOfViewMesh>())
            fov.UpdateVisibility();
        foreach (var fovColor in GetComponentsInChildren<FOVColorController>())
            fovColor.UpdateVisibility();
    }
    #region gyzmos
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 a = DirFromAngle(-viewAngle * 0.5f, false);
        Vector3 b = DirFromAngle(viewAngle * 0.5f, false);
        Gizmos.DrawRay(transform.position, a * viewRadius);
        Gizmos.DrawRay(transform.position, b * viewRadius);
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
            angleInDegrees += transform.eulerAngles.y;

        return new Vector3(
            Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),
            0f,
            Mathf.Cos(angleInDegrees * Mathf.Deg2Rad)
        );
    }
    #endregion
}