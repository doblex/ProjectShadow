using UnityEngine;

public class IBait : MonoBehaviour
{
    [SerializeField] private NoiseOptions iBaitSound;
    Vector3 destination;
    float speed;

    public IBait SetDestination(Vector3 _destination)
    {
        destination = new Vector3(_destination.x, _destination.y + .25f, _destination.z);
        return this;
    }

    public IBait SetSpeed(float _speed)
    {
        speed = _speed;
        return this;
    }

    void Start()
    {
        
    }

    void Update()
    {
        // move towards destination
        if (Vector3.Distance(transform.position, destination) < 0.001f) return;
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
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
