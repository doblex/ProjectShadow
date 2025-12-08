using UnityEngine;

public class AbilityController : MonoBehaviour
{
    //Throwable stone
    [Header("Throwable Stone")]
    [SerializeField] private GameObject throwableStonePrefab;
    [SerializeField] private float stoneThrowRadius;
    [SerializeField] private float stoneThrowSpeed;
    [SerializeField] private float stoneThrowHeight;
    [SerializeField] private float stoneThrowCooldown;
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
        UpdateTImers();
    }

    private void UpdateTImers()
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
        
        //TODO Display range and throwing arc preview, then intercept ActionManager delegate for left click to confirm instead of move?
        

    }
}
