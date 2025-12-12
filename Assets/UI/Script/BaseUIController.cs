using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BaseUIController : MonoBehaviour
{
    [Header("Docs")]
    [SerializeField] List<BaseDocController> docControllers = new List<BaseDocController>();
    HashSet<BaseDocController> activedDocs = new HashSet<BaseDocController>();

    [Header("Templates")]
    [SerializeField] List<Template> templates = new List<Template>();

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

    /// <summary>
    /// Shows the document with the specified name. If the document is already visible, it does nothing.
    /// </summary>
    /// <param name="docName"></param>
    public void ShowDoc(string docName)
    {
        foreach (BaseDocController controller in docControllers)
        {
            if (controller.DocName == docName)
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
                else
                {
                    Debug.LogWarning($"Document {docName} is already visible.");
                }
            }
        }
    }

    /// <summary>
    /// Hides the document with the specified name. If the document is not currently visible, it does nothing.
    /// </summary>
    /// <param name="docName"></param>
    public void HideDoc(string docName)
    {
        foreach (BaseDocController controller in activedDocs)
        {
            if (controller.DocName == docName)
            {
                controller.ShowDoc(false);
                activedDocs.Remove(controller);
                return;
            }
        }

        Debug.LogWarning($"Document {docName} is not currently active.");
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

