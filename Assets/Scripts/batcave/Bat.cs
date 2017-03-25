using UnityEngine;
using UnityEngine.EventSystems;
using System;
using Infra;
using Infra.Gameplay;
using Infra.Gameplay.UI;

namespace BatCave {
/// <summary>
/// The Bat controller. Responsible for playing bat animations, handling collision
/// with the cave walls and responding to player input.
/// </summary>
[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(RotateByVelocity))]
public class Bat : MonoBehaviour {
    public float xSpeed = 0.1f;
    public float flyYSpeed = 1f;

    // This variable is only compiled when running the game in the editor.
    // When the game is built for PC or mobile for example, invulnerability will
    // be disabled automatically because its code is removed by the preprocessor.
#if UNITY_EDITOR
    [Tooltip("Check this to test terrain generation")]
    public bool isInvulnerable;
#endif

    public static event Action OnDied;

    public bool FlyUp {
        get {
            return _flyUp;
        }
        set {
            _flyUp = value;
            animatorComponent.SetBool(animFlyUpBoolId, value);
        }
    }
    private bool _flyUp;

    private int animFlyUpBoolId = Animator.StringToHash("FlyUp");
    private int animAliveBoolId = Animator.StringToHash("Alive");

    private float gravityScale;
    private Animator animatorComponent;
    private Rigidbody2D body;
    private RotateByVelocity rotator;

    protected void Awake() {
        Game.OnGameStartedEvent += OnGameStarted;
        Game.OnGameOverEvent += OnGameOver;

        animatorComponent = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        rotator = GetComponent<RotateByVelocity>();

        gravityScale = body.gravityScale;

        ResetBat();
    }

    public void ResetBat() {
        // NOTE: "Reset" is a Unity message like Awake and Update. It is called
        // when a component is added to a game object (also when editing inside
        // the editor).
        body.gravityScale = 0;
        body.velocity = new Vector2(xSpeed, 0f);
        rotator.enabled = true;
        FlyUp = true;
        enabled = false;
        animatorComponent.SetBool(animAliveBoolId, true);
    }

    protected void OnDestroy() {
        // Unregister from all events.
        Game.OnGameStartedEvent -= OnGameStarted;
        Game.OnGameOverEvent -= OnGameOver;
        GameInputCapture.OnTouchDown -= OnTouchDown;
        GameInputCapture.OnTouchUp -= OnTouchUp;
    }

    protected void Update() {
        // Handle keyboard input.
        if (Input.GetKeyDown(KeyCode.Space)) {
            FlyUp = true;
        } else if (Input.GetKeyUp(KeyCode.Space)) {
            FlyUp = false;
        }
    }

    protected void FixedUpdate() {
        // Maintain X speed and set Y speed if flying up.
        var velocity = body.velocity;
        velocity.x = xSpeed;
        if (FlyUp) {
            velocity.y = flyYSpeed;
        }
        body.velocity = velocity;
    }

    protected void OnCollisionEnter2D(Collision2D collision) {
#if UNITY_EDITOR
        if (isInvulnerable) return;
#endif
        if (!collision.gameObject.CompareTag("Terrain")) return;

        DebugUtils.Log("Collided with terrain!");

        OnGameOver();

        // OnDied eventually invokes OnGameOver too, but we called OnGameOver
        // explicitly because in the future, OnGameOver might also be used for
        // quiting a game session to a main menu or something like that.
        if (OnDied != null) {
            OnDied();
        }
    }

    private void OnGameStarted() {
        GameInputCapture.OnTouchDown += OnTouchDown;
        GameInputCapture.OnTouchUp += OnTouchUp;
        body.gravityScale = gravityScale;
        enabled = true;
    }

    private void OnGameOver() {
        if (!enabled) {
            // The bat is already dead.
            return;
        }
        GameInputCapture.OnTouchDown -= OnTouchDown;
        GameInputCapture.OnTouchUp -= OnTouchUp;
        animatorComponent.SetBool(animAliveBoolId, false);
        body.velocity = Vector2.zero;
        rotator.enabled = false;
        enabled = false;
    }

    private void OnTouchDown(PointerEventData e) {
        FlyUp = true;
    }

    private void OnTouchUp(PointerEventData e) {
        FlyUp = false;
    }
}
}
