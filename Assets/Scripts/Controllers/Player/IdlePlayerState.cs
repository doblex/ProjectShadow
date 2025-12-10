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
        if (IsCrouching)
        {
            // TODO : SetPlayerModel crouch
        }
        else
        {

            // TODO : SetPlayerModel idle
        }

        Controller.navMeshAgent.isStopped = true;
        Controller.navMeshAgent.ResetPath();
        
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
