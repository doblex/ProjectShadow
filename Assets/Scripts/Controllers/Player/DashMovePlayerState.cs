using UnityEngine;

public class DashMovePlayerState : MovingPlayerState
{
    bool WasCrouching;

    public DashMovePlayerState(PlayerController playerController, Vector3 targetPos, PlayerVariables playerVariables, bool wasCrouching)
        : base(playerController, targetPos, playerVariables)
    {
        Speed = playerVariables.dashSpeed;
        StepInterval = playerVariables.DashSoundInterval;
        WasCrouching = wasCrouching;
        SoundOptions = playerVariables.dashSoundOptions;
    }

    public override void Exit()
    {
        base.Exit();
        nextState = new IdlePlayerState(Controller, PlayerVariables, WasCrouching);
    }
}