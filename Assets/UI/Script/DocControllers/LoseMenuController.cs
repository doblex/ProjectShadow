using System;
using UnityEngine.UIElements;

public class LoseMenuController : BaseDocController
{
    Button retryButton;
    Button mainMenuButton;
    Button optionButton;
    Button exitButton;
    protected override bool SetComponents()
    {
        bool bInit = base.SetComponents();

        retryButton = Root.Q<Button>("Retry");
        retryButton.clicked += Retry;

        mainMenuButton = Root.Q<Button>("MainMenu");
        mainMenuButton.clicked += ToMainMenu;

        optionButton = Root.Q<Button>("Options");
        optionButton.clicked += Options;

        exitButton = Root.Q<Button>("Exit");
        exitButton.clicked += Exit;

        return bInit;
    }

    public override void ShowDoc(bool show, bool force = false)
    {
        base.ShowDoc(show, force);

        UiController.Pause(show);
    }

    private void Retry()
    {
        ShowDoc(false);
        ((UIController)UiController).ReloadState();
    }
    private void Options()
    {
        ((UIController)UiController).ShowOptions(true);
    }

    private void ToMainMenu()
    {
        ShowDoc(false);
        ((UIController)UiController).LoadMainMenu();
    }

    private void Exit()
    {
        UiController.QuitGame();
    }
}
