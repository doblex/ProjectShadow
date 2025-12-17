using UnityEngine.UIElements;

public class CreditsController : BaseDocController
{
    Button backButton;
    protected override bool SetComponents()
    {
        bool bInit = base.SetComponents();

        backButton = Root.Q<Button>("Back");
        backButton.clicked += Back;

        return bInit;
    }
    private void Back()
    {
        ShowDoc(false);
    }
}
