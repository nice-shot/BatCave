using UnityEngine;
using Infra.Collections;
using System.Collections.Generic;

namespace BatCave.Terrain {
    
// Holds a group of points to create a specific pattern
[System.Serializable]
public class TerrainPattern {
    public string name;
    public TerrainGenerator.TerrainPoint[] points;
    public int difficultyScore;
    private int currentIndex = 0;

    public void ResetPoints() {
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
        public string patternName;
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

    private class PatternNameComparer : System.Collections.IComparer {
        public int Compare(System.Object a_name,  System.Object b_name) {
            var a_pattern = TerrainGenerator.instance.GetPatternByName((string)a_name);
            var b_pattern = TerrainGenerator.instance.GetPatternByName((string)b_name);
            return a_pattern.difficultyScore.CompareTo(b_pattern.difficultyScore);
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

    [Tooltip("Create the patterns here")]
    public TerrainPattern[] terrainPatterns;
    [Tooltip("Write the name of the starting pattern")]
    public string initialPatternName;
    [Tooltip("The pattern ranking - changes dynamically")]
    public string[] patternNameRanking;

    public TerrainPattern currentPattern;
    private Dictionary<string, TerrainPattern> patternNames = new Dictionary<string, TerrainPattern>();
    private Dictionary<int, TerrainPattern> patternRanking = new Dictionary<int, TerrainPattern>();
    private Dictionary<string, int> patternNameToRank = new Dictionary<string, int>();

    public TerrainPattern GetPatternByName(string name) {
        return patternNames[name];
    }

    public TerrainPattern GetPatternByRank(int rank) {
        if (rank == 0) {
            return GetPatternByName(initialPatternName);
        }
        return GetPatternByName(patternNameRanking[rank - 1]);
    }

    private readonly ObjectPool<TerrainPoint> terrainPoints = new ObjectPool<TerrainPoint>(5, 5);
       

    // Sets the current pattern to the first which should be the wide tunnel
    protected void Awake() {
        // Initialize the patternNames dict to be used later
        for (int i = 0; i < terrainPatterns.Length; i++) {
            var pattern = terrainPatterns[i];
            patternNames[pattern.name] = pattern;
        }

        patternNameRanking = new string[terrainPatterns.Length];
        for (int i = 0; i < terrainPatterns.Length; i++) {
            patternNameRanking[i] = terrainPatterns[i].name;
        }
        SetPatternRanks();

//        for (int i = 0; i < patternNameRanking.Length; i++) {
//            var patternName = patternNameRanking[i];
//            patternRanking[i] = patternNames[patternName];
//            patternNameToRank[patternName] = i;
//        }

        currentPattern = GetPatternByName(initialPatternName);
    }

    private void SetPatternRanks() {
        System.Collections.IComparer comparer = new PatternNameComparer();
        System.Array.Sort(patternNameRanking, comparer);
    }

    /// <summary>
    /// Returns the next point according to the difficulty level.
    /// </summary>
    /// <param name="difficulty">The difficulty level. Min level is 0. There is
    /// no limit on the max level.</param>
    public static TerrainPoint GetNextPoint(int difficulty) {
        return instance._GetNextPoint(difficulty);
    }

    private TerrainPoint _GetNextPoint(int difficulty) {
        var point = terrainPoints.Borrow();
        point.difficulty = difficulty;

        // This means we've got a restart command 
        if (difficulty == 0 && currentPattern.name != initialPatternName) {
            currentPattern.ResetPoints();
            currentPattern = GetPatternByName(initialPatternName);
        }


        if (!currentPattern.HasMorePoints()) {
            Debug.Log("Reached end of pattern - " + currentPattern.name);

            if (OnTerrainPatternFinishedEvent != null) {
                OnTerrainPatternFinishedEvent(currentPattern);
            }

            currentPattern = GetPatternByRank(difficulty);
            Debug.Log("Starting new pattern - " + currentPattern.name);
        }

        var basePoint = currentPattern.GetNextPoint();
        point.distanceFromPrevious = basePoint.distanceFromPrevious;
        point.ceilingY = basePoint.ceilingY;
        point.floorY = basePoint.floorY;
        point.patternName = currentPattern.name;
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

    /// <summary>
    /// Moves the given pattern index higher in the difficulty chain
    /// </summary>
    public void ChangePatternDifficulty(string patternName) {
//        int patternIndex = patternNameToRank[patternName];
//
//        // Can't level up the last pattern
//        // Won't level up pattern 0 since it's supposed to be the wide tunnel in the start of the game
//        if (patternIndex >= terrainPatterns.Length - 1 || patternIndex <= 0) {
//            return;
//        }
//        var patternToLevelUp = patternRanking[patternIndex];
//        var patternToLevelDown = patternRanking[patternIndex + 1];
//        Debug.Log("Leveling up pattern: " + patternToLevelUp.name);
//        patternNameToRank[patternToLevelUp.name]++;
//        patternRanking[patternIndex + 1] = patternToLevelUp;
//        // Used for the inspector
//        patternNameRanking[patternIndex + 1] = patternToLevelUp.name;
//        Debug.Log("Leveling down pattern: " + patternToLevelDown.name);
//        patternNameToRank[patternToLevelDown.name]--;
//        patternRanking[patternIndex] = patternToLevelDown;
//        patternNameRanking[patternIndex] = patternToLevelDown.name;
//       
    }
}
}
