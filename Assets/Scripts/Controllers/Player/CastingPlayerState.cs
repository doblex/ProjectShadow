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
    bool canThrow = false;
    Vector3 throwTarget;

    float castRadius;
    float noiseRadius;
    float raycastHeight;
    float throwSpeed;
    float throwHeight;

    // preview rendering
    int segments = 50;
    LineRenderer skillRadiusPreview;
    LineRenderer noiseRadiusPreview;
    LineRenderer throwArcPreview;

    public CastingPlayerState(PlayerController controller, PlayerVariables playerVariables, CastTypes _castType, bool isCrouching = false,
            float _castRadius = 0, float _castNoiseRadius = 0, float _raycastHeight = 0, float _throwSpeed = 0, float _throwHeight = 0)
        : base(controller, playerVariables)
    {
        IsCrouching = isCrouching;
        castType = _castType;
        castRadius = _castRadius;
        noiseRadius = _castNoiseRadius;
        raycastHeight = _raycastHeight;
        throwSpeed = _throwSpeed;
        throwHeight = _throwHeight;
    }

    public override void Enter()
    {
        Debug.Log($"Enter cast: {castType}");

        if (IsCrouching)
        {
            // TODO : SetPlayerModel crouch
        }
        else
        {

            // TODO : SetPlayerModel idle
        }

        // Stop player
        Controller.navMeshAgent.isStopped = true;
        Controller.navMeshAgent.ResetPath();

        // Init line rendereres
        skillRadiusPreview = Controller.GetComponentsInChildren<LineRenderer>()[0];
        noiseRadiusPreview = Controller.GetComponentsInChildren<LineRenderer>()[1];
        throwArcPreview = Controller.GetComponentsInChildren<LineRenderer>()[2];

        // Add to onCancelSkill delegate
        ActionManager.Instance.onCancelSkill += CallExit;

        // Add left click function based on castType
        switch (castType)
        {
            case CastTypes.Stone:
                ActionManager.Instance.onPlayerMovement += ThrowStone;
                break;

            case CastTypes.Whistle:
                ActionManager.Instance.onPlayerMovement += Whistle;
                break;

            case CastTypes.IBait:
                ActionManager.Instance.onPlayerMovement += ThrowIBait;
                break;

            case CastTypes.RBait:
                break;
        }
    }

    public override void Update()
    {
        // Handle ability preview display based on type
        switch (castType)
        {
           case CastTypes.Stone:
                DrawAbilityPreview(Color.red);
                DrawNoisePreview(Color.blue);
                break;

            case CastTypes.Whistle:
                DrawAbilityPreview(Color.red);
                break;

            case CastTypes.IBait:
                DrawAbilityPreview(Color.red);
                DrawNoisePreview(Color.blue);
                break;

            case CastTypes.RBait:
                DropRBait();
                break;
        }
    }

    public void DrawAbilityPreview(Color abilityColor)
    {
        skillRadiusPreview.positionCount = segments + 1;
        skillRadiusPreview.useWorldSpace = true;
        skillRadiusPreview.startColor = abilityColor;
        skillRadiusPreview.endColor = abilityColor;
        skillRadiusPreview.widthMultiplier = 0.1f;
        CreateCircle(skillRadiusPreview, Controller.gameObject.transform.position, castRadius);
        canThrow = true;
        throwTarget = Controller.transform.position;
    }

    public void DrawNoisePreview(Color noiseColor)
    {
        noiseRadiusPreview.positionCount = segments + 1;
        noiseRadiusPreview.useWorldSpace = true;
        noiseRadiusPreview.startColor = noiseColor;
        noiseRadiusPreview.endColor = noiseColor;
        noiseRadiusPreview.widthMultiplier = 0.1f;

        Ray ray = Camera.main.ScreenPointToRay(ActionManager.Instance.GetMousePosition());
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, 100000f)) return;

        // TODO range check
        Vector3 target = hit.point;

        Vector3 start = new Vector3(Controller.transform.position.x, Controller.transform.position.y + raycastHeight, Controller.transform.position.z);
        Vector3 destination = new Vector3(target.x, Controller.transform.position.y + raycastHeight, target.z);

        // Disable throw if LOS is interrupted
        if (Physics.Linecast(start, destination))
        {
            noiseRadiusPreview.positionCount = 0;
            canThrow = false;
            return;
        }

        // Limit target to castRadius Range
        if (Vector3.Distance(start, destination) > castRadius)
        {
            noiseRadiusPreview.positionCount = 0;
            canThrow = false;
            return;
        }

        // TODO Perhaps draw throwing arc here

        canThrow = true;
        CreateCircle(noiseRadiusPreview, target, noiseRadius);
        throwTarget = target;
    }

    public void ThrowStone(Vector2 mousePos, bool dash)
    {
        if (!canThrow) return;

        Debug.Log("Threw Stone");

        Controller.ThrowStone(throwTarget, throwSpeed);

        Controller.GetComponent<AbilityController>().ResetStoneThrowTimer();
        CallExit();
    }

    public void Whistle(Vector2 mousePos, bool dash)
    {
        Debug.Log("Used Whistle!");

        Controller.GetComponent<AbilityController>().ResetWhistleTimer();
        CallExit();
    }

    public void ThrowIBait(Vector2 mousePos, bool dash)
    {
        if (!canThrow) return;

        Debug.Log("Threw Irreplaceable Bait");

        Controller.GetComponent<AbilityController>().ConsumeIBait();
        CallExit();
    }

    public void DropRBait()
    {
        Debug.Log("Left Replaceable bait");

        Controller.GetComponent<AbilityController>().ResetRBaitTimer();
        CallExit();
    }

    public void CallExit()
    {
        Debug.Log("Exiting cast...");

        // Delete line renderers
        skillRadiusPreview.positionCount = 0;
        noiseRadiusPreview.positionCount = 0;
        throwArcPreview.positionCount = 0;

        // Flag exit
        canExit = true;

        // Unflag player cast
        Controller.SetCast(false);
        //Controller.CallUncastWithDelay(.3f);

        // Return to Idle
        nextState = new IdlePlayerState(Controller, PlayerVariables, IsCrouching);

        // Unbind cancel action
        ActionManager.Instance.onCancelSkill -= CallExit;

        // Remove left click function
        switch (castType)
        {
            case CastTypes.Stone:
                ActionManager.Instance.onPlayerMovement -= ThrowStone;
                break;

            case CastTypes.Whistle:
                ActionManager.Instance.onPlayerMovement -= Whistle;
                break;

            case CastTypes.IBait:
                ActionManager.Instance.onPlayerMovement -= ThrowIBait;
                break;

            case CastTypes.RBait:
                break;
        }
    }

    void CreateCircle(LineRenderer renderer, Vector3 center, float radius)
    {
        float x;
        float z;

        float angle = 20f;

        for (int i = 0; i < segments + 1; i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            renderer.SetPosition(i, new Vector3(center.x + x, center.y, center.z + z));

            angle += (360f / (segments/2));
        }
    }
}
