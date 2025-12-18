using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Highlighter : MonoBehaviour
{
    public Material HLMaterial;

    private bool hasMat = false;

    List<Renderer> renderers = new List<Renderer>();


    private void Awake()
    {
        CollectChildComponentsRecursive<Renderer>(transform, ref renderers);
    }

    private void OnEnable()
    {
        ActionManager.Instance.onHighlight += ShowHighlight;
    }

    private void OnDisable()
    {
        ActionManager.Instance.onHighlight -= ShowHighlight;
    }

    public void ShowHighlight(bool active)
    {
        if(HLMaterial == null) return;
        
        if(renderers.Count <= 0) return;

        foreach (Renderer renderer in renderers)
        {
            if (active)
            {
                if (!hasMat)
                {
                    List<Material> materials =  renderer.sharedMaterials.ToList<Material>();
                    materials.Add(HLMaterial);
                    
                    renderer.sharedMaterials = materials.ToArray();
                }
            }
            else
            {
                if (hasMat)
                {
                    List<Material> materials = renderer.sharedMaterials.ToList<Material>();
                    materials.Remove(HLMaterial);

                    renderer.sharedMaterials = materials.ToArray();
                }
            }
        }

        hasMat = !hasMat;
    }

    public void CollectChildComponentsRecursive<T>(Transform root, ref List<T> result) where T : Component
    {
        foreach (Transform child in root)
        {
            // Prova a prendere il componente (può non esistere)
            if (child.TryGetComponent<T>(out var component))
            {
                result.Add(component);
            }

            // Continua SEMPRE la ricorsione
            CollectChildComponentsRecursive(child, ref result);
        }
    }
}