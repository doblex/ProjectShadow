using UnityEngine;


public class Highlighter : MonoBehaviour
{
    public Material HLMaterial;
    public Color HLcolor;

    public bool overrideActive = false;
    private bool hasMat = false;

    private void OnEnable()
    {
        FindAnyObjectByType<ActionManager>().onHighlight += ShowHighlight;
    }

    private void OnDisable()
    {
        FindAnyObjectByType<ActionManager>().onHighlight -= ShowHighlight;
    }

    public void ShowHighlight(bool active)
    {
        if (active)
        {
            if (!hasMat)
            {
                Material[] mArray = new Material[(this.GetComponent<Renderer>().materials.Length + 1)];
                this.GetComponent<Renderer>().materials.CopyTo(mArray, 0);

                Material m = new Material(HLMaterial);
                m.color = HLcolor;
                mArray[mArray.Length - 1] = m;
                this.GetComponent<Renderer>().materials = mArray;
                hasMat = true;
            }
        }
        else
        {
            if (hasMat)
            {
                Material[] mArray = new Material[(this.GetComponent<Renderer>().materials.Length - 1)];
                for (int i = 0; i < this.GetComponent<Renderer>().materials.Length - 1; i++)
                {
                    mArray[i] = this.GetComponent<Renderer>().materials[i];
                }
                this.GetComponent<Renderer>().materials = mArray;
                hasMat = false;
            }
        }
    }
}