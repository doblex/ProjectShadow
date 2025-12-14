using UnityEngine;
using System.Collections;

public class RBait : MonoBehaviour
{
    [SerializeField] private NoiseOptions iBaitSound;

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Despawn()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        NoiseSpawnerManager.Instance.SpawnNoiseOrigin(transform.position, iBaitSound);
        StartCoroutine(StopOnlanding());
    }

    private IEnumerator StopOnlanding()
    {
        float oldDrag = rb.linearDamping;
        float oldAngularDrag = rb.angularDamping;

        // Se non faccio sta cazzata Unity continua a far rimbalzare la sfera in verticale, non capirò mai sto engine dimmerda
        rb.linearDamping = 1000f;
        rb.angularDamping = 1000f;

        yield return new WaitForSeconds(0.5f);

        rb.linearDamping = oldDrag;
        rb.angularDamping = oldAngularDrag;
    }
}
