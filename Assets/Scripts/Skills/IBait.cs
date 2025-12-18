using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IBait : MonoBehaviour, ISaveable
{
    // Save system
    private string id;
    public string ID => id;
    private bool loading = false;

    [SerializeField] private NoiseOptions iBaitSound;
    [SerializeField] private Vector3 origin;
    private Vector3 apex;
    [SerializeField] private Vector3 destination;
    private float speed;
    private float throwHeight;
    Rigidbody rb;

    int samples = 30; // number of sampling points
    [SerializeField] List<Vector3> pointList = new List<Vector3>();
    [SerializeField] int pointIndex = 0;

    [SerializeField] bool reached = false;

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

    public IBait SetThrowHeight(float _throwHeight)
    {
        throwHeight = _throwHeight;
        return this;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        origin = transform.position;
        apex = .5f * (origin + destination);
        apex.y += throwHeight;

        for (float ratio = 0; ratio <= 1; ratio += 1.0f / samples)
        {
            Vector3 tangentLineVertex1 = Vector3.Lerp(origin, apex, ratio);
            Vector3 tangentLineVectex2 = Vector3.Lerp(apex, destination, ratio);
            Vector3 bezierPoint = Vector3.Lerp(tangentLineVertex1, tangentLineVectex2, ratio);
            pointList.Add(bezierPoint);
        }
    }

    void FixedUpdate()
    {
        // destination check
        if (reached) return;

        // movement code
        else if (Vector3.Distance(transform.position, pointList[pointIndex]) > .001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, pointList[pointIndex], speed * Time.fixedDeltaTime);
        }
        else if (pointIndex < pointList.Count - 1)
        {
            pointIndex++;
        }
        else
        {
            reached = true;
        }
        if (reached)
        {
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
        }

        Debug.Log("Moving...");
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
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        reached = true;
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

    private struct IBaitData
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 origin;
        public Vector3 destination;
        public int pointIndex;
        public bool reached;
        public float speed;
        public float throwHeight;
    }

    public object Save()
    {
        return new IBaitData
        {
            position = this.transform.position,
            rotation = this.transform.eulerAngles,
            origin = this.origin,
            destination = this.destination,
            pointIndex = this.pointIndex,
            reached = this.reached,
            speed = this.speed,
            throwHeight = this.throwHeight
        };
    }

    public void Load(string stateJson)
    {
        IBaitData data = JsonUtility.FromJson<IBaitData>(stateJson);

        // apply variables
        transform.position = data.position;
        transform.eulerAngles = data.rotation;
        origin = data.origin;
        destination = data.destination;
        pointIndex = data.pointIndex;
        reached = data.reached;
        speed = data.speed;
        throwHeight = data.throwHeight;

        loading = true;
    }
}
