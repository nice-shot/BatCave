//HELLO! :)

using UnityEngine;

namespace BatCave.Spline {
public class SingleSpline {
    
    private Vector2[] controlPoints;

    public SingleSpline(Vector2[] controlPoints) {
        this.controlPoints = controlPoints;
        // TODO: Implement: initialize the spline to match the given control points.
    }

    /// <summary>
    /// Returns the value of the spline at point X.
    /// </summary>
    public float Value(float X) {
        // TODO: Implement: find the polynom f_i that passes at X and calculate
        //       f_i(X).

        return 0f;
    }

    private float GetA(Vector2 point1, Vector2 point2, float derivative) {
        return derivative * (point2.x - point1.x) - (point2.y - point1.y);
    }

    private float GetB(Vector2 point1, Vector2 point2, float derivative) {
        return (-derivative) * (point2.x - point1.x) + (point2.y - point1.y);
    }

    private float GetT(Vector2 point1, Vector2 point2, float X) {
        return (X - point1.x) / (point2.x - point1.x);
    }
}
}
