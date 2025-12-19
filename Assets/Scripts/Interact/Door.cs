using System.Collections;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Transforms")]
    [SerializeField] private Transform doorLeft;
    [SerializeField] private Transform doorRight;

    [Header("Animation Settings")]
    [SerializeField] private AnimationCurve doorCurve;
    [SerializeField] private float maxAngle = 60f;
    [SerializeField] private float duration = 1f;

    private bool isAnimating = false;
    private bool isOpen = false;

    public bool Activated;

    VideoPlayer Vp;

    private void Awake()
    {
        Vp = GetComponent<VideoPlayer>();
    }

    private void Update()
    {
        if (Activated)
        {
            Activated = false;
            Interact();
        }
    }

    public void Interact()
    {
        if (!isAnimating)
        {
            StartCoroutine(AnimateDoor(!isOpen));
        }
    }

    private IEnumerator AnimateDoor(bool opening)
    {
        isAnimating = true;

        float elapsed = 0f;
        float startAngle = isOpen ? maxAngle : 0f;
        float endAngle = opening ? maxAngle : 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curveT = doorCurve.Evaluate(t);
            float currentAngle = Mathf.Lerp(startAngle, endAngle, curveT);

            doorLeft.localRotation = Quaternion.Euler(0, currentAngle, 0);
            doorRight.localRotation = Quaternion.Euler(0, -currentAngle, 0);

            yield return null;
        }

        // Ensure final rotation
        doorLeft.localRotation = Quaternion.Euler(0, endAngle, 0);
        doorRight.localRotation = Quaternion.Euler(0, -endAngle, 0);

        isOpen = opening;
        isAnimating = false;

        // Optional: trigger cutscene when opening
        if (opening)
        {
            Play();
        }
    }

    public void Play()
    {
        Vp.Play();

        Vp.loopPointReached += Vp_loopPointReached;
    }

    private void Vp_loopPointReached(VideoPlayer source)
    {
        LevelLoaderManager.Instance.LoadMenuScene(null);
    }
}
