using System;

public class UIController : BaseUIController
{
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

    internal void ShowCredits(bool show)
    {
        ShowDoc("Credits", show);
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

