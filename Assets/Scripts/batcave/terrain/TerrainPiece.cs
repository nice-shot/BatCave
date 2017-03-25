using UnityEngine;

namespace BatCave.Terrain {
/// <summary>
/// Terrain piece.
/// Allows to set its bounding points. Updates its mesh and its collider to
/// match.
/// </summary>
[RequireComponent(typeof(PolygonCollider2D), typeof(MeshFilter))]
public class TerrainPiece : MonoBehaviour {
    private PolygonCollider2D polygonCollider;
    private MeshFilter meshFilter;

    public float RightEdge {
        get {
            return polygonCollider.bounds.max.x;
        }
    }

    protected void Awake() {
        polygonCollider = GetComponent<PolygonCollider2D>();
        meshFilter = GetComponent<MeshFilter>();
    }

    /// <summary>
    /// Sets the bounding points of the mesh and collider.
    /// </summary>
    public void SetPoints(Vector2[] points) {
        // Set polygon collider.
        polygonCollider.points = points;

        // Triangulate points for mesh.
        int[] indices = Triangulator.Triangulate(points);

        // Create 3D mesh vertices.
        var vertices = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++) {
            var point = points[i];
            vertices[i] = point;
        }

        // Reset mesh.
        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.triangles = indices;
        meshFilter.mesh.RecalculateBounds();
    }
}
}
