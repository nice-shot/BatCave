using UnityEngine;
using Infra.Utils;

namespace Infra.Gameplay {
public class EndlessWorldScroller : MonoBehaviour {
    [Tooltip("Scrolling will be performed in attempt to keep the target in the center of the world")]
    public Transform target;
    [Tooltip("It is recommended to use 3 chunks")]
    public Transform[] chunks;

    [Tooltip("The index of the center chunk")]
    public int index;

    [Tooltip("Set to 0 to calculate the size using the distance between the chunks")]
    public float chunkSize;
    [Tooltip("The offset from the target to move the back chunk to the front or vice versa. Set to 0 to calculate the trigger using the chunk size")]
    public float offsetTrigger;

    private int indexFromCenterToFront;
    private int indexFromCenterToBack;
    private float backToFrontDistance;

    protected void Awake() {
        indexFromCenterToFront = chunks.Length - 1 - index;
        indexFromCenterToBack = index;
        if (Mathf.Approximately(chunkSize, 0)) {
            chunkSize = Mathf.Abs(chunks[1].position.x - chunks[0].position.x);
        }
        if (Mathf.Approximately(offsetTrigger, 0)) {
            offsetTrigger = chunkSize / 2;
        }
        backToFrontDistance = chunkSize * chunks.Length;
    }

    public void UpdateNow() {
        while (MoveIfNeeded());
    }

    protected void Update() {
        MoveIfNeeded();
    }

    private bool MoveIfNeeded() {
        var chunk = chunks[index];
        var offset = target.position.x - chunk.position.x;
        if (offset > offsetTrigger) {
            // Target is ahead move back chunk forward.
            var backChunk = chunks[MathsUtils.Mod(index - indexFromCenterToBack, chunks.Length)];
            backChunk.Translate(backToFrontDistance, 0, 0);
            index = (index + 1) % chunks.Length;
            return true;
        } else if (offset < -offsetTrigger) {
            // Target is behind move front chunk backward.
            var frontChunk = chunks[(index + indexFromCenterToFront) % chunks.Length];
            frontChunk.Translate(-backToFrontDistance, 0, 0);
            index = MathsUtils.Mod(index - 1, chunks.Length);
            return true;
        }
        return false;
    }
}
}
