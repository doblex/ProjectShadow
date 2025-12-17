using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenuController : BaseDocController
{
    Button retryButton;
    Button optionsButton;
    Button mainMenuButton;

    protected override bool SetComponents()
    {
        bool bInit = base.SetComponents();

        retryButton = Root.Q<Button>("Retry");
        retryButton.clicked += Retry;

        optionsButton = Root.Q<Button>("Options");
        optionsButton.clicked += Options;

        mainMenuButton = Root.Q<Button>("MainMenu");
        mainMenuButton.clicked += ToMainMenu;

        return bInit;
    }

    public override void ShowDoc(bool show, bool force = false)
    {
        base.ShowDoc(show, force);

        UiController.Pause(show);
    }

    private void Retry()
    {
        ActionManager.Instance.OnPause();
        ((UIController)UiController).ReloadState();
    }
    private void Options()
    {
        ((UIController)UiController).ShowOptions(true);
    }

    private void ToMainMenu()
    {
        ActionManager.Instance.OnPause();
        ((UIController)UiController).LoadMainMenu();
    }
}
