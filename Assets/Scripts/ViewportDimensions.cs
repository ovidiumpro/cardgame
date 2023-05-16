using UnityEngine;

public class ViewportDimensions : MonoBehaviour
{ public Camera mainCamera;
    public float distanceFromCamera = 5.0f;
    public float canvasScale = 0.005f;

    void Start()
    {
        // Calculate the viewport's height and width in world units at the given distance from the camera
        float viewportHeight = 2.0f * distanceFromCamera * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float viewportWidth = viewportHeight * mainCamera.aspect;

        // Calculate the required width and height for the canvas in world units
        float canvasWidth = viewportWidth / canvasScale;
        float canvasHeight = viewportHeight / canvasScale;

        // Set the canvas width and height
       Debug.Log($"width: {canvasWidth}, height: {canvasHeight}");
    }
    
}