using UnityEngine;

public class IdlePlayerState : PlayerState
{
    bool IsCrouching;

    public IdlePlayerState(PlayerController controller, PlayerVariables playerVariables, bool isCrouching = false) 
        : base(controller, playerVariables) 
    {
        IsCrouching = isCrouching;
    }
    public override void Enter()
    {
        Debug.Log("Idle state");

        Controller.navMeshAgent.isStopped = true;
        Controller.navMeshAgent.ResetPath();

        Controller.SetIntoIdle();


    }

    public override void Update()
    {
       
    }

    public override void Exit()
    {
        nextState = this;
        Controller.navMeshAgent.isStopped = false;
    }

}
