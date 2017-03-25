using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Infra;
using BatCave.Terrain;

namespace BatCave {
public class Game : MonoBehaviour {
    public static Game instance;
    
    public static event Action OnGameStartedEvent;
    public static event Action OnGameOverEvent;
    public static event Action<TerrainGenerator.TerrainPoint> OnPlayerPassedPointEvent;

    public Bat player;
    public Text tapToStartText;

    public int pointsToKeepBehindPlayer;
    [Tooltip("The x value that the initial generated terrain must pass")]
    public float minXForGameStart;

    public List<TerrainGenerator.TerrainPoint> terrainPoints =
        new List<TerrainGenerator.TerrainPoint>();

    /// <summary>
    /// The index of the next point the player needs to pass.
    /// </summary>
    private int nextPointIndex;
    /// <summary>
    /// The index of the next point to rasterize.
    /// </summary>
    private int nextPointToRasterize;

    public bool HasStarted {
        get {
            return gameObject.activeInHierarchy;
        }
    }

    public int Score {
        get {
            return _score;
        }
    }
    private int _score;
    /// <summary>
    /// The X coordinate where the score is zero.
    /// </summary>
    private float zeroScoreX;

    protected void Awake() {
        instance = this;

        Bat.OnDied += OnGameOver;
        TerrainAliveChecker.OnTerrainEnteredScreen += OnTerrainEnteredScreen;
        gameObject.SetActive(false);

        // Generate initial terrain.
        TerrainGenerator.TerrainPoint point;
        do {
            point = TerrainGenerator.GetNextPoint(0);
            terrainPoints.Add(point);
        } while (point.x < minXForGameStart);
        // One more point after that to allow accounting for the next point's slope.
        point = TerrainGenerator.GetNextPoint(0);
        terrainPoints.Add(point);

        DebugUtils.Log("Created " + terrainPoints.Count + " initial points");

        nextPointIndex = 0;
        _score = 0;

        // Rasterize the terrain.
        nextPointToRasterize = 0;
        int lastPointIndex = -1;
        while (nextPointToRasterize > lastPointIndex) {
            lastPointIndex = nextPointToRasterize;
            DebugUtils.Log("Rasterizing point #" + lastPointIndex);
            nextPointToRasterize = TerrainRasterizer.RasterizeNextChunk(lastPointIndex);
        }
    }

    protected void OnDestroy() {
        Bat.OnDied -= OnGameOver;
        TerrainAliveChecker.OnTerrainEnteredScreen -= OnTerrainEnteredScreen;
    }

    protected void FixedUpdate() {
        // Check if the player passed the next point.
        var point = GetPassedPoint();
        if (point != null && OnPlayerPassedPointEvent != null) {
            OnPlayerPassedPointEvent(point);
        }

        // Update score.
        _score = Mathf.FloorToInt(player.transform.position.x - zeroScoreX);
    }

    public void StartGame() {
        gameObject.SetActive(true);

        tapToStartText.gameObject.SetActive(false);
        zeroScoreX = player.transform.position.x;
        _score = 0;

        // Clear up passed points.
        while (GetPassedPoint() != null) {
            // Empty on purpose.
        }

        if (OnGameStartedEvent != null) {
            OnGameStartedEvent();
        }
    }

    private TerrainGenerator.TerrainPoint GetPassedPoint() {
        DebugUtils.Assert(nextPointIndex < terrainPoints.Count, "Did not create enough terrain points! Player passed them all");
        var nextPoint = terrainPoints[nextPointIndex];
        if (player.transform.position.x >= nextPoint.x) {
            DebugUtils.Log("Player passed point " + nextPoint.x);
            if (nextPointIndex < pointsToKeepBehindPlayer) {
                ++nextPointIndex;
            } else {
                terrainPoints.RemoveAt(0);
                --nextPointToRasterize;
            }
            return nextPoint;
        }
        return null;
    }

    private static void OnGameOver() {
        if (OnGameOverEvent != null) {
            OnGameOverEvent();
        }
    }

    private void OnTerrainEnteredScreen() {
        DebugUtils.Log("Terrain entered screen");
        // Add points until the terrain is rasterized.
        int lastPointIndex = nextPointToRasterize;
        while (nextPointToRasterize == lastPointIndex) {
            var difficulty = DifficultyManager.GetNextDifficulty();
            var point = TerrainGenerator.GetNextPoint(difficulty);
            terrainPoints.Add(point);
            nextPointToRasterize = TerrainRasterizer.RasterizeNextChunk(lastPointIndex);
        }
    }
}
}
