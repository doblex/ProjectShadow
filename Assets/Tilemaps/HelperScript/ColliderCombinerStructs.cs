using UnityEngine;

public struct FaceKey
{
    public Vector3Int cell;
    public Vector3Int normal;

    public FaceKey(Vector3Int cell, Vector3Int normal)
    {
        this.cell = cell;
        this.normal = normal;
    }

    public override int GetHashCode()
        => cell.GetHashCode() ^ (normal.GetHashCode() * 23);

    public override bool Equals(object obj)
    {
        if (!(obj is FaceKey)) return false;
        FaceKey other = (FaceKey)obj;
        return cell == other.cell && normal == other.normal;
    }
}

// Face data storage
struct FaceData
{
    public Vector3 v0, v1, v2;
    public bool hidden;

    public FaceData(Vector3 a, Vector3 b, Vector3 c)
    {
        v0 = a;
        v1 = b;
        v2 = c;
        hidden = false;
    }
}