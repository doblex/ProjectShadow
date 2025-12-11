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
        AIController ai = other.GetComponentInParent<AIController>();
        if (ai != null)
        {
            ai.OnSoundHeard(transform.position);
        }
    }
}
