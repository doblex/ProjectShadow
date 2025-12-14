using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IBait : MonoBehaviour
{
    [SerializeField] private NoiseOptions iBaitSound;
    private Vector3 origin;
    private Vector3 apex;
    private Vector3 destination;
    private float speed;
    private float throwHeight;
    Rigidbody rb;

    int samples = 30; // number of sampling points
    List<Vector3> pointList = new List<Vector3>();
    int pointIndex = 0;

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
        if (Vector3.Distance(transform.position, destination) <= .25f)
        {
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            reached = true;
        }

        // movement code
        else if (Vector3.Distance(transform.position, pointList[pointIndex]) > .25f)
        {
            transform.position = Vector3.MoveTowards(transform.position, pointList[pointIndex], speed * Time.deltaTime);
        }
        else if (pointIndex < pointList.Count - 1)
        {
            pointIndex++;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
        }
    }

    public void Despawn()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        reached = true;
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
