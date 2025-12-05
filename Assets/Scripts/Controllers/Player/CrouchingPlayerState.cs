using UnityEngine;

public class CrouchingPlayerState : MovingPlayerState
{
    public CrouchingPlayerState(PlayerController controller, Vector3 targetPos, PlayerVariables playerVariables)
        : base(controller, targetPos, playerVariables)
    {
        Speed = playerVariables.crouchSpeed;
        StepInterval = 0;
    }

    public override void Exit()
    {
        base.Exit();
        nextState = new IdlePlayerState(Controller, PlayerVariables ,true);
    }
}