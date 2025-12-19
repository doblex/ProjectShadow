public interface IInteractable
{
    void Interact();
}

public interface IBooleanInteractable : IInteractable
{
    bool Activated { get; set; }
}

