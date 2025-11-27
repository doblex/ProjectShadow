using System;
using System.Collections;
using UnityEngine;

public class SoundOrigin : MonoBehaviour
{
    [SerializeField] private float baseScaling = 0.1f;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private float scaleDuration = 1f;

    [SerializeField] private AnimationCurve scalingCurve;
    [SerializeField] private bool useCurve = false;

    [SerializeField] private GameObject visualRepresentation;


    private GameObject currentGO;

    private void Start()
    {
        ActionManager.Instance.onInteract += ScaleSoundOrigin;
    }

    private void ScaleSoundOrigin()
    {
        if (currentGO == null)
        { 
            currentGO = Instantiate(visualRepresentation, transform.position, Quaternion.identity);
            currentGO.transform.localScale = Vector3.one * baseScaling;
            StartCoroutine(ScaleOverTime(Vector3.one * maxDistance, scaleDuration));
        }
    }

    IEnumerator ScaleOverTime(Vector3 targetScale, float duration)
    {
        Vector3 initialScale = currentGO.transform.localScale;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            if (useCurve)
            {
                currentGO.transform.localScale = Vector3.Lerp(initialScale, targetScale, scalingCurve.Evaluate(elapsedTime / duration));
            }
            else
            {
                currentGO.transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / duration);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        currentGO.transform.localScale = targetScale;

        Destroy(currentGO);
        currentGO = null;
    }
}
