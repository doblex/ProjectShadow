using UnityEngine;
using UnityEngine.InputSystem;

public class TriggerSpawner : MonoBehaviour
{
    public GameObject investigationTriggerPrefab;
    public LayerMask groundLayer;
    public float defaultRadius = 3f;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
            {
                Vector3 spawnPosition = hit.point + Vector3.up * 0.1f;
                GameObject trigger = Instantiate(investigationTriggerPrefab, spawnPosition, Quaternion.identity);
                trigger.transform.localScale = Vector3.one * defaultRadius * 2f;
                Debug.Log("Sfera trigger creata sul terreno a: " + spawnPosition);
            }
        }
    }
}
