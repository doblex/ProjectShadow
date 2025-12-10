using UnityEngine;

public enum CastTypes
{
    Stone,
    Whistle,
    IBait,
    RBait
};


public class CastingPlayerState : PlayerState
{
    bool IsCrouching;
    CastTypes castType;

    protected CastingPlayerState(PlayerController controller, PlayerVariables playerVariables, CastTypes type, bool isCrouching = false)
        : base(controller, playerVariables)
    {
        IsCrouching = isCrouching;
        castType = type;
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

        // add to onCancelSkill delegate
        ActionManager.Instance.onCancelSkill += Exit;
    }

    public override void Update()
    {
        // TODO Handle ability casting based on type
        switch (castType)
        {
           case CastTypes.Stone:

                break;
            case CastTypes.Whistle:

                break;
            case CastTypes.IBait:

                break;
            case CastTypes.RBait:

                break;
        }

    }

    public override void Exit()
    {
        nextState = this;
        Controller.navMeshAgent.isStopped = false;
        ActionManager.Instance.onCancelSkill -= Exit;
    }
}
