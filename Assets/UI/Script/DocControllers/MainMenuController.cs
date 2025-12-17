using System;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : BaseDocController
{
    Button newGameButton;
    Button continueButton;
    Button optionButton;
    Button creditsButton;
    Button exitButton;

    protected override bool SetComponents()
    {
        bool bInit = base.SetComponents();

        newGameButton = Root.Q<Button>("NewGame");
        newGameButton.clicked += StartGame;

        continueButton = Root.Q<Button>("Continue");
        continueButton.clicked += ContinueGame;

        optionButton = Root.Q<Button>("Options");
        optionButton.clicked += Options;

        creditsButton = Root.Q<Button>("Credits");
        creditsButton.clicked += Credits;

        exitButton = Root.Q<Button>("Exit");
        exitButton.clicked += Exit;

        return bInit;
    }

    private void StartGame()
    {
        ((UIController)UiController).LoadLevel();
    }

    private void ContinueGame()
    {
        ShowDoc(false);
    }

    private void Options()
    {
        ((UIController)UiController).ShowOptions(true);
    }

    private void Credits()
    {
        ((UIController)UiController).ShowCredits(true);
    }

    private void Exit()
    {
        UiController.QuitGame();
    }
}
