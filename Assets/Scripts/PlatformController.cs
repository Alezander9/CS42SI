// Controls moving platform behavior with simple back-and-forth movement between two points.
// Provides velocity information to PlayerMovement for proper player-platform interaction.

using UnityEngine;

public class PlatformController : MonoBehaviour
{
    [SerializeField] private float _xSpeed;
    [SerializeField] private float _ySpeed;
    [SerializeField] private Vector2 _pointA;
    [SerializeField] private Vector2 _pointB;

    private Vector2 _rawMovement;
    private float _horizontalSpeed;
    private float _verticalSpeed;
    private Vector2 _currentWaypoint;

    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }

    private void Start()
    {
        _transform.position = _pointA;

        _horizontalSpeed = _xSpeed;
        _verticalSpeed = _ySpeed;
        _currentWaypoint = _pointB;
    }

    private void Update()
    {
        MovePlatform();
        Move();
    }

    private void MovePlatform()
    {
        var distance = Vector2.Distance(_transform.position, _currentWaypoint);

        if(distance < 0.1f)
        {
            if (_currentWaypoint == _pointB)
                _currentWaypoint = _pointA;
            else
                _currentWaypoint = _pointB;

            _horizontalSpeed = -_horizontalSpeed;
            _verticalSpeed = -_verticalSpeed;
        }
    }

    public Vector2 GetRawVelocity()
    {
        return new Vector2(_horizontalSpeed, _verticalSpeed);
    }

    private void Move()
    {
        _rawMovement = new Vector2(_horizontalSpeed, _verticalSpeed);
        var move = _rawMovement * Time.deltaTime;
        _transform.position += (Vector3)move;
    }
}

