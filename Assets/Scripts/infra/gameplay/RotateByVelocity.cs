using UnityEngine;
using Infra.Utils;

namespace Infra.Gameplay {
public class RotateByVelocity : MonoBehaviour {
	public Rigidbody2D rigidbodyToTrack;

    [Range(0,1)]
    public float lerpAmount;
    public float minAngle;
    public float maxAngle;
    public float offsetAngle;
    [Tooltip("The abslute X velocity that below that the angle will be set to 0")]
    public float minXVelocityToZeroAngle;
    [Tooltip("This is for debugging")]
    public float currentAngle;

    protected void FixedUpdate() {
        Vector2 velocity = rigidbodyToTrack.velocity;

        if (velocity.x < 0) {
            velocity = -velocity;
        }

        if (-minXVelocityToZeroAngle < velocity.x && velocity.x < minXVelocityToZeroAngle) {
            currentAngle = 0;
        } else {
            currentAngle = velocity.GetAngle();
        }
        currentAngle += offsetAngle;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        Quaternion currentRotation = transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, currentAngle));
        transform.localRotation = Quaternion.Lerp(currentRotation, targetRotation, lerpAmount);
    }
}
}
