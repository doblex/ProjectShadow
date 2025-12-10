using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _camera;

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
        transform.position += movement;
    }
}
