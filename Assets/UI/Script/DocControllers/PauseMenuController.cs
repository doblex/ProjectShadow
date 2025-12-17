using UnityEngine.UIElements;

public class PauseMenuController : BaseDocController
{
    Button retryButton;
    Button optionsButton;
    Button mainMenuButton;
    Button exitButton;

    protected override bool SetComponents()
    {
        bool bInit = base.SetComponents();

        retryButton = Root.Q<Button>("Continue");
        retryButton.clicked += Retry;

        optionsButton = Root.Q<Button>("Options");
        optionsButton.clicked += Options;

        mainMenuButton = Root.Q<Button>("MainMenu");
        mainMenuButton.clicked += ToMainMenu;
        
        exitButton = Root.Q<Button>("Exit");
        exitButton.clicked += Exit;

        return bInit;
    }

    private void ToMainMenu()
    {
        ((UIController)UiController).LoadMainMenu();
    }

    private void Retry()
    {
        ((UIController)UiController).ReloadState();
    }
    private void Options()
    {
        ((UIController)UiController).ShowOptions(true);
    }
    private void Exit()
    {
        UiController.QuitGame();
    }
}
