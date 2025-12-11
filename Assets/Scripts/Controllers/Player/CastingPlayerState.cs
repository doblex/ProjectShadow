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

    float castRadius;
    float castNoiseRadius;
    float raycastHeight;

    public CastingPlayerState(PlayerController controller, PlayerVariables playerVariables, CastTypes type, bool isCrouching = false,
        float _castRadius = 0, float _castNoiseRadius = 0, float _raycastHeight = 0)
        : base(controller, playerVariables)
    {
        IsCrouching = isCrouching;
        castType = type;
        castRadius = _castRadius;
        castNoiseRadius = _castNoiseRadius;
        raycastHeight = _raycastHeight;
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
        ActionManager.Instance.onCancelSkill += CallExit;

        // TODO Add left click function based on castType
    }

    public override void Update()
    {
        // TODO Handle ability preview display based on type
        switch (castType)
        {
           case CastTypes.Stone:
                Debug.Log("Throwing stone...");
                break;
            case CastTypes.Whistle:
                Debug.Log("Whistle...");
                break;
            case CastTypes.IBait:
                Debug.Log("Throwing IBait...");
                break;
            case CastTypes.RBait:
                Debug.Log("RBait...");
                break;
        }

    }

    // TODO make ability usage functions and add to left click OnEnter (or use immediately in case of RBait)

    public void CallExit()
    {
        canExit = true;
        Controller.SetCast(false);
        nextState = new IdlePlayerState(Controller, PlayerVariables, IsCrouching);
        ActionManager.Instance.onCancelSkill -= CallExit;

        // TODO remove left click function
    }

    public override void Exit()
    {
        base.Exit();
    }
}
