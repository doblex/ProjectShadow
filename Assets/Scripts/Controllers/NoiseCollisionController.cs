using UnityEngine;

public class NoiseCollisionController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        { 
            GameObject enemy = other.gameObject;
            if (enemy.TryGetComponent<AIController>(out AIController ai))
            { 
                ai.OnSoundHeard(transform.parent.position);
            }
        }
    }
}
