using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Infra.Gameplay.UI;

namespace BatCave {
/// <summary>
/// Shows the restart menu after death and allows trying again
/// </summary>
public class GameRestarter : MonoBehaviour {

    public Text tapToRestartText;

    protected void Awake() {
        Game.OnGameOverEvent += OnGameOver;
    }

    private void OnGameOver() {
        tapToRestartText.gameObject.SetActive(true);
        Debug.Log("cool");
    }
}
}

