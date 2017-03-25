using UnityEngine;
using Infra.Collections;

namespace BatCave.Terrain {
/// <summary>
/// Contains a ceiling and a floor of the cave.
/// When borrowed from a pool, sets the ceiling and floor to the given bounding
/// points.
/// </summary>
public class TerrainChunk : MonoBehaviour, IPoolable {
    public TerrainPiece ceiling;
    public TerrainPiece floor;

    /// <summary>
    /// Expected parameters: Vector2 ceilingPoints, Vector2 floorPoints.
    /// </summary>
    public int Activate(params object[] activateParams) {
        int index = 0;
        var ceilingPoints = (Vector2[])activateParams[index++];
        var floorPoints = (Vector2[])activateParams[index++];

        ceiling.SetPoints(ceilingPoints);
        floor.SetPoints(floorPoints);

        return index;
    }

    public void ReturnSelf() {
        gameObject.SetActive(false);
    }
}
}
