using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class MovingPlayerState : PlayerState
{
    private Coroutine MovementCoroutine;

    private Vector3 TargetPos;

    protected float Speed;
    protected float StepInterval;

    protected SoundOptions SoundOptions;


    public MovingPlayerState(PlayerController controller, Vector3 targetPos, PlayerVariables playerVariables) 
        : base(controller, playerVariables)
    {
        TargetPos = targetPos;
    }
    public override void Enter()
    {
        Controller.navMeshAgent.isStopped = false;
        Controller.navMeshAgent.speed = Speed;


        NavMeshPath path = new NavMeshPath();

        if (Controller.navMeshAgent.CalculatePath(TargetPos, path))
        {
            Controller.navMeshAgent.SetPath(path);
        }

        if (StepInterval > 0)
            MovementCoroutine = Controller.StartCoroutine(MovementSoundCue(StepInterval, SoundOptions));
    }
    public override void Update()
    {
        if (Controller.navMeshAgent.remainingDistance <= Controller.navMeshAgent.stoppingDistance)
        {
            canExit = true;
        }
    }
    public override void Exit()
    {
        if(MovementCoroutine != null)
        {
            Controller.StopCoroutine(MovementCoroutine);
            MovementCoroutine = null;
        }
    }

    private IEnumerator MovementSoundCue(float interval, SoundOptions soundOptions )
    {
        while (true)
        {
            NoiseSpawnerManager.Instance.SpawnNoiseOrigin(Controller.transform.position, soundOptions);
            //TODO : SoundImplementation
            yield return new WaitForSeconds(interval);
        }
    }
}
