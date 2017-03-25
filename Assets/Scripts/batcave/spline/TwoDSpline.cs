using UnityEngine;
using System;

namespace BatCave.Spline {
/// <summary>
/// A spline that is set in a 2D plane - it can have multiple X values.
/// It is implemented using 2 splines - one for the X coordinates and one for
/// the Y coordinates. We use the order of the control points as the "X"
/// coordinate of the control points used for each of the splines - the X spline
/// coordinates are (i / n, X_i) and the Y spline coordinates are (i / n, Y_i).
/// This yields for each t in the range [0, 1], the point (x, y) on the 2D spline.
/// </summary>
public class TwoDSpline {
    private SingleSpline splineX;
    private SingleSpline splineY;

    public TwoDSpline(Vector2[] controlPoints) {
        var pointsX = new Vector2[controlPoints.Length];
        var pointsY = new Vector2[controlPoints.Length];

        for (int i = 0; i < controlPoints.Length; i++) {
            pointsX[i] = new Vector2(i / (controlPoints.Length - 1f), controlPoints[i].x);
            pointsY[i] = new Vector2(i / (controlPoints.Length - 1f), controlPoints[i].y);
        }

        splineX = new SingleSpline(pointsX);
        splineY = new SingleSpline(pointsY);
    }

    /// <summary>
    /// T should be in the range [0, 1].
    /// </summary>
    public Vector2 Value(float t) {
        var x = splineX.Value(t);
        var y = splineY.Value(t);
        return new Vector2(x, y);
    }
}
}
