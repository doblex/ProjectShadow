using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
/// <summary>
/// Base class for UI Document controllers.
/// </summary>

[RequireComponent(typeof(UIDocument))]
public abstract class BaseDocController : MonoBehaviour
{
    [Header("Doc options")]
    [SerializeField] private string docName = "BaseDocController";
    [SerializeField] private DocBehavior docBehaviour = DocBehavior.single;

    [Header("Transition Options")]
    [SerializeField] protected bool canFade = true;
    [SerializeField][ShowIf("canFade")] protected float fadeInDuration = 0.5f;
    [SerializeField][ShowIf("canFade")] protected float fadeOutDuration = 0.5f;

    [Header("Debugging")]
    [SerializeField] private DocumentState documentState = DocumentState.Hidden;
    UIDocument doc;
    VisualElement root;

    protected Coroutine currentFadeRoutine;

    /// <summary>
    /// Reference to the UIDocument component.
    /// </summary>
    public UIDocument Doc { get => doc; }

    /// <summary>
    /// Reference to the root VisualElement of the UI Document.
    /// </summary>
    public VisualElement Root { get => root; protected set => root = value; }

    /// <summary>
    /// Name of the UI Document, used for identification and debugging purposes.
    /// </summary>
    public string DocName { get => docName; set => docName = value; }
    /// <summary>
    /// Current state of the document, used to track visibility and transitions.
    /// </summary>
    public DocumentState DocumentState { get => documentState; }
    /// <summary>
    /// Behavior of the document, whether it allows single or multiple instances.
    /// </summary>
    public DocBehavior DocBehaviour { get => docBehaviour; }

    protected virtual void Awake()
    {
        doc = GetComponent<UIDocument>();
        root = doc.rootVisualElement;

        if (root != null)
            SetComponents();
    }

    /// <summary>
    /// Override this method to set up the components of the UI Document.
    /// </summary>
    protected abstract void SetComponents();

    /// <summary>
    /// Shows or hides the UI Document with a fade effect if enabled.
    /// </summary>
    /// <param name="show"></param>
    public virtual void ShowDoc(bool show, bool force = false)
    {
        if (currentFadeRoutine != null)
            StopCoroutine(currentFadeRoutine);

        if (!force && canFade)
        {
            if (show)
            {
                currentFadeRoutine = StartCoroutine(FadeIn());
            }
            else
            {
                currentFadeRoutine = StartCoroutine(FadeOut());
            }
        }
        else
        {
            root.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            documentState = show ? DocumentState.Visible : DocumentState.Hidden;
        }
    }

    #region Coroutines

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        float startOpacity = Root.style.opacity.value;
        float endOpacity = 1f;

        Root.style.display = DisplayStyle.Flex;

        documentState = DocumentState.Transitioning;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float newOpacity = Mathf.Lerp(startOpacity, endOpacity, elapsed / fadeInDuration);
            Root.style.opacity = newOpacity;
            yield return null;
        }

        Root.style.opacity = endOpacity;
        documentState = DocumentState.Visible;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float startOpacity = Root.style.opacity.value;
        float endOpacity = 0f;

        documentState = DocumentState.Transitioning;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float newOpacity = Mathf.Lerp(startOpacity, endOpacity, elapsed / fadeOutDuration);
            Root.style.opacity = newOpacity;
            yield return null;
        }

        Root.style.opacity = endOpacity;
        Root.style.display = DisplayStyle.None;
        documentState = DocumentState.Hidden;
    }

    #endregion
}
