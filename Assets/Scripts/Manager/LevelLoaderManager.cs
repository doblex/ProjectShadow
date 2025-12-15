using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoaderManager : MonoBehaviour
{
    public static LevelLoaderManager Instance;

    public bool fireEvent;

    [SerializeField] SceneAsset ManagersScene;
    [SerializeField] SceneAsset SceneToLoad;

    AsyncOperation managerAOp;
    AsyncOperation levelAOp;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        if (fireEvent)
        {
            fireEvent = false;
            StartSceneTransition();
        }
    }


    public void StartSceneTransition()
    {
        StartCoroutine( LoadLevelAsync());
    }

    private IEnumerator LoadLevelAsync()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        managerAOp = SceneManager.LoadSceneAsync(ManagersScene.name, LoadSceneMode.Additive);
        while (!managerAOp.isDone)
        {
            yield return null;
        }

        levelAOp = SceneManager.LoadSceneAsync(SceneToLoad.name, LoadSceneMode.Additive);
        levelAOp.allowSceneActivation = false;

        while (levelAOp.progress < 0.9f)
        {
            yield return null;
        }

        levelAOp.allowSceneActivation = true;

        AsyncOperation Op = SceneManager.UnloadSceneAsync(currentScene);

        while (!Op.isDone)
        {
            yield return null;
        }
    }
}
