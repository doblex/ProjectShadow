using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TutorialTrigger : MonoBehaviour
{
    [SerializeField,TextArea] string TaskText;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ((UIController)UIController.Instance).AddTask(TaskText);
        }
    }
}
