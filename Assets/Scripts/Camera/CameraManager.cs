// Manages the main camera and switches between different camera controllers.

using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private Camera _camera;
    private ICameraController _activeController;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    public void SetController(ICameraController controller)
    {
        _activeController?.OnDeactivate();
        _activeController = controller;
        _activeController?.OnActivate();
    }

    private void LateUpdate()
    {
        if (_activeController != null)
        {
            Vector3 newPosition = _activeController.UpdatePosition(Time.deltaTime);
            newPosition.z = _camera.transform.position.z; // Preserve Z for 2D
            _camera.transform.position = newPosition;
        }
    }
}

