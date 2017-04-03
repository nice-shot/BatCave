using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Infra.Gameplay.UI;

namespace BatCave {
/// <summary>
/// Shows the restart menu after death and allows trying again
/// </summary>
public class GameRestarter : MonoBehaviour {

    public Text tapToRestartText;
    public Text tapToStartText;
    private bool doneReset = false;
    private Bat bat;
    private Vector2 batOriginalPosition;

    protected void Awake() {
        Game.OnGameOverEvent += OnGameOver;
        bat = Game.instance.player;
        batOriginalPosition = bat.gameObject.transform.position;
        gameObject.SetActive(false);
    }

    protected void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (!doneReset) {
                ResetParameters();
            } else {
                StartGame();
            }
        }
    }

    private void ResetParameters() {
        Debug.Log("Reset game position");
        bat.gameObject.transform.position = batOriginalPosition;
        bat.ResetBat();
        // Remove all terrain chunks
        Terrain.TerrainRasterizer.instance.terrainPool.ReturnAll();
        Game.instance.GenerateInitialTerrain();
        tapToRestartText.gameObject.SetActive(false);
        tapToStartText.gameObject.SetActive(true);
        doneReset = true;
    }

    private void StartGame() {
        Game.instance.StartGame();
        gameObject.SetActive(false);
    }

    private void OnGameOver() {
        gameObject.SetActive(true);
        doneReset = false;
        tapToRestartText.gameObject.SetActive(true);
    }
}
}

