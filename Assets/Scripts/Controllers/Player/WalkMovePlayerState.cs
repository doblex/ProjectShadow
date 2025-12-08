using UnityEngine;

public class WalkMovePlayerState : MovingPlayerState
{
    public WalkMovePlayerState(PlayerController playerController, Vector3 targetPos, PlayerVariables playerVariables) 
        : base(playerController, targetPos, playerVariables)
    {
        Speed = playerVariables.walkMoveSpeed;
        StepInterval = playerVariables.walkSoundInterval;
        SoundOptions = playerVariables.walkSoundOptions;
    }

    public override void Exit()
    {
        base.Exit();
        nextState = new IdlePlayerState(Controller, PlayerVariables);
    }
}
