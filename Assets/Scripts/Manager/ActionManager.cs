using UnityEngine;
using UnityEngine.InputSystem;

public class ActionManager : MonoBehaviour
{
    public static ActionManager Instance;

    public delegate void OnInteract();
    
    public OnInteract onInteract;

    [SerializeField] private InputActionAsset _action;

    private InputAction InteractAction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else 
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    private void Start()
    {
        SetupCommands();
    }

    private void Update()
    {
        CheckForCommands();
    }

    private void SetupCommands()
    {
        InteractAction = InputSystem.actions.FindAction("Interact");
    }

    private void CheckForCommands()
    {
        if (InteractAction.IsPressed())
        {
            onInteract.Invoke();
        }
    }
}
