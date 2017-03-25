using UnityEngine;
using System;

namespace BatCave.Terrain {
/// <summary>
/// Place on a trigger to be notified when terrain that is moving from right to
/// left fully entered the screen.
/// Also returns terrain to the pool when it leaves the trigger.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class TerrainAliveChecker : MonoBehaviour {
    public static event Action OnTerrainEnteredScreen;

    /// <summary>
    /// A piece of terrain that the checker waits for it to be inside.
    /// </summary>
    private TerrainPiece polledPiece;
    private BoxCollider2D trigger;

    protected void Awake() {
        trigger = GetComponent<BoxCollider2D>();
    }

    protected void OnTriggerEnter2D(Collider2D collider) {
        if (!collider.CompareTag("Terrain")) return;

        var terrain = collider.GetComponent<TerrainPiece>();
        if (terrain == null) return;

        // Terrain was created inside the trigger. Don't notify about it since
        // it is probably the initial terrain generation when the game starts.
        if (IsFullyInside(terrain)) return;

        // Terrain was not created inside - Wait for it to fully enter.
        if (polledPiece == null || polledPiece.RightEdge < terrain.RightEdge) {
            polledPiece = terrain;
        }
    }

    protected void OnTriggerExit2D(Collider2D collider) {
        if (!collider.CompareTag("Terrain")) return;

        var terrainChunk = collider.GetComponentInParent<TerrainChunk>();
        if (terrainChunk == null) return;

        // Terrain has left the camera area. Disable it to return it to the pool.
        terrainChunk.ReturnSelf();
    }

    protected void FixedUpdate() {
        if (polledPiece != null && IsFullyInside(polledPiece)) {
            polledPiece = null;
            // The piece is inside. Notify listeners and accept the next piece.
            if (OnTerrainEnteredScreen != null) {
                OnTerrainEnteredScreen();
            }
        }
    }

    /// <summary>
    /// Assuming terrain is moving from right to left, a terrain piece is fully
    /// inside the trigger if its right edge is before the trigger's right edge.
    /// </summary>
    private bool IsFullyInside(TerrainPiece terrain) {
        var triggerRightEdge = trigger.bounds.max.x;
        return terrain.RightEdge <= triggerRightEdge;
    }
}
}
