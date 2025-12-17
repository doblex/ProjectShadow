using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoaderManager : MonoBehaviour
{
    public static LevelLoaderManager Instance;

    [SerializeField] SceneAsset MenuScene;
    [SerializeField] SceneAsset ManagersScene;
    [SerializeField] SceneAsset Level;

    AsyncOperation managerAOp;
    AsyncOperation levelAOp;

    bool isLoadingLevel = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// Loads the menu scene and invokes a callback after the scene has finished loading.
    /// </summary>
    /// <param name="OnAfterLoad">An action to invoke after the menu scene has been loaded. Can be null if no callback is required.</param>
    public void LoadMenuScene(Action OnAfterLoad)
    {
        LoadScene(MenuScene, false, OnAfterLoad);
    }

    /// <summary>
    /// Begins loading the current level and invokes a callback after the load completes.
    /// </summary>
    /// <param name="OnAfterLoad">An action to invoke after the level has finished loading. Can be null if no callback is required.</param>
    public void StartLevelLoading(Action OnAfterLoad)
    {
        LoadScene(Level, true, OnAfterLoad);
    }

    private void LoadScene(SceneAsset sceneToLoad, bool loadManagerScene, Action OnAfterLoad)
    {
        if (isLoadingLevel)
        { 
            Debug.LogWarning("A level is already being loaded. Please wait until the current loading process is complete.");
            return;
        }

        isLoadingLevel = true;
        StartCoroutine(LoadLevelAsync(sceneToLoad, loadManagerScene, OnAfterLoad));
    }

    /// <summary>
    /// Asynchronously loads the specified level scene, optionally loading the manager scene first, and unloads the
    /// current active scene upon completion.
    /// </summary>
    /// <remarks>This method loads the specified level scene additively and delays scene activation until
    /// loading is nearly complete. If loadManagerScene is true, the manager scene is loaded additively before the level
    /// scene. The current active scene is unloaded after the new scenes are loaded. This method is intended to be used
    /// as a coroutine.</remarks>
    /// <param name="sceneToLoad">The scene asset representing the level to load. Must not be null.</param>
    /// <param name="loadManagerScene">true to load the manager scene before loading the level; otherwise, false.</param>
    /// <param name="OnAfterLoad">An optional callback invoked after the level has been loaded and the current scene unloaded.</param>
    private IEnumerator LoadLevelAsync(SceneAsset sceneToLoad, bool loadManagerScene, Action OnAfterLoad)
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (loadManagerScene)
        {
            managerAOp = SceneManager.LoadSceneAsync(ManagersScene.name, LoadSceneMode.Additive);

            while (!managerAOp.isDone)
            {
                yield return null;
            }
        }

        LoadSceneMode mode = loadManagerScene ? LoadSceneMode.Additive : LoadSceneMode.Single;

        levelAOp = SceneManager.LoadSceneAsync(sceneToLoad.name, mode);
        levelAOp.allowSceneActivation = false;

        while (levelAOp.progress < 0.9f)
        {
            yield return null;
        }

        levelAOp.allowSceneActivation = true;

        if (loadManagerScene)
        {
            AsyncOperation Op = SceneManager.UnloadSceneAsync(currentScene);

            while (!Op.isDone)
            {
                yield return null;
            }
        }

        OnAfterLoad?.Invoke();
        isLoadingLevel = false;
    }
}
