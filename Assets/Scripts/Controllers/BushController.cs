using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BushController : MonoBehaviour
{
    private void Awake()
    {
        Collider sc = GetComponent<Collider>();
        sc.isTrigger = true;
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.TryGetComponent<PlayerController>(out PlayerController pc))
        {
            pc.SetHiding(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<PlayerController>(out PlayerController pc))
        {
            pc.SetHiding(false);
        }
    }
}
