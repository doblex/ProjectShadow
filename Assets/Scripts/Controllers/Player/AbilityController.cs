using UnityEngine;

public class AbilityController : MonoBehaviour
{
    // Setup variables
    [Header("Setup Variables")]
    [SerializeField] private PlayerController controller;

    // Throw height
    [Header("Throw Height")]
    [SerializeField] private float raycastHeight; // horizontal raycast height from player center to determine if skill can be cast

    // Use linerenderer for throwing arc display

    //Throwable stone
    [Header("Throwable Stone")]
    [SerializeField] private GameObject throwableStonePrefab;
    [SerializeField] private float stoneThrowRadius;
    [SerializeField] private float stoneThrowNoiseRadius;
    [SerializeField] private float stoneThrowSpeed;
    [SerializeField] private float stoneThrowHeight;
    [SerializeField] private float stoneThrowCooldown;
    // TODO add soundOptions for ability
    private float stoneThrowTimer;
    private bool stoneThrowEnabled = true;

    //Whistle
    [Header("Whistle")]
    [SerializeField] private float whistleRadius;
    [SerializeField] private float whistleCooldown;
    private float whistleTimer;
    private bool whistleEnabled = true;

    // Irreplaceable Bait
    [Header("Irreplaceable Bait")]
    [SerializeField] private GameObject iBaitPrefab;
    [SerializeField] private float iBaitCount;
    [SerializeField] private float iBaitThrowRadius;
    [SerializeField] private float iBaitThrowSpeed;
    [SerializeField] private float iBaitThrowHeight;

    // Replaceable Bait
    [Header("Replaceable Bait")]
    [SerializeField] private GameObject rBaitPrefab;
    [SerializeField] private float rBaitCooldown;
    private float rBaitTimer;
    private bool rBaitEnabled = true;

    // Current cast state
    [HideInInspector] public CastingPlayerState currentCast;

    private void OnEnable()
    {
        ActionManager.Instance.onThrowStone += PrepareStoneThrow;
        ActionManager.Instance.onWhistle += PrepareWhistle;
        ActionManager.Instance.onThrowIBait += PrepareIBaitThrow;
        ActionManager.Instance.onRBait += PlaceRBait;
    }

    private void OnDisable()
    {
        ActionManager.Instance.onThrowStone -= PrepareStoneThrow;
        ActionManager.Instance.onWhistle -= PrepareWhistle;
        ActionManager.Instance.onThrowIBait -= PrepareIBaitThrow;
        ActionManager.Instance.onRBait -= PlaceRBait;
    }

    private void Update()
    {
        // Update ability timers
        UpdateTimers();
    }

    private void UpdateTimers()
    {
        // stone throw
        if (stoneThrowTimer > 0)
        {
            stoneThrowTimer -= Time.deltaTime;
            if (stoneThrowTimer <= 0)
            {
                stoneThrowEnabled = true;
            }
        }

        // whistle
        if (whistleTimer > 0)
        {
            whistleTimer -= Time.deltaTime;
            if(whistleTimer <= 0)
            {
                whistleEnabled = true;
            }
        }

        // rBait
        if (rBaitTimer > 0)
        {
            rBaitTimer -= Time.deltaTime;
            if (rBaitTimer <= 0)
            {
                rBaitEnabled = true;
            }
        }
    }

    public void PrepareStoneThrow()
    {
        if (!stoneThrowEnabled) return;

        currentCast = new CastingPlayerState(controller, controller.playerVariables, CastTypes.Stone, controller.IsCrouching(),
            stoneThrowRadius, stoneThrowNoiseRadius, raycastHeight);
        controller.SetCast(true);

    }

    public void ResetStoneThrowTimer()
    {
        stoneThrowTimer = stoneThrowCooldown;
        stoneThrowEnabled = false;
    }

    public void PrepareWhistle()
    {
       if (!whistleEnabled) return;
       currentCast = new CastingPlayerState(controller, controller.playerVariables, CastTypes.Whistle, controller.IsCrouching(),
            stoneThrowRadius, stoneThrowNoiseRadius, raycastHeight);
       controller.SetCast(true);

    }

    public void ResetWhistleTimer()
    {
        whistleTimer = whistleCooldown;
        whistleEnabled = false;
    }

    public void PrepareIBaitThrow()
    {
        if (iBaitCount <= 0) return;

        currentCast = new CastingPlayerState(controller, controller.playerVariables, CastTypes.IBait, controller.IsCrouching(),
            stoneThrowRadius, stoneThrowNoiseRadius, raycastHeight);
        controller.SetCast(true);
    }

    public void ConsumeIBait()
    {
        iBaitCount--;
    }

    public void PlaceRBait()
    {
        if (!rBaitEnabled) return;

        currentCast = new CastingPlayerState(controller, controller.playerVariables, CastTypes.RBait, controller.IsCrouching(),
            stoneThrowRadius, stoneThrowNoiseRadius, raycastHeight);
        controller.SetCast(true);
    }

    public void ResetRBaitTimer()
    {
        rBaitTimer = rBaitCooldown;
        rBaitEnabled = false;
    }


}
