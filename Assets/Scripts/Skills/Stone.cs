using UnityEngine;

public class Stone : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private SoundOptions stoneSound;
    private Vector3 destination;
    private float speed;
    private float lifeTimer;

    public Stone SetDestination(Vector3 _destination)
    {
        destination = new Vector3(_destination.x, _destination.y + .25f, _destination.z);
        return this;
    }

    public Stone SetSpeed(float _speed)
    {
        speed = _speed;
        return this;
    }

    void Start()
    {
        lifeTimer = lifetime;
    }

    void Update()
    {
        if (lifeTimer > 0)
        {
            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0)
            {
                Destroy(gameObject);
            }

            // move towards destination
            if (Vector3.Distance(transform.position, destination) < 0.001f) return;
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collided, position {transform.position}");
        NoiseSpawnerManager.Instance.SpawnNoiseOrigin(collision.GetContact(0).point, stoneSound);
    }
}
