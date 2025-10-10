// Core movement system implementing Celeste-style mechanics including walk, jump, wall interactions, and dash.
// Features coyote time, jump buffering, wall sliding/climbing/jumping for smooth, responsive control.

using System;
using UnityEngine;

[RequireComponent(typeof(CharacterCollision))]
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController : MonoBehaviour
{
    public event Action OnDash;
    public event Action OnJump;
    public event Action OnLand;

    [Header("Walking")]
    [SerializeField] private float _maxMove = 13;
    [SerializeField] private float _acceleration = 180;
    [SerializeField] private float _deceleration = 90;

    [Header("Jumping")]
    [SerializeField] private float _maxJumpHeight = 3;
    [SerializeField] private float _minJumpHeight = 1.2f;
    [SerializeField] private float _timeToJumpApex = .3f;
    [SerializeField] private float _jumpBuffer = 0.1f;
    [SerializeField] private float _coyoteJump = 0.1f;

    [Header("Wall")]
    [SerializeField] private float _wallSlide = 6;
    [SerializeField] private float _wallClimb = 3.5f;
    [SerializeField] private float _wallStickTime = 0.3f;

    [Header("Wall Grabed")]
    [SerializeField] private float _wallGrabTime = 4;
    [SerializeField] private float _grabDistance = 0.2f;
    [SerializeField] private float _wallGrabJumpApexTime = 0.15f;
    [SerializeField] private Vector2 _topEdgeClimbJump = new Vector2(6, 10);
    [SerializeField] private Vector2 _wallJump = new Vector2(12, 20);

    [Header("Dash")]
    [SerializeField] private float _dashDistance = 3f;
    [SerializeField] private float _dashDuration = 0.1f;
    [SerializeField] private float _ySpeedAfterDash = 10;

    [Header("Falling")]
    [SerializeField] private float _minFallSpeed = 8;
    [SerializeField] private float _maxFallSpeed = 40;

    private Vector2 _velocity;
    private float _gravity;
    private float _maxJumpSpeed;
    private float _minJumpSpeed;
    private float _wallGrabJumpSpeed;
    private float _wallGrabJumpTimer;
    private Vector2 _rawMovement;
    private Vector2 _lastPosition;
    private Vector2 _furthestPoint;
    private float _horizontalSpeed;
    private float _verticalSpeed;
    private float _jumpBufferTimeLeft;
    private float _coyoteJumpTimeLeft;
    private float _wallStickTimeLeft;
    private float _wallGrabTimeLeft;
    private float _dashTimer;
    private bool _canWallJump;
    private bool _isWallJumpInProgress;
    private bool _dashJustEnded;
    private bool _canDash = false;
    private bool _canLand = false;
    
    // Input state variables (sampled in Update, used in FixedUpdate)
    private float _inputHorizontal;
    private float _inputVertical;
    private bool _inputJumpPressed;
    private bool _inputGrabPressed;
    private bool _inputJumpHeld;
    private bool _inputJumpReleased;
    private bool _inputDashPressed;
    
    // Debug logging
    private float _debugLogTimer = 0f;
    private const float DEBUG_LOG_INTERVAL = 0.5f; // Log twice per second

    private Transform _transform;
    private Rigidbody2D _rb;
    private CharacterCollision _playerCollision;
    private ICharacterInput _characterInput;

    private void Awake()
    {
        _transform = transform;
        _playerCollision = GetComponent<CharacterCollision>();
        
        // Set up Rigidbody2D as kinematic for physics interactions without affecting movement
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.useFullKinematicContacts = true;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // Lock fixed timestep for deterministic physics
        Time.fixedDeltaTime = 1f / 60f; // 60Hz physics
        Time.maximumDeltaTime = Time.fixedDeltaTime; // Prevent physics spiral of death
    }

    private void Start()
    {
        // Get input component (may be added at runtime by spawner)
        _characterInput = GetComponent<ICharacterInput>();
        
        if (_characterInput == null)
        {
            Debug.LogError("CharacterController requires a component implementing ICharacterInput (e.g., PlayerInput, RecordedInput, AIInput)");
            enabled = false;
            return;
        }
        
        _lastPosition = _transform.position;

        SetGravity();
        SetJumpSpeed();

        _characterInput.onJumpPressed += OnJumpPressed;
        _characterInput.onJumpReleased += OnJumpReleased;
        _characterInput.onDashPressed += OnDashPressed;
    }

    #region Start Functions

    private void SetGravity()
    {
        _gravity = 2 * _maxJumpHeight / Mathf.Pow(_timeToJumpApex, 2);
    }

    private void SetJumpSpeed()
    {
        _maxJumpSpeed = _gravity * _timeToJumpApex;
        _minJumpSpeed = Mathf.Sqrt(2 * _gravity * _minJumpHeight);
        _wallGrabJumpSpeed = _gravity * _wallGrabJumpApexTime;
    }

    #endregion

    private void Update()
    {
        // Poll input every frame for responsiveness
        _inputHorizontal = _characterInput.GetHorizontalInput();
        _inputVertical = _characterInput.GetVerticalInput();
        _inputJumpHeld = _characterInput.IsJumpPressed();
        _inputGrabPressed = _characterInput.IsGrabPressed();
    }

    private void FixedUpdate()
    {
        // Process buffered input events
        ProcessInputEvents();
        
        CalculateVelocity();

        CalculateGravity();

        if(!_isWallJumpInProgress)
            Walk();

        Jump();

        HandleWallMovement();

        Dash();

        ClampSpeedY();

        Move();
        
        // Periodic debug logging
        DebugLogPositionAndVelocity();
        
        // Clear one-frame input flags
        ClearInputFlags();
    }
    
    private void ProcessInputEvents()
    {
        // Process jump press event
        if (_inputJumpPressed)
        {
            print($"[TEMPLOG] OnJumpPressed - IsOnWall={IsOnWall()}, V={_verticalSpeed:F2}, canWallJump={_canWallJump}");
            _jumpBufferTimeLeft = _jumpBuffer;
            _wallGrabJumpTimer = _wallGrabJumpApexTime;

            if(IsOnWall())
                _canWallJump = true;
        }
        
        // Process jump release event
        if (_inputJumpReleased)
        {
            if(_verticalSpeed > _minJumpSpeed)
            {
                float vBefore = _verticalSpeed;
                _verticalSpeed = _minJumpSpeed;
                LogMovementFunction("OnJumpReleased", _horizontalSpeed, vBefore, _horizontalSpeed, _verticalSpeed, $"minJumpSpeed={_minJumpSpeed:F2}");
            }
        }
        
        // Process dash press event
        if (_inputDashPressed)
        {
            if(_canDash)
            {
                _dashTimer = _dashDuration;
                OnDash?.Invoke();
            }
        }
    }
    
    private void ClearInputFlags()
    {
        // Clear one-frame input events after processing
        _inputJumpPressed = false;
        _inputJumpReleased = false;
        _inputDashPressed = false;
    }
    
    private void DebugLogPositionAndVelocity()
    {
        _debugLogTimer += Time.deltaTime;
        
        if (_debugLogTimer >= DEBUG_LOG_INTERVAL)
        {
            print($"[TEMPLOG] Periodic - Position: {_transform.position}, Velocity: ({_horizontalSpeed:F2}, {_verticalSpeed:F2}), IsOnWall: {IsOnWall()}");
            _debugLogTimer = 0f;
        }
    }
    
    private void LogMovementFunction(string functionName, float hSpeedBefore, float vSpeedBefore, float hSpeedAfter, float vSpeedAfter, string extraInfo = "")
    {
        string extra = string.IsNullOrEmpty(extraInfo) ? "" : $" | {extraInfo}";
        print($"[TEMPLOG] {functionName} - H: {hSpeedBefore:F2} → {hSpeedAfter:F2}, V: {vSpeedBefore:F2} → {vSpeedAfter:F2}{extra}");
    }

    public Vector2 GetVelocity()
    {
        return _velocity;
    }

    public Vector2 GetRawVelocity()
    {
        return _rawMovement;
    }

    public Vector2 GetFurthestPoint()
    {
        return _furthestPoint;
    }

    #region Gravity and Velocity

    private void CalculateVelocity()
    {
        _velocity = ((Vector2)_transform.position - _lastPosition) / Time.fixedDeltaTime;
        _lastPosition = _transform.position;
    }

    private void CalculateGravity()
    {
        if (!_playerCollision.DownCollision.Colliding && !CanCoyoteJump())
            _verticalSpeed -= _gravity * Time.fixedDeltaTime;

        if (_playerCollision.IsVerticallyColliding())
            _verticalSpeed = 0;
    }

    #endregion

    #region Walk

    private void Walk()
    {
        if (_inputHorizontal != 0)
            _horizontalSpeed = Mathf.MoveTowards(_horizontalSpeed, _maxMove * _inputHorizontal, _acceleration * Time.fixedDeltaTime);
        else
            _horizontalSpeed = Mathf.MoveTowards(_horizontalSpeed, 0, _deceleration * Time.fixedDeltaTime);
    }

    #endregion

    #region Jump

    private void OnJumpPressed()
    {
        // Set flag for FixedUpdate to process
        _inputJumpPressed = true;
    }

    private void Jump()
    {
        var downColl = _playerCollision.DownCollision.Colliding;

        CoyoteJump();
        JumpBuffer();
        HandleLanding();

        if (CanJump() && (downColl || CanCoyoteJump()))
        {
            OnJump?.Invoke();
        }

        if (CanJump() && downColl)
        {
            float vBefore = _verticalSpeed;
            _verticalSpeed = _maxJumpSpeed;
            LogMovementFunction("Jump(ground)", _horizontalSpeed, vBefore, _horizontalSpeed, _verticalSpeed, $"maxJumpSpeed={_maxJumpSpeed:F2}, jumpBuffer={_jumpBufferTimeLeft:F3}");
            _jumpBufferTimeLeft = 0; // Consume jump buffer
            return; // Don't check coyote jump if we already jumped from ground
        }

        if (CanJump() && CanCoyoteJump())
        {
            float vBefore = _verticalSpeed;
            _verticalSpeed = _maxJumpSpeed;
            LogMovementFunction("Jump(coyote)", _horizontalSpeed, vBefore, _horizontalSpeed, _verticalSpeed, $"maxJumpSpeed={_maxJumpSpeed:F2}, jumpBuffer={_jumpBufferTimeLeft:F3}");
            _coyoteJumpTimeLeft = 0;
            _jumpBufferTimeLeft = 0; // Consume jump buffer
        }
    }

    private void OnJumpReleased()
    {
        // Set flag for FixedUpdate to process
        _inputJumpReleased = true;
    }

    private void CoyoteJump()
    {
        if (_playerCollision.DownCollision.Colliding)
        {
            _coyoteJumpTimeLeft = _coyoteJump;
            return;
        }

        _coyoteJumpTimeLeft -= Time.fixedDeltaTime;
    }

    private void JumpBuffer()
    {
        if (_verticalSpeed > 0 && _playerCollision.DownCollision.Colliding)
            _jumpBufferTimeLeft = 0;

        _jumpBufferTimeLeft -= Time.fixedDeltaTime;
    }

    private bool CanJump()
    {
        return _jumpBufferTimeLeft > 0;
    }

    private bool CanCoyoteJump()
    {
        return _coyoteJumpTimeLeft > 0;
    }

    private void HandleLanding()
    {
        var downCol = _playerCollision.DownCollision.Colliding;

        if (!downCol)
        {
            _canLand = true;
            return;
        }

        if (_canLand)
        {
            OnLand?.Invoke();
            _canLand = false;
        }
    }

    #endregion

    #region Wall

    public bool IsOnWall()
    {
        return !_playerCollision.DownCollision.Colliding && _playerCollision.IsHorizontallyColliding();
    }

    private bool CanGrab()
    {
        var right = _playerCollision.RightCollision;
        var left = _playerCollision.LeftCollision;

        var rightDistance = right.Distance < _grabDistance && right.RayHit;
        var leftDistance = left.Distance < _grabDistance && left.RayHit;

        return (rightDistance || leftDistance) && _inputGrabPressed;
    }

    private void HandleWallMovement()
    {
        var collision = _playerCollision.GetClosestHorizontal();

        WallSlide();
        WallJump(collision);
        WallGrab(collision, _inputVertical);
        WallGrabJump(collision, _inputHorizontal);

        if (_verticalSpeed <= 0)
            _isWallJumpInProgress = false;
    }

    private void WallSlide()
    {
        _wallStickTimeLeft -= Time.fixedDeltaTime;

        if (!IsOnWall() && _wallStickTimeLeft <= 0)
            return;

        _isWallJumpInProgress = true;    

        if (_verticalSpeed < 0)
            _verticalSpeed = -_wallSlide;
    }

    private void WallJump(CollisionInfo collision)
    {
        if(IsOnWall() && !_inputGrabPressed && _verticalSpeed < 0)
        {
            if(_inputJumpHeld && _canWallJump && collision.LastHit)
            {
                float hBefore = _horizontalSpeed;
                float vBefore = _verticalSpeed;
                _horizontalSpeed = _wallJump.x * -collision.RaycastInfo.RayDirection.x;
                _verticalSpeed = _wallJump.y;
                LogMovementFunction("WallJump", hBefore, vBefore, _horizontalSpeed, _verticalSpeed, $"wallJump=({_wallJump.x:F2}, {_wallJump.y:F2})");
                _wallStickTimeLeft = _wallStickTime;
                _canWallJump = false;
                _jumpBufferTimeLeft = 0; // Consume jump buffer
            }
        }
    }

    private void WallGrabJump(CollisionInfo collision, float input)
    {
        _wallGrabJumpTimer -= Time.fixedDeltaTime;

        if (CanGrab() && collision != null && _inputJumpHeld)
        {
            // Priority 1: Jump OFF the wall (away from wall)
            if(input != 0 && collision.RaycastInfo.RayDirection.x != input)
            {
                float hBefore = _horizontalSpeed;
                float vBefore = _verticalSpeed;
                _horizontalSpeed = _wallJump.x * input;
                _verticalSpeed = _wallJump.y;
                LogMovementFunction("WallGrabJump(off)", hBefore, vBefore, _horizontalSpeed, _verticalSpeed, $"wallJump=({_wallJump.x:F2}, {_wallJump.y:F2})");
                _jumpBufferTimeLeft = 0; // Consume jump buffer
                return;
            }
            
            // Priority 2: Small jump UP while still on wall
            if (_wallGrabJumpTimer > 0)
            {
                float vBefore = _verticalSpeed;
                _verticalSpeed = _wallGrabJumpSpeed;
                LogMovementFunction("WallGrabJump(up)", _horizontalSpeed, vBefore, _horizontalSpeed, _verticalSpeed, $"wallGrabJumpSpeed={_wallGrabJumpSpeed:F2}");
                _jumpBufferTimeLeft = 0; // Consume jump buffer
            }
        }
    }

    private void WallGrab(CollisionInfo collision, float input)
    {
        _wallGrabTimeLeft -= Time.fixedDeltaTime;

        if (_playerCollision.DownCollision.Colliding)
            _wallGrabTimeLeft = _wallGrabTime;

        if (CanGrab() && collision != null && _wallGrabTimeLeft > 0)
        {
            if(collision.FirstHit && collision.HitCount == 1)
            {
                float hBefore = _horizontalSpeed;
                float vBefore = _verticalSpeed;
                _verticalSpeed = _topEdgeClimbJump.y;
                _horizontalSpeed = _topEdgeClimbJump.x * collision.RaycastInfo.RayDirection.x;
                LogMovementFunction("WallGrab(edgeClimb)", hBefore, vBefore, _horizontalSpeed, _verticalSpeed, $"topEdgeClimbJump=({_topEdgeClimbJump.x:F2}, {_topEdgeClimbJump.y:F2})");
                return;
            }

            _verticalSpeed = input * _wallClimb;
            _horizontalSpeed = 0;
        }    
    }

    #endregion

    #region Dash

    private void OnDashPressed()
    {
        // Set flag for FixedUpdate to process
        _inputDashPressed = true;
    }

    private void Dash()
    {
        float inputX = _inputHorizontal;
        float inputY = _inputVertical;

        if (inputX == 0 && inputY == 0)
            inputX = 1;

        Vector2 dir = new Vector2(inputX, inputY).normalized;

        if (_playerCollision.DownCollision.Colliding)
            _canDash = true;

        _dashTimer -= Time.fixedDeltaTime;

        if (_dashTimer <= 0)
        {
            if(!_dashJustEnded)
            {
                float vBefore = _verticalSpeed;
                _verticalSpeed = _ySpeedAfterDash * inputY;
                LogMovementFunction("Dash(end)", _horizontalSpeed, vBefore, _horizontalSpeed, _verticalSpeed, $"ySpeedAfterDash={_ySpeedAfterDash:F2}, inputY={inputY:F2}");
                _dashJustEnded = true;
            }

            return;
        }

        var furthestPoint = GetDashFurthestPoint(dir);
        var hit = _playerCollision.GetDashHitPos(dir, furthestPoint);
        var distance = Vector2.Distance(_transform.position, hit);
        var clampDistance = Mathf.Clamp(distance, 0, _dashDistance);
        var velocity = clampDistance / _dashDuration * dir;
        float hBefore = _horizontalSpeed;
        float vBefore2 = _verticalSpeed;
        _horizontalSpeed = velocity.x;
        _verticalSpeed = velocity.y;
        LogMovementFunction("Dash(active)", hBefore, vBefore2, _horizontalSpeed, _verticalSpeed, $"dir=({dir.x:F2}, {dir.y:F2}), dist={clampDistance:F2}");
        _canDash = false;
        _dashJustEnded = false;
    }

    private Vector2 GetDashFurthestPoint(Vector2 dir)
    {
        return (Vector2)_transform.position + dir * _dashDistance;
    }

    #endregion

    private void ClampSpeedY()
    {
        if(_verticalSpeed < 0)
            _verticalSpeed = Mathf.Clamp(_verticalSpeed, -_maxFallSpeed, -_minFallSpeed);
    }

    private void Move()
    {
        var pos = _transform.position;
        _rawMovement = new Vector2(_horizontalSpeed, _verticalSpeed);
        var move = _rawMovement * Time.fixedDeltaTime;
        _furthestPoint = (Vector2)pos + move;

        _playerCollision.HandleCollisions(_furthestPoint, ref move, _rawMovement);

        _transform.position += (Vector3)move;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(_furthestPoint, Vector2.one);
    }

    private void OnDestroy()
    {
        if (_characterInput != null)
        {
            _characterInput.onJumpPressed -= OnJumpPressed;
            _characterInput.onJumpReleased -= OnJumpReleased;
            _characterInput.onDashPressed -= OnDashPressed;
        }
    }
}

