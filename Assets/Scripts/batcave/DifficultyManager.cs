using UnityEngine;
using BatCave.Terrain;

namespace BatCave {
/// <summary>
/// Allows calculating difficulty based on player success level.
/// </summary>
public class DifficultyManager : MonoSingleton<DifficultyManager> {

    public override void Init() {
        Game.OnPlayerPassedPointEvent += OnPlayerPassedPoint;
    }

    public static int GetNextDifficulty() {
        // Always return 0 until the game starts.
        if (!Game.instance.HasStarted) return 0;

        // EXERCISE: Return difficulty level based on difficulty curve plan.
        return Random.Range(0, 10);
    }

    private void OnPlayerPassedPoint(TerrainGenerator.TerrainPoint point) {
        // EXERCISE: Process player success level.
    }
}
}
