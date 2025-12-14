using UnityEngine;

public class RBait : MonoBehaviour
{
    [SerializeField] private NoiseOptions iBaitSound;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Despawn()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        NoiseSpawnerManager.Instance.SpawnNoiseOrigin(transform.position, iBaitSound);
    }
}
