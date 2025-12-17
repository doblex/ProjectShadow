public class UIController : BaseUIController
{
    public static UIController Instance;

    protected override void Awake()
    {
        base.Awake();

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
            ShowLoading(false);
            ShowMainMenu(true);
        });
    }

    public void LoadLevel()
    {
        ShowLoading(true);
        LevelLoaderManager.Instance.StartLevelLoading(() =>
        {
            ShowLoading(false);
            ShowHud(true);
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
        ShowDoc("PauseMenu", show);
    }

    public void ShowHud(bool show)
    {
        ShowDoc("HUD", show);
    }


    #endregion

    public void ReloadState()
    { 
        PersistenceManager.Instance.LoadRequest();
    }
}

