using UnityEngine;

namespace BatCave.Spline {
public class ContinuousSpline : SingleSpline {
    public ContinuousSpline(Vector2[] firstControlPoints) : base(firstControlPoints) {
        // Empty on purpose.
    }

    /// <summary>
    /// Continue the spline with more control points.
    /// </summary>
    public void AddControlPoints(params Vector2[] points) {
        // TODO: Implement: don't keep all of the control points, there could be
        //       millions of them!
    }
}
}
