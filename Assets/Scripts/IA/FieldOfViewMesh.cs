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

    void Awake()
    {
        mesh = new Mesh();
        mesh.name = "FOV Mesh";
        GetComponent<MeshFilter>().mesh = mesh;

        rend = GetComponent<MeshRenderer>();
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rend.receiveShadows = false;
    }

    void LateUpdate()
    {
        if (owner == null) return;

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
}
