using UnityEngine;
using Infra.Collections;
using System.Collections.Generic;

namespace BatCave.Terrain {
// Information to create points by
[System.Serializable]
public class TerrainPointPrepare {
    // Range for distance to previous point
    public float distanceMin;
    public float distanceMax;
    // Range for ceiling height
    public float ceilingMin;
    public float ceilingMax;
    // Range for floor height
    public float floorMin;
    public float floorMax;

    public float GetDistance() {
        return Random.Range(distanceMin, distanceMax);
    }

    public float GetCeilingY() {
        return Random.Range(ceilingMin, ceilingMax);
    }

    public float GetFloorY() {
        return Random.Range(floorMin, floorMax);
    }
}
    
// Holds a group of points to create a specific pattern
[System.Serializable]
public class TerrainPattern {
    public string name;
    public TerrainGenerator.TerrainPoint[] points;
    private int currentIndex = 0;

    private void ResetPoints() {
        currentIndex = 0;
    }

    public bool HasMorePoints() {
        return (currentIndex < points.Length);
    }

    public TerrainGenerator.TerrainPoint GetNextPoint() {
        if (!HasMorePoints()) {
            ResetPoints();
        }
        var point = points[currentIndex];
        currentIndex++;
        return point;
    }
}

public class TerrainGenerator : MonoSingleton<TerrainGenerator> {
    [System.Serializable]
    public class TerrainPoint {
        public float x;
        public float ceilingY;
        public float floorY;
        [Tooltip("Distance from previous point.\n" +
            "Determines the chunk size from the previous point to this one.")]
        public float distanceFromPrevious;
        [Tooltip("Kept here for easy future reference mostly at OnPlayerPassedPoint.\n" +
            "We might not have the points before the one passed, and this is " +
            "needed to assess the difficulty of passing it.")]
        public int difficulty;

        public float GetY(bool isCeiling) {
            return isCeiling ? ceilingY : floorY;
        }

        // Changes the points a bit to avoid repeating patterns
        public void CreateVariation(float variation = 0.05f) {
            distanceFromPrevious = Random.Range(distanceFromPrevious - variation, distanceFromPrevious + variation);
            ceilingY = Random.Range(ceilingY - variation, ceilingY + variation);
            floorY = Random.Range(floorY - variation, floorY + variation);
        }
    }

    public float minFloor = -1.6f;
    public float maxCeiling = 1.6f;
    [Tooltip("The minimal gap that could be created.\n" +
        "Make sure this is no less than the height of the bat's collider.")]
    public float minGap = 0.5f;
    [Tooltip("Minimal distance from point to point")]
    public float minDistance = 1f;
    [Tooltip("Maximal distance from point to point")]
    public float maxDistance = 5f;
    [Tooltip("The X position of the first point")]
    public float startingX;

    public static event System.Action<TerrainPattern> OnTerrainPatternFinishedEvent;

    public TerrainPattern[] terrainPatterns;
    [Tooltip("Write the pattern names in the order of their difficulty")]
    public string[] patternNameRanking;
    public TerrainPattern currentPattern;
    private Dictionary<int, TerrainPattern> patternRanking = new Dictionary<int, TerrainPattern>();
    private Dictionary<string, TerrainPattern> patternNames = new Dictionary<string, TerrainPattern>();


    private readonly ObjectPool<TerrainPoint> terrainPoints = new ObjectPool<TerrainPoint>(5, 5);
       

    // Sets the current pattern to the first which should be the wide tunnel
    protected void Awake() {
        for (int i = 0; i < terrainPatterns.Length; i++) {
            var pattern = terrainPatterns[i];
            patternNames[pattern.name] = pattern;
        }

        for (int i = 0; i < patternNameRanking.Length; i++) {
            var patternName = patternNameRanking[i];
            patternRanking[i] = patternNames[patternName];
        }

        currentPattern = patternRanking[0];
    }

    /// <summary>
    /// Returns the next point according to the difficulty level.
    /// </summary>
    /// <param name="difficulty">The difficulty level. Min level is 0. There is
    /// no limit on the max level.</param>
    public static TerrainPoint GetNextPoint(int difficulty) {
        return instance._GetNextPoint(difficulty);
    }

//    private TerrainPoint _GetNextPoint(int difficulty) {
//        var point = terrainPoints.Borrow();
//        point.difficulty = difficulty;
//
//        // EXERCISE: Difficulty should affect distance from previous point,
//        //           height differences from previous point (slope) and gap
//        //           between floor and ceiling.
//        //           Need to take into account previous terrain to make sure the
//        //           terrain is passable: don’t create tunnels that are too
//        //           narrow or slopes that are too steep for the bat to maneuver.
//
//        if (difficulty == 0) {
//            // Create a nice random tunnel that is very wide.
//            // Note that difficulty is 0 before the game starts, so this should
//            // never collide with the bat that is flying at the center of the
//            // cave.
//            point.floorY = Random.Range(minFloor, minFloor + 0.5f);
//            point.ceilingY = Random.Range(maxCeiling - 0.5f, maxCeiling);
//        } else {
//            point.floorY = Random.Range(minFloor, maxCeiling - minGap);
//            point.ceilingY = Random.Range(point.floorY + minGap, maxCeiling);
//        }
//        // Choose the distance from the previous point.
//        point.distanceFromPrevious = Random.Range(minDistance, maxDistance);
//
//        // Set the point at the correct position based on the previous point's
//        // position and the distance from it that we just set.
//        if (Game.instance.terrainPoints.Count == 0) {
//            point.x = startingX;
//        } else {
//            float previousX = Game.instance.terrainPoints[Game.instance.terrainPoints.Count - 1].x;
//            point.x = previousX + point.distanceFromPrevious;
//        }
//
//        return point;
//    }

    private TerrainPoint _GetNextPoint(int difficulty) {
        var point = terrainPoints.Borrow();
        point.difficulty = difficulty;

        // Should mark 0 not as easy but as inactive so we won't need this bad hack
        if (!Game.instance.HasStarted) {
            currentPattern = patternRanking[0];
        }


        if (!currentPattern.HasMorePoints()) {
            Debug.Log("Reached end of pattern - " + currentPattern.name);

            if (OnTerrainPatternFinishedEvent != null) {
                OnTerrainPatternFinishedEvent(currentPattern);
            }

            if (difficulty == 0) {
                currentPattern = patternRanking[0];
            } else {
                currentPattern = patternRanking[difficulty];
            }
        }

        var basePoint = currentPattern.GetNextPoint();
        point.distanceFromPrevious = basePoint.distanceFromPrevious;
        point.ceilingY = basePoint.ceilingY;
        point.floorY = basePoint.floorY;
        point.CreateVariation();

        // Set the point at the correct position based on the previous point's
        // position and the distance from it that we just set.
        if (Game.instance.terrainPoints.Count == 0) {
            point.x = startingX;
        } else {
            float previousX = Game.instance.terrainPoints[Game.instance.terrainPoints.Count - 1].x;
            point.x = previousX + point.distanceFromPrevious;
        }

        return point;
    }
}
}
