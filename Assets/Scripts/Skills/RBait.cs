using System.Collections;
using Unity.Loading;
using UnityEngine;
using UnityEngine.AI;

public class RBait : MonoBehaviour, ISaveable
{
    [SerializeField] private NoiseOptions iBaitSound;

    Rigidbody rb;

    private string id;

    private bool loading = false;

    public string ID => id;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        id = System.Guid.NewGuid().ToString();
    }

    public void Despawn()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        StartCoroutine(StopOnlanding());
        if (loading)
        {
            loading = false;
            return;
        }
        NoiseSpawnerManager.Instance.SpawnNoiseOrigin(transform.position, iBaitSound);
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

    private struct RBaitData
    {
        public Vector3 position;
        public Vector3 rotation;
    }

    public object Save()
    {
        return new RBaitData
        {
            position = this.transform.position,
            rotation = this.transform.eulerAngles,
        };
    }

    public void Load(string stateJson)
    {
        RBaitData data = JsonUtility.FromJson<RBaitData>(stateJson);

        // apply variables
        transform.position = data.position;
        transform.eulerAngles = data.rotation;
        loading = true;
    }
}
