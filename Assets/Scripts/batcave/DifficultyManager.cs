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
    private int currentPlayerPattern = 0;
    private TerrainPattern nextPattern;
    private static Dictionary<DifficultyLevel, int[]> diffToPattern = new Dictionary<DifficultyLevel, int[]>();

    private static DifficultyLevel currentDifficulty {
        get {
            return difficultyCurve[difficultyCurveIndex];
        }
    }


    public override void Init() {
        Game.OnPlayerPassedPointEvent += OnPlayerPassedPoint;
        Game.OnGameOverEvent += OnGameOver;
        TerrainGenerator.OnTerrainPatternFinishedEvent += OnPatternFinished;

        difficultyCurveIndex = 0;

        // Correspond difficulty level to number of patterns
        // So if we have 7 patterns:
        // * 0 should be the wide pattern to be used before the game starts
        // * 1-2 will be easy
        // * 3-5 will be medium
        // * 6-7 will be hard
        var numPatterns = TerrainGenerator.instance.terrainPatterns.Length;

        diffToPattern[DifficultyLevel.easy] = new int[2] { 1, numPatterns / 3 };
        diffToPattern[DifficultyLevel.hard] = new int[2]
        {
            numPatterns - (numPatterns / 3) + 1,
            numPatterns
        };
        diffToPattern[DifficultyLevel.medium] = new int[2]
        {
            numPatterns / 3 + 1,
            numPatterns - numPatterns / 3
        };
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
        Debug.Log("Difficulty changed to: " + currentDifficulty);
    }

    private void OnPlayerPassedPoint(TerrainGenerator.TerrainPoint point) {
        currentPlayerPattern = point.difficulty;
    }

    private void OnGameOver() {
        // Set index to 0 to restart the curve
        difficultyCurveIndex = 0;
    }
}
}
