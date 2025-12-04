using UnityEngine;

public class SphereTrigger : MonoBehaviour
{
    public float triggerDuration = 0.1f;

    void Start()
    {
        Destroy(gameObject, triggerDuration);
    }

    void OnTriggerEnter(Collider other)
    {
        AIController ai = other.GetComponent<AIController>();
        if (ai != null)
        {
            ai.StartInvestigation(transform.position);
            Debug.Log("Enemy triggered investigation.");
        }
    }
}
