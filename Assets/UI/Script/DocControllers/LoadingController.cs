using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LoadingController : BaseDocController
{
    [SerializeField] float waveHeight = 5f;
    [SerializeField] float waveSpeed = 5f;
    [SerializeField] float waveFrequency = 1f;

    VisualElement loadingAnimation;

    List<VisualElement> childElements = new List<VisualElement>();

    bool isAnimationPlaying = false;

    protected override bool SetComponents()
    {
        bool bInit = base.SetComponents();
        loadingAnimation = Root.Q<VisualElement>("Loading");

        childElements = new List<VisualElement>(loadingAnimation.Children());

        return bInit;
    }


    public override void ShowDoc(bool show, bool force = false)
    {
        base.ShowDoc(show, force);

        isAnimationPlaying = show;
    }

    private void Update()
    {
        if (isAnimationPlaying)
        {
            for (int i = 0; i < childElements.Count; i++)
            {
                var label = childElements[i];
                float offset = Mathf.Sin(Time.unscaledDeltaTime * waveSpeed + i * waveFrequency) * waveHeight;

                label.style.top = offset;
            }
        }
    }
}