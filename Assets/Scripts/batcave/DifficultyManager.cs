using UnityEngine;
using BatCave.Terrain;
using System.Collections.Generic;

namespace BatCave {
/// <summary>
/// Allows calculating difficulty based on player success level.
/// </summary>
public class DifficultyManager : MonoSingleton<DifficultyManager> {

    public enum DifficultyLevel {
        easy, medium, hard
    }

    // This is the chosen difficulty curve for this game. Start big, relax and then build up to a big climax
    public static DifficultyLevel[] difficultyCurve = {
        DifficultyLevel.easy, 
        DifficultyLevel.hard,
        DifficultyLevel.easy,
        DifficultyLevel.easy,
        DifficultyLevel.medium,
        DifficultyLevel.easy,
        DifficultyLevel.medium,
        DifficultyLevel.medium,
        DifficultyLevel.easy,
        DifficultyLevel.hard,
        DifficultyLevel.easy,
        DifficultyLevel.medium,
        DifficultyLevel.hard,
        DifficultyLevel.hard,
        DifficultyLevel.hard,
        DifficultyLevel.medium,
        DifficultyLevel.easy
    };

    private static int difficultyCurveIndex = 0;
    private static DifficultyLevel currentDifficulty;
    private TerrainPattern nextPattern;
    private static Dictionary<DifficultyLevel, int[]> diffToPattern = new Dictionary<DifficultyLevel, int[]>();

    public override void Init() {
        Game.OnPlayerPassedPointEvent += OnPlayerPassedPoint;
        Game.OnGameOverEvent += OnGameOver;
        TerrainGenerator.OnTerrainPatternFinishedEvent += OnPatternFinished;

        // Should always start at zero but we want the inspector to show current position and difficulty
        difficultyCurveIndex = 0;
        currentDifficulty = difficultyCurve[difficultyCurveIndex];

        var numPatterns = TerrainGenerator.instance.terrainPatterns.Length;
        // Correspond difficulty level to number of patterns
        // So if we have 7 patterns:
        // * 0-2 will be easy
        // * 3-5 will be medium
        // * 6-7 will be hard
        diffToPattern[DifficultyLevel.easy] = new int[2] { 0, numPatterns / 3 };
        diffToPattern[DifficultyLevel.hard] = new int[2]
        {
            numPatterns - numPatterns / 3,
            numPatterns
        };
        diffToPattern[DifficultyLevel.medium] = new int[2]
        {
            numPatterns / 3,
            numPatterns - numPatterns / 3
        };
        Debug.Log("Created diff to pattern:");
        foreach (KeyValuePair<DifficultyLevel, int[]> kvp in diffToPattern) {
            Debug.Log("Level: " + kvp.Key);
            foreach (int patternNum in kvp.Value) {
                Debug.Log("Pattern: " + patternNum);
            }
            Debug.Log("-------------------------");
        }
    }

    public static int GetNextDifficulty() {
        // Always return 0 until the game starts.
        if (!Game.instance.HasStarted) return 0;
            
        // Returns a difficulty settinng in the range of the current difficulty level
        return Random.Range(diffToPattern[currentDifficulty][0], diffToPattern[currentDifficulty][1]);
    }

    private void OnPatternFinished(TerrainPattern pattern) {
        if (!Game.instance.HasStarted) return;
        difficultyCurveIndex += 1;
        // If the player finished the curve it starts all over again
        if (difficultyCurveIndex >= difficultyCurve.Length) {
            difficultyCurveIndex = 0;
        }
        currentDifficulty = difficultyCurve[difficultyCurveIndex];
        Debug.Log("Difficulty changed to: " + currentDifficulty);
    }

    private void OnPlayerPassedPoint(TerrainGenerator.TerrainPoint point) {
        // EXERCISE: Process player success level.
    }

    private void OnGameOver() {
        // Set index to 0 to restart the curve
        difficultyCurveIndex = 0;
    }
}
}
