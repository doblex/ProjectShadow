using UnityEngine;

public class NoiseCollisionController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            AIController ai = other.GetComponentInParent<AIController>();
            if (ai != null)
            {
                ai.OnSoundHeard(transform.position);
            }
        }
    }
}
