public class UIController : BaseUIController
{
    private void Start()
    {
        ShowMainMenu(true);
    }

    #region private methods
    private void ShowLoading(bool show)
    { 
        ShowDoc("Loading", show);
    }
#endregion
    #region loading scenes
    public void LoadMainMenu()
    {
        ShowLoading(true);
        LevelLoaderManager.Instance.LoadMenuScene(() =>
        {
            CheckAllDocs();

            ActionManager.Instance.onPauseGame -= ShowPauseMenu;
            ShowMainMenu(true);
            ShowLoading(false);
        });
    }

    public void LoadLevel()
    {
        ShowLoading(true);
        LevelLoaderManager.Instance.StartLevelLoading(() =>
        {
            CheckAllDocs();

            ActionManager.Instance.onPauseGame += ShowPauseMenu;
            ShowHud(true);
            ShowLoading(false);
        });
    }

    #endregion
    #region show/hide docs


    public void ShowMainMenu(bool show)
    {
        ShowDoc("MainMenu", show);
    }

    public void ShowOptions(bool show)
    {
        ShowDoc("Options", show);
    }

    public void ShowPauseMenu(bool show)
    {
        ShowDoc("Pause", show);
    }

    public void ShowHud(bool show)
    {
        ShowDoc("Hud", show);
    }

    public void ShowCredits(bool show)
    {
        ShowDoc("Credits", show);
    }

    public void ShowLose(bool show)
    {
        ShowDoc("Lose", show);
    }

    public void AddTask(string text)
    {
        HudController controller = GetDoc<HudController>("Hud");

        controller.AddTask(text);
    }


    #endregion

    public void ReloadState()
    { 
        PersistenceManager.Instance?.LoadRequest();
    }


}

