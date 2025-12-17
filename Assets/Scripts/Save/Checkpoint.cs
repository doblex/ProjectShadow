using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private SaveSlot saveSlot;
    [SerializeField] private bool triggered = false;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, gameObject.transform.localScale);
    }

    public void ResetCheckpoint()
    {
        triggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        Debug.Log("Checkpoint activated!");
        PersistenceManager.Instance?.SaveRequest(saveSlot);
        triggered = true;
    }
}
