using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;
using System.Collections.Generic;

public class AIController : MonoBehaviour
{
    #region Variables
    public enum State { Patrol, Investigation, Alarm }
    public enum EnemyType { Patrol, Sentry }

    [Header("Enemy Type")]
    public EnemyType enemyType = EnemyType.Patrol;

    [Header("State")]
    public State currentState = State.Patrol;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    int patrolIndex = 0;
    int lastPatrolIndex = 0;

    [Header("Field of View")]
    public float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 110f;
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    [HideInInspector] public List<Transform> visibleTargets = new List<Transform>();

    [Header("Investigation")]
    public float investigationTime = 20f;
    float investigationTimer;
    public Vector3 investigationPosition;
    public float searchAreaRadius = 5f;
    public float searchPauseTime = 2f;
    float searchTimer;
    bool isSearchingArea = false;
    Vector3 searchCenter;

    [Header("Look Around / Head")]
    public float headLookSpeed = 2f;
    float headLookTimer;
    float headLookAngle;

    [Header("Global Alarm Response")]
    public float alarmInvestigationRadiusMin = 3f;
    public float alarmInvestigationRadiusMax = 8f;

    [Header("Alarm")]
    public float alarmSearchTime = 15f;
    float alarmTimer;
    Vector3 lastSeenPlayerPosition;
    Vector3 lastAlarmPosition;
    public float alarmMinMoveDist = 2f;

    [Header("Reaction Priority")]
    public float reactionDelay = 2f;   // 2s prima di Alarm
    float alarmDelayTimer = 0f;
    bool alarmTriggered = false;

    float lastDistractionTime = -9999f;
    Vector3 lastDistractionPosition;

    [Header("Patrol Look")]
    public float lookAroundTime = 2f;
    float lookTimer;
    bool isLookingAround = false;

    [Header("Sentry Settings")]
    public float sentryLookInterval = 3f;
    public float sentryLookAngleRange = 360f;
    float sentryLookTimer;
    Vector3 sentryOriginalPosition;
    Quaternion sentryOriginalRotation;
    Quaternion sentryTargetRotation;

    NavMeshAgent agent;
    Transform player;
    #endregion
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (enemyType == EnemyType.Sentry)
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
        GlobalAlarm.OnPlayerSpotted += OnGlobalAlarm;

        alarmTimer = alarmSearchTime;
        sentryLookTimer = sentryLookInterval;
    }

    void OnDestroy()
    {
        GlobalAlarm.OnPlayerSpotted -= OnGlobalAlarm;
    }

    void Update()
    {
        LookForPlayer();

        switch (currentState)
        {
            case State.Patrol:
                if (enemyType == EnemyType.Sentry)
                    SentryBehavior();
                else
                    Patrol();
                break;

            case State.Investigation:
                Investigate();
                break;

            case State.Alarm:
                Alarm();
                break;
        }
    }
    #region look for player
    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle * 0.5f)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }
    void LookForPlayer()
    {
        if (visibleTargets.Count > 0)
        {
            player = visibleTargets[0];
            Vector3 currentPlayerPos = player.position;
            lastSeenPlayerPosition = currentPlayerPos;

            if (currentState == State.Patrol || currentState == State.Investigation)
            {
                alarmDelayTimer += Time.deltaTime;
                agent.isStopped = true;

                if (alarmDelayTimer >= reactionDelay)
                {
                    currentState = State.Alarm;
                    alarmDelayTimer = 0f;
                }
            }
        }
        else
        {
            alarmDelayTimer = 0f;
            if (currentState != State.Alarm && agent.isStopped)
                agent.isStopped = false;
        }
    }
    #endregion
    #region patrol
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
    public void StartInvestigation(Vector3 position)
    {
        investigationPosition = position;
        currentState = State.Investigation;

        isSearchingArea = false;
        searchCenter = position;
        lastPatrolIndex = patrolIndex;

        lastDistractionTime = Time.time;
        lastDistractionPosition = position;

        alarmTriggered = false;
        alarmDelayTimer = 0f;
    }

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
                if (enemyType == EnemyType.Sentry)
                {
                    agent.SetDestination(sentryOriginalPosition);
                    StartCoroutine(ReturnSentryToPost());
                }
                else
                {
                    patrolIndex = lastPatrolIndex;
                    if (patrolPoints != null && patrolPoints.Length > 0)
                        agent.SetDestination(patrolPoints[patrolIndex].position);
                    currentState = State.Patrol;
                }

                isSearchingArea = false;
            }
        }

        if (agent.enabled && agent.velocity.magnitude > 0.1f)
            WanderLook();
    }

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
    void SentryBehavior()
    {
        agent.SetDestination(sentryOriginalPosition);

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
    IEnumerator ReturnSentryToPost()
    {
        currentState = State.Patrol;

        while (Vector3.Distance(transform.position, sentryOriginalPosition) > 0.2f)
            yield return null;

        agent.ResetPath();
        transform.position = sentryOriginalPosition;
        transform.rotation = sentryOriginalRotation;

        alarmTriggered = false;
        alarmDelayTimer = 0f;
    }
    #endregion
    #region Alarms
    void Alarm()
    {
        if (!alarmTriggered)
        {
            alarmTriggered = true;
            lastAlarmPosition = lastSeenPlayerPosition;
            GlobalAlarm.RaiseAlarm(lastSeenPlayerPosition);
            lastDistractionTime = Time.time;
            lastDistractionPosition = lastSeenPlayerPosition;
            agent.isStopped = false;
            StartInvestigation(lastSeenPlayerPosition);
            currentState = State.Investigation;
        }
    }
    void OnGlobalAlarm(Vector3 spottedPosition)
    {
        if (currentState == State.Alarm)
            return;

        if (Time.time <= lastDistractionTime)
            return;

        for (int i = 0; i < 10; i++)
        {
            Vector2 offset2D = UnityEngine.Random.insideUnitCircle *
                               UnityEngine.Random.Range(alarmInvestigationRadiusMin, alarmInvestigationRadiusMax);

            Vector3 candidate = spottedPosition + new Vector3(offset2D.x, 0f, offset2D.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidate, out hit, alarmInvestigationRadiusMax, NavMesh.AllAreas))
            {
                StartInvestigation(hit.position);
                return;
            }
        }

        StartInvestigation(spottedPosition);
    }
    void ExitAlarmToPatrol()
    {
        player = null;

        if (enemyType == EnemyType.Sentry)
        {
            agent.SetDestination(sentryOriginalPosition);
            StartCoroutine(ReturnSentryToPost());
        }
        else
        {
            patrolIndex = lastPatrolIndex;
            if (patrolPoints != null && patrolPoints.Length > 0)
                agent.SetDestination(patrolPoints[patrolIndex].position);
            currentState = State.Patrol;
        }

        alarmTriggered = false;
        alarmDelayTimer = 0f;
    }
    #endregion
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