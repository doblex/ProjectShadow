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

    protected override void SetComponents()
    {
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
    }

    private void StartGame()
    {
        LevelLoaderManager.Instance.StartSceneTransition();
        ShowDoc(false);
    }

    private void ContinueGame()
    {
        ShowDoc(false);
    }

    private void Options()
    {

    }

    private void Credits()
    {
        
    }

    private void Exit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
