using UnityEngine;
using UnityEngine.Rendering;
using System;
using BatCave.Spline;

namespace BatCave.Render {
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class Mesh2DSpline : MonoBehaviour {
    public int splinePoints = 40;
    public Transform[] controlPoints1;
    public Transform[] controlPoints2;

    public bool monitorControlPoints;

    [Tooltip("Checked: stretches the texture to fit the bounding box of the mesh.\n" +
        "Unchecked: matches the UVs to Unity units")]
    public bool stretchToFit = true;
    [Tooltip("If Stretch to Fit is not set, this is the scale of the texture")]
    public float textureScale = 1f;

    private Mesh mesh;
    private MeshRenderer meshRenderer;

    protected void Awake() {
        mesh = GetComponent<MeshFilter>().sharedMesh;
        meshRenderer = GetComponent<MeshRenderer>();
    }

#if UNITY_EDITOR
    /// <summary>
    /// This method is executed when this component is added to a game object.
    /// </summary>
    protected void Reset() {
        Awake();
        if (mesh == null) {
            var meshFilter = GetComponent<MeshFilter>();
            meshFilter.sharedMesh = new Mesh();
            mesh = meshFilter.sharedMesh;
        }
        if (meshRenderer.sharedMaterial == null) {
            meshRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            meshRenderer.lightProbeUsage = LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }
    }
#endif

    protected void OnEnable() {
        UpdateSpline();
        enabled = monitorControlPoints;
    }

    protected void Update() {
        UpdateSpline();
    }

    private void UpdateSpline() {
        var points = GetControlPoints(controlPoints1);
        var spline1 = new TwoDSpline(points);
        points = GetControlPoints(controlPoints2);
        var spline2 = new TwoDSpline(points);

        var vertices = new Vector2[splinePoints * 2];
        var space = 1f / (splinePoints - 1);
        var t = 0f;

        for (int i = 0; i < splinePoints; i++) {
            vertices[i] = spline1.Value(t);
            vertices[vertices.Length - 1 - i] = spline2.Value(t);
            t += space;
        }

        SetVertices(vertices);
    }

    private static Vector2[] GetControlPoints(Transform[] controlPoints) {
        var points = new Vector2[controlPoints.Length];
        for (int i = 0; i < points.Length; i++) {
            points[i] = controlPoints[i].position;
        }
        return points;
    }

    private void SetVertices(Vector2[] points) {
        var vertices = new Vector3[points.Length];
        var min = Vector2.one * float.PositiveInfinity;
        var max = -min;
        float x, y;
        for (int i = 0; i < vertices.Length; i++) {
            x = points[i].x;
            y = points[i].y;
            vertices[i] = new Vector3(x, y);
            if (stretchToFit) {
                min.x = Mathf.Min(x, min.x);
                min.y = Mathf.Min(y, min.y);
                max.x = Mathf.Max(x, max.x);
                max.y = Mathf.Max(y, max.y);
            }
        }

        var uv = new Vector2[vertices.Length];
        for (int i = 0; i < uv.Length; i++) {
            if (stretchToFit) {
                x = vertices[i].x;
                y = vertices[i].y;
                uv[i] = new Vector2(
                    Mathf.InverseLerp(min.x, max.x, x),
                    Mathf.InverseLerp(min.y, max.y, y));
            } else {
                uv[i] = vertices[i] / textureScale;
            }
        }

        if (vertices.Length < mesh.vertices.Length) {
            mesh.Clear();
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = Triangulator.Triangulate(points);

        mesh.RecalculateBounds();
    }
}
}
