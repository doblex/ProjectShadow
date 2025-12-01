using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


#if UNITY_EDITOR
[CustomEditor(typeof(ColliderCombiner))]
public class ColliderCombinerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ColliderCombiner combiner = (ColliderCombiner)target;

        if (GUILayout.Button("Combine Colliders"))
        {
            combiner.Build();
        }
    }
}
#endif

public class ColliderCombiner : MonoBehaviour
{
    public float cellSize = 1f;
    public bool saveMeshAsset = true;

    public void Build()
    {
        Dictionary<FaceKey, FaceData> faces = new Dictionary<FaceKey, FaceData>();

        // PRENDI TUTTI I FIGLI (ricorsivo)
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();

        foreach (var mf in filters)
        {
            // Salta il MeshFilter del GameObject radice
            if (mf.gameObject == this.gameObject)
                continue;

            Mesh m = mf.sharedMesh;
            if (!m) continue;

            Matrix4x4 localToWorld = mf.transform.localToWorldMatrix;

            AddMeshFaces(m, localToWorld, mf.transform, faces);
        }

        Mesh finalMesh = BuildMeshFromFaces(faces);

        MeshCollider collider = GetComponent<MeshCollider>();
        if (!collider) collider = gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = finalMesh;

#if UNITY_EDITOR
        if (saveMeshAsset)
            SaveMeshAsset(finalMesh);
#endif

        Debug.Log("Collider combinato generato con " + faces.Count + " facce analizzate.");
    }

    // ---------------------------------------------
    // Step 2: Extract Faces and Auto-Detect Hidden Ones
    // ---------------------------------------------
    private void AddMeshFaces(
    Mesh mesh, Matrix4x4 localToWorld, Transform tile,
    Dictionary<FaceKey, FaceData> dict)
    {
        var verts = mesh.vertices;
        var tris = mesh.triangles;

        // GRID CELL OF THIS TILE
        Vector3Int tileCell = new Vector3Int(
            Mathf.RoundToInt(tile.position.x / cellSize),
            Mathf.RoundToInt(tile.position.y / cellSize),
            Mathf.RoundToInt(tile.position.z / cellSize)
        );

        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector3 v0 = localToWorld.MultiplyPoint3x4(verts[tris[i]]);
            Vector3 v1 = localToWorld.MultiplyPoint3x4(verts[tris[i + 1]]);
            Vector3 v2 = localToWorld.MultiplyPoint3x4(verts[tris[i + 2]]);

            // NORMAL
            Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;

            // quantized axis direction
            Vector3Int nInt = new Vector3Int(
                Mathf.RoundToInt(normal.x),
                Mathf.RoundToInt(normal.y),
                Mathf.RoundToInt(normal.z)
            );

            // THIS face key (face of this tile)
            FaceKey key = new FaceKey(tileCell, nInt);

            // OPPOSITE face key (neighbor tile)
            FaceKey oppositeKey = new FaceKey(tileCell + nInt, -nInt);

            // If the opposite face exists, remove both (internal)
            if (dict.ContainsKey(oppositeKey))
            {
                dict.Remove(oppositeKey);
                continue;
            }

            // Otherwise store face
            dict[key] = new FaceData(v0, v1, v2);
        }
    }


    // ---------------------------------------------
    // Step 3: Build Final Mesh from Visible Faces
    // ---------------------------------------------
    private Mesh BuildMeshFromFaces(Dictionary<FaceKey, FaceData> dict)
    {
        List<Vector3> finalVerts = new List<Vector3>();
        List<int> finalTris = new List<int>();

        foreach (var kv in dict)
        {
            if (kv.Value.hidden)
                continue;

            int baseIndex = finalVerts.Count;

            finalVerts.Add(kv.Value.v0);
            finalVerts.Add(kv.Value.v1);
            finalVerts.Add(kv.Value.v2);

            finalTris.Add(baseIndex);
            finalTris.Add(baseIndex + 1);
            finalTris.Add(baseIndex + 2);
        }

        Mesh m = new Mesh();
        m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Large mesh support
        m.SetVertices(finalVerts);
        m.SetTriangles(finalTris, 0);
        m.RecalculateNormals();
        m.RecalculateBounds();

        return m;
    }

#if UNITY_EDITOR
    private void SaveMeshAsset(Mesh mesh)
    {
        string assetName = gameObject.name;
        string path = "Assets/CombinedColliders/" + assetName + "_CombinedCollider.asset";
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
        Debug.Log("Saved mesh collider asset to " + path);
    }
#endif


}
