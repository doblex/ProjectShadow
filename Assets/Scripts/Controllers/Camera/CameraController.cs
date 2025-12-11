using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _camera;

    [Header("Camera Bounds")]
    [SerializeField] private Vector2 Dimensions;

    private void OnEnable()
    {
        ActionManager.Instance.onMovementChanged += OnVisualPosChanged;
        ActionManager.Instance.onRotationChanged += OnVisualRotChanged;
    }

    private void OnDisable()
    {
        ActionManager.Instance.onMovementChanged -= OnVisualPosChanged;
        ActionManager.Instance.onRotationChanged -= OnVisualRotChanged;
    }

    private void Start()
    {
        _camera.transform.LookAt(transform.position);
    }

    private void OnVisualRotChanged(Quaternion rotation)
    {
        transform.rotation *= rotation;
    }

    private void OnVisualPosChanged(Vector3 movement)
    {
        movement = transform.rotation * movement;
        Vector3 newPos = transform.position + movement;

        float halfWidth = Dimensions.x / 2f;
        float halfHeight = Dimensions.y / 2f;
        newPos.x = Mathf.Clamp(newPos.x, transform.parent.position.x - halfWidth, transform.parent.position.x + halfWidth);
        newPos.z = Mathf.Clamp(newPos.z, transform.parent.position.z - halfHeight, transform.parent.position.z + halfHeight);

        transform.position = newPos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.parent.position, new Vector3(Dimensions.x, 1, Dimensions.y));
    }
}
