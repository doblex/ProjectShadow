using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FieldOfViewMesh : MonoBehaviour
{
    public AIController owner;
    public int rayCount = 50;
    [Range(0.1f, 1f)]
    public float radiusMultiplier = 1f;

    Mesh mesh;
    MeshRenderer rend;
    public bool isPlayerInsideFOV;

    void Awake()
    {
        // DEBUG per capire quale oggetto è
        Debug.Log($"{gameObject.name}: Awake FieldOfViewMesh");

        // Mesh
        mesh = new Mesh();
        mesh.name = "FOV Mesh";
        GetComponent<MeshFilter>().mesh = mesh;

        // Renderer
        rend = GetComponent<MeshRenderer>();
        if (rend == null)
            Debug.LogError($"{gameObject.name}: MeshRenderer mancante!");

        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rend.receiveShadows = false;
        rend.enabled = false;
    }
    void LateUpdate()
    {
        if (owner == null || rend == null || !rend.enabled) return;


        float fov = owner.viewAngle;
        float viewDistance = owner.viewRadius * radiusMultiplier;

        int vertexCount = rayCount + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero;
        float angle = -fov * 0.5f;

        for (int i = 0; i <= rayCount; i++)
        {
            float angRad = angle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Sin(angRad), 0f, Mathf.Cos(angRad));
            Vector3 vertex = dir * viewDistance;

            vertices[i + 1] = vertex;

            if (i < rayCount)
            {
                int triIndex = i * 3;
                triangles[triIndex] = 0;
                triangles[triIndex + 1] = i + 1;
                triangles[triIndex + 2] = i + 2;
            }

            angle += fov / rayCount;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
    public void UpdateVisibility()
    {
        if (owner == null)
        {
            Debug.LogWarning($"{gameObject.name}: owner NULL in UpdateVisibility");
            return;
        }
        if (rend == null)
        {
            Debug.LogWarning($"{gameObject.name}: rend NULL in UpdateVisibility");
            return;
        }

        bool visible = owner.IsSelected || isPlayerInsideFOV;
        Debug.Log($"{gameObject.name}: UpdateVisibility -> selected={owner.IsSelected}, inFOV={isPlayerInsideFOV}, visible={visible}");
        rend.enabled = visible;
    }

}
