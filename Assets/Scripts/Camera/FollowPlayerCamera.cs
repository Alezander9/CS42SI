// Smoothly follows a target transform with configurable delay.

using UnityEngine;

public class FollowPlayerCamera : ICameraController
{
    private Transform _target;
    private Vector3 _velocity;
    private Vector3 _currentPosition;
    
    public float SmoothTime { get; set; } = 0.2f;
    public Vector2 Offset { get; set; } = Vector2.zero;

    public FollowPlayerCamera(Transform target, Vector3 startPosition)
    {
        _target = target;
        _currentPosition = startPosition;
    }

    public void OnActivate()
    {
        _velocity = Vector3.zero;
    }

    public void OnDeactivate()
    {
    }

    public Vector3 UpdatePosition(float deltaTime)
    {
        if (_target == null)
            return _currentPosition;

        Vector3 targetPosition = _target.position + (Vector3)Offset;
        _currentPosition = Vector3.SmoothDamp(_currentPosition, targetPosition, ref _velocity, SmoothTime);
        
        return _currentPosition;
    }
}

