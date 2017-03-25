//#define _DEBUG_
using UnityEngine;

namespace Infra {
[RequireComponent(typeof(Camera))]
public class CameraResize : MonoBehaviour {
    public float designAspectHeight;
    public float designAspectWidth;

    [Tooltip("If 'false' will match the design width")]
    public bool stretchToFitDesignHeight = true;
    [Tooltip("Can the camera zoom to fit the designed width/height")]
    public bool allowToZoomIn = false;

    private float designHeight;
    private float designWidth;

    protected void Awake() {
        Camera cameraComponent = GetComponent<Camera>();
        // The orthographic size is half of the vertical size of the viewport.
        var designAspect = designAspectWidth / designAspectHeight;

        designHeight = cameraComponent.orthographicSize;
        designWidth = designHeight * designAspect;
#if UNITY_EDITOR && _DEBUG_
    }

    protected void Update() {
        Camera cameraComponent = GetComponent<Camera>();
#endif
        float wantedSize = 0;
        if (stretchToFitDesignHeight) {
            // Change the size of the camera so that the height will match the
            // design height.
            wantedSize = designHeight;
        } else {
            wantedSize = designWidth / cameraComponent.aspect;
        }

        if (wantedSize < designHeight && !allowToZoomIn) {
            wantedSize = designHeight;
        }
        cameraComponent.orthographicSize = wantedSize;
    }
}
}
