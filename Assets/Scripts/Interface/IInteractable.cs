public interface IInteractable
{
    void Interact();
}

public interface IBooleanInteractable : IInteractable
{
    bool Active { get; set; }
}

