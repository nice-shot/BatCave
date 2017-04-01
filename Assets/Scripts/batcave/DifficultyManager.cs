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

    public DifficultyLevel[] difficultyCurve = {
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

    private int difficultyCurveIndex = 0;
    private TerrainPattern nextPattern;
    private Dictionary<DifficultyLevel, int[]> diffToPattern;

    public override void Init() {
        Game.OnPlayerPassedPointEvent += OnPlayerPassedPoint;
        TerrainGenerator.OnTerrainPatternFinishedEvent += OnPatternFinished;

        var numPatterns = TerrainGenerator.instance.terrainPatterns.Length;
        // Correspond difficulty level to number of patterns
        // So if we have 7 patterns:
        // * 0-3 will be easy
        // * 3-5 will be medium
        // * 5-7 will be hard
        diffToPattern[DifficultyLevel.easy] = new int[2] { 0, numPatterns / 3 + 1 };
        diffToPattern[DifficultyLevel.hard] = new int[2]
        {
            numPatterns - numPatterns / 3,
            numPatterns
        };
        diffToPattern[DifficultyLevel.medium] = new int[2]
        {
            numPatterns / 3 + 1,
            numPatterns - numPatterns / 3
        };
        Debug.Log("There are " + TerrainGenerator.instance.terrainPatterns.Length + " patterns");
    }

    public static int GetNextDifficulty() {
        // Always return 0 until the game starts.
        if (!Game.instance.HasStarted) return 0;

        // EXERCISE: Return difficulty level based on difficulty curve plan.
        return Random.Range(0, TerrainGenerator.instance.patternNameRanking.Length);
    }

    private void OnPatternFinished(TerrainPattern pattern) {
        Debug.Log("Finished pattern - " + pattern.name);
    }

    private void OnPlayerPassedPoint(TerrainGenerator.TerrainPoint point) {
        // EXERCISE: Process player success level.
    }
}
}
