using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;

public class AIController : MonoBehaviour
{
    #region Variables
    public enum Phase { Patrol, Investigation, Alarm }
    public enum EnemyRole { Patrol, Sentry }
    public enum EnemyType { Sentinel, ParanoidSentinel, Guard}

    [Header("Enemy Phase")]
    public EnemyRole role = EnemyRole.Patrol;

    [Header("Role")]
    public Phase phase = Phase.Patrol;

    [Header("Enemy Type")]
    public EnemyType enemyType = EnemyType.Sentinel;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    int patrolIndex = 0;
    int lastPatrolIndex = 0;

    [Header("Field of View")]
    public float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 110f;
    public LayerMask playerAndBaitMask;
    public LayerMask obstacleMask;
    [HideInInspector] public List<Transform> visibleTargets = new List<Transform>();

    [Header("Investigation")]
    public float investigationTime = 20f;
    public float investigationTimer;
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

    [Header("Alarm")]
    public float alarmSearchTime = 15f;
    public float alarmTimer;
    Vector3 lastSeenPlayerPosition;
    Vector3 lastAlarmPosition;
    public float alarmMinMoveDist = 2f;
    public float alarmRadius = 5f;
    public LayerMask enemyMask;

    [Header("Reaction Priority")]
    public float reactionDelay = 2f;
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
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, playerAndBaitMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2f)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    if (target.CompareTag("Player"))
                    {
                        visibleTargets.Add(target);
                    }
                    else if (target.CompareTag("Bait"))
                    {
                        OnBaitSeen(target.position);
                    }
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

            if (phase == Phase.Patrol || phase == Phase.Investigation)
            {
                alarmDelayTimer += Time.deltaTime;
                agent.isStopped = true;

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
            alarmDelayTimer = 0f;
            if (phase != Phase.Alarm && agent.isStopped)
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
        phase = Phase.Investigation;

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
    void Alarm()
    {
        if (!alarmTriggered)
        {
            alarmTriggered = true;
            lastAlarmPosition = lastSeenPlayerPosition;
            RaiseLocalAlarm(lastAlarmPosition);
            lastDistractionTime = Time.time;
            lastDistractionPosition = lastSeenPlayerPosition;
            agent.isStopped = false;
            StartInvestigation(lastSeenPlayerPosition);
            phase = Phase.Investigation;
        }
    }

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