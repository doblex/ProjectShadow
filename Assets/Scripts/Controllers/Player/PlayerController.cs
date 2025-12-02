using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerVariables playerVariables;
    NavMeshAgent navMeshAgent;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        ActionManager.Instance.onPlayerMovement += HandlePlayerMovement;
    }

    private void OnDisable()
    {
        ActionManager.Instance.onPlayerMovement -= HandlePlayerMovement;
    }

    private void HandlePlayerMovement(Vector2 mousePos, bool dash)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        RaycastHit hit;

        Physics.Raycast(ray, out hit, 100f);

        if (dash)
        {
            navMeshAgent.speed = playerVariables.dashSpeed;
        }
        else
        {
            navMeshAgent.speed = playerVariables.moveSpeed;
        }

        navMeshAgent.SetDestination(hit.point);
    }
}
