using UnityEngine;
using System;
using BatCave.Spline;

namespace BatCave.Render {
[RequireComponent(typeof(LineRenderer))]
public class LineRendererSpline : MonoBehaviour {
    private const int EVEN_SEGMENTS_SAMPLE_RESOLUTION = 4;

    public int linePoints = 40;
    public Transform[] controlPoints;

    [Tooltip("If set, tries to even the distances between the points. Useful if the texture of the line is detailed.")]
    public bool maintainEvenSegments;
    public bool monitorControlPoints;

    private LineRenderer line;

    protected void Awake() {
        line = GetComponent<LineRenderer>();
    }

    protected void OnEnable() {
        UpdateSpline();
        enabled = monitorControlPoints;
    }

    protected void Update() {
        UpdateSpline();
    }

    private void UpdateSpline() {
        var points = GetControlPoints();
        var spline = new SingleSpline(points);
        // Set starting X.
        var x = points[0].x;
        // Calculate the X distance of the spline.
        var width = points[points.Length - 1].x - x;
        // Prepare enough vertices in the line renderer.
        line.numPositions = linePoints;

        if (maintainEvenSegments) {
            // This is a bit more complicated that it has to be because we try
            // to create points with equal distances.
            // Calculate the X distance between 2 sample points.
            var space = width / (linePoints * EVEN_SEGMENTS_SAMPLE_RESOLUTION - 1);
            // Measure estimated spline length using sample points.
            var length = 0f;
            var samplePoints = new Vector2[linePoints * EVEN_SEGMENTS_SAMPLE_RESOLUTION];
            samplePoints[0] = points[0];
            for (int i = 1; i < samplePoints.Length; i++) {
                x += space;
                samplePoints[i] = new Vector2(x, spline.Value(x));
                length += (samplePoints[i] - samplePoints[i - 1]).magnitude;
            }
            // Calculate the length of each segment.
            length /= (linePoints - 1);
            // Trace the sample points to find the X of each desired line point.
            x = points[0].x;
            line.SetPosition(0, (Vector3)points[0]);
            var sizeLeft = length;
            var sampleIndex = 1;
            var segmentLength = 1f;
            for (int i = 1; i < linePoints; i++) {
                while (sampleIndex < samplePoints.Length
                    && (segmentLength =
                        (samplePoints[sampleIndex] - samplePoints[sampleIndex - 1]).magnitude) < sizeLeft) {
                    sizeLeft -= segmentLength;
                    ++sampleIndex;
                }
                if (sampleIndex == samplePoints.Length) {
                    x = samplePoints[sampleIndex - 1].x;
                } else {
                    // Find the X of the point that is sizeLeft away from the
                    // first sample point.
                    x = Mathf.LerpUnclamped(
                        samplePoints[sampleIndex - 1].x,
                        samplePoints[sampleIndex].x,
                        sizeLeft / segmentLength);
                }
                line.SetPosition(i, new Vector3(x, spline.Value(x)));
                sizeLeft += length;
            }
        } else {
            // The simple implementation.
            var space = width / (linePoints - 1);
            for (int i = 0; i < linePoints; i++) {
                line.SetPosition(i, new Vector3(x, spline.Value(x)));
                x += space;
            }
        }
    }

    private Vector2[] GetControlPoints() {
        var points = new Vector2[controlPoints.Length];
        for (int i = 0; i < points.Length; i++) {
            points[i] = controlPoints[i].position;
        }
        return points;
    }
}
}
