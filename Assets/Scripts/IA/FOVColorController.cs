using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class FOVColorController : MonoBehaviour
{
    public AIController owner;

    [Header("Layer type")]
    public bool isInner = false;

    [Header("Base Colors")]
    public Color patrolOuter = new Color(0f, 0f, 0f, 0.2f);
    public Color patrolInner = new Color(0.1f, 0.1f, 0.1f, 0.3f);

    public Color alarmOuter = new Color(0.5f, 0f, 0f, 0.3f);
    public Color alarmInner = new Color(1f, 0.1f, 0.1f, 0.6f);

    public Color invOuter = new Color(0.5f, 0.5f, 0f, 0.3f);
    public Color invInner = new Color(1f, 1f, 0.1f, 0.6f);

    MeshRenderer rend;
    Material mat;

    void Awake()
    {
        rend = GetComponent<MeshRenderer>();
        mat = new Material(rend.sharedMaterial);
        rend.material = mat;
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rend.receiveShadows = false;
    }

    void Update()
    {
        if (owner == null) return;

        Color baseColor;
        Color fillColor;
        float fillAmount = 0f;

        switch (owner.phase)
        {
            case AIController.Phase.Patrol:
                baseColor = isInner ? patrolInner : patrolOuter;
                fillAmount = 0f;
                break;

            case AIController.Phase.Alarm:
                baseColor = isInner ? alarmInner : alarmOuter;
                fillAmount = 1f;
                break;

            case AIController.Phase.Investigation:
                baseColor = isInner ? invInner : invOuter;
                if (owner.investigationTime > 0f)
                    fillAmount = 1f - (owner.investigationTimer / owner.investigationTime);
                break;

            default:
                baseColor = isInner ? patrolInner : patrolOuter;
                fillAmount = 0f;
                break;
        }
        fillColor = baseColor * 1.2f;
        fillColor.a = Mathf.Clamp01(baseColor.a + 0.2f);
        mat.SetColor("_BaseColor", baseColor);
        mat.SetColor("_FillColor", fillColor);
        mat.SetFloat("_FillAmount", Mathf.Clamp01(fillAmount));
    }
}
