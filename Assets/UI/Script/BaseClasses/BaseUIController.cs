using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

public class BaseUIController : MonoBehaviour
{
    public static BaseUIController Instance;

    [Header("Docs")]
    [SerializeField] List<BaseDocController> docControllers = new List<BaseDocController>();
    HashSet<BaseDocController> activedDocs = new HashSet<BaseDocController>();

    [Header("Templates")]
    [SerializeField] List<Template> templates = new List<Template>();

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        foreach (BaseDocController controller in docControllers)
        {
            controller.Init(this);
        }

        SetAllDocsHidden();
    }

    /// <summary>
    /// Exits the application and stops play mode in the Unity Editor.
    /// </summary>
    /// <remarks>In a built application, this method closes the application. When running in the Unity Editor,
    /// it stops play mode instead of closing the editor. This method has no effect in WebGL builds.</remarks>
    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void Pause(bool pauseStatus)
    { 
        Time.timeScale = pauseStatus ? 0f : 1f;
    }

    /// <summary>
    /// Retrieves a template by its name.
    /// </summary>
    /// <param name="templateName"></param>
    /// <param name="asset">populated with the Asset found, in other case is null</param>
    /// <returns>return true if found</returns>
    public bool GetTemplate(string templateName, out VisualTreeAsset asset)
    {
        asset = null;

        foreach (Template template in templates)
        {
            if (template.templateName == templateName)
            {
                Debug.Log($"Template found: {templateName}");
                asset = template.asset;
                return true;
            }
        }

        return false;
    }

    public T GetDoc<T>(string docName) where T : BaseDocController
    { 
        T doc = null;

        foreach (BaseDocController controller in docControllers)
        {
            if (controller.DocName == docName)
            {
                doc = (T)controller;
                break;
            }
        }

        return doc;
    }

    /// <summary>
    /// Shows the document with the specified name. If the document is already visible, it does nothing.
    /// </summary>
    /// <param name="docName"></param>
    /// <param name="show">true to show, false to hide</param>
    public void ShowDoc(string docName, bool show)
    {
        bool bFound = false;

        foreach (BaseDocController controller in docControllers)
        {
            if (controller.DocName == docName)
            {
                if (show)
                {
                    ShowDoc(controller);
                }
                else
                { 
                    HideDoc(controller);
                }

                bFound = true;
            }
        }

        if(!bFound)
        {
            Debug.LogWarning($"ShowDoc: Document '{docName}' not found.");
            return;
        }   

        Debug.Log($"ShowDoc: {docName} - {show}");
    }

    private void ShowDoc(BaseDocController controller)
    {
        if (controller.DocumentState == DocumentState.Hidden)
        {
            controller.ShowDoc(true);

            if (controller.DocBehaviour == DocBehavior.single)
            {
                SetAllDocsHidden();
            }

            AddActiveDoc(controller);
        }
    }

    private void HideDoc(BaseDocController controller)
    {
        controller.ShowDoc(false);
        activedDocs.Remove(controller);
    }

    private void SetAllDocsHidden()
    {
        foreach (BaseDocController controller in activedDocs)
        {
            controller.ShowDoc(false, true);
        }

        activedDocs.Clear();
    }

    private void AddActiveDoc(BaseDocController controller)
    {
        controller.Doc.sortingOrder = activedDocs.Count + 1;
        activedDocs.Add(controller);
    }
}

