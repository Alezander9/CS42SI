// Interface for camera controllers. Implement this to create new camera behaviors.

using UnityEngine;

public interface ICameraController
{
    void OnActivate();
    void OnDeactivate();
    Vector3 UpdatePosition(float deltaTime);
}

