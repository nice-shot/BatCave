using UnityEngine;
using UnityEngine.EventSystems;
using Infra.Gameplay.UI;

namespace BatCave {
/// <summary>
/// Starts the game when clicking the screen or when pressing space.
/// </summary>
public class GameStarter : MonoBehaviour {
    protected void Awake() {
        GameInputCapture.OnTouchDown += OnTouchDown;
    }

    protected void Update() {
        if (Input.GetKeyUp(KeyCode.Space) && !Game.instance.HasStarted) {
            StartGame();
        }
    }

    private void OnTouchDown(PointerEventData e) {
        StartGame();
    }

    private void StartGame() {
        GameInputCapture.OnTouchDown -= OnTouchDown;

        Game.instance.StartGame();
        gameObject.SetActive(false);
    }
}
}
