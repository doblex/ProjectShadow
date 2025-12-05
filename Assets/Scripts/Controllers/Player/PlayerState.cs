public class PlayerState
{
    protected PlayerController Controller;

    protected PlayerVariables PlayerVariables;

    protected PlayerState nextState = null;

    protected bool canExit = false;

    protected PlayerState(PlayerController controller, PlayerVariables playerVariables)
    { 
        Controller = controller;
        PlayerVariables = playerVariables;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }


    public PlayerState GetNextState()
    {
        return nextState;
    }

    public bool CanExit()
    {
        return canExit;
    }
}
