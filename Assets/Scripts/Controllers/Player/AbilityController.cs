using UnityEngine;

public class AbilityController : MonoBehaviour
{
    // Throw height
    [Header("Throw Height")]
    [SerializeField] private float raycastHeight; // horizontal raycast height from player center to determine if skill can be cast

    // Use linerenderer for throwing arc display

    //Throwable stone
    [Header("Throwable Stone")]
    [SerializeField] private GameObject throwableStonePrefab;
    [SerializeField] private float stoneThrowRadius;
    [SerializeField] private float stoneThrowSpeed;
    [SerializeField] private float stoneThrowHeight;
    [SerializeField] private float stoneThrowCooldown;
    // TODO add soundOptions for ability
    private float stoneThrowTimer;
    private bool stoneThrowEnabled;

    //Whistle
    [Header("Whistle")]
    [SerializeField] private float whistleRadius;

    private void OnEnable()
    {
        ActionManager.Instance.onThrowStone += PrepareStoneThrow;
    }

    private void OnDisable()
    {
        ActionManager.Instance.onThrowStone -= PrepareStoneThrow;
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
    }

    public void PrepareStoneThrow()
    {
        if (!stoneThrowEnabled) return;
        
        // TODO make player enter Casting state, display range and throwing arc preview,
        // then use PlayerState to make a Casting state and decide what to throw/execute the throw
        

    }
}
