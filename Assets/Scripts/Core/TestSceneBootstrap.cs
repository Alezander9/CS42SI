// Bootstrap script for test scenes to setup various character scenarios.
// Spawns characters with different input configurations for testing.

using UnityEngine;

public class TestSceneBootstrap : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Vector3 _playerSpawnPosition = new Vector3(0, 2, 0);
    [SerializeField] private string _characterPrefabPath = "Prefabs/Character";
    [SerializeField] private bool _useWorldManager = true;
    
    [Header("Test Scenarios")]
    [SerializeField] private bool _spawnPlayer = true;
    [SerializeField] private bool _recordPlayer = false;
    
    [Header("Ghost Settings")]
    [SerializeField] private Vector3 _ghostSpawnOffset = new Vector3(0, 0, 0);
    [SerializeField] private Color _ghostColor = new Color(1, 1, 1, 0.5f);
    
    private GameObject _characterPrefab;
    private CharacterController _playerCharacter;
    private InputRecorder _recorder;
    private RoomGenerator _roomGenerator;
    private WorldManager _worldManager;
    private CameraManager _cameraManager;

    private void Start()
    {
        LoadPrefab();
        
        // Check if using WorldManager or standalone RoomGenerator
        _worldManager = FindObjectOfType<WorldManager>();
        
        if (_useWorldManager && _worldManager != null)
        {
            // Initialize world (generates 3 rooms)
            _worldManager.InitializeWorld();
            
            // Get spawn position from current room's center portal
            _playerSpawnPosition = _worldManager.GetCurrentRoomCenterPortalPosition();
        }
        else
        {
            // Fallback to standalone RoomGenerator (legacy mode)
            _roomGenerator = FindObjectOfType<RoomGenerator>();
            if (_roomGenerator != null)
            {
                _roomGenerator.GenerateRoom();
                
                Vector3[] portals = _roomGenerator.GetPortalWorldPositions();
                _playerSpawnPosition = portals[2]; // Index 2 is center portal
            }
            else
            {
                Debug.LogWarning("No WorldManager or RoomGenerator found in scene. Using default spawn position.");
            }
        }
        
        if (_spawnPlayer)
        {
            SpawnPlayer();
            SetupCamera();
            
            // Register player with WorldManager if it exists
            if (_worldManager != null && _playerCharacter != null)
            {
                _worldManager.SetPlayerCharacter(_playerCharacter.transform);
            }
        }
    }
    
    private void SetupCamera()
    {
        _cameraManager = FindObjectOfType<CameraManager>();
        
        if (_cameraManager != null && _playerCharacter != null)
        {
            Vector3 cameraStartPos = _cameraManager.transform.position;
            var followCam = new FollowPlayerCamera(_playerCharacter.transform, cameraStartPos);
            _cameraManager.SetController(followCam);
        }
    }

    private void LoadPrefab()
    {
        _characterPrefab = Resources.Load<GameObject>(_characterPrefabPath);
        
        if (_characterPrefab == null)
        {
            Debug.LogError($"Failed to load character prefab at Resources/{_characterPrefabPath}");
        }
    }

    /// <summary>
    /// Spawns a player-controlled character at the configured spawn position.
    /// </summary>
    public CharacterController SpawnPlayer()
    {
        return SpawnPlayer(_playerSpawnPosition);
    }

    /// <summary>
    /// Spawns a player-controlled character at a specific position.
    /// </summary>
    public CharacterController SpawnPlayer(Vector3 position)
    {
        if (_characterPrefab == null)
        {
            Debug.LogError("Cannot spawn player: character prefab not loaded");
            return null;
        }

        var characterObj = Instantiate(_characterPrefab, position, Quaternion.identity);
        characterObj.name = "Player";
        
        // Add PlayerInput component to make it player-controlled
        var playerInput = characterObj.AddComponent<PlayerInput>();
        
        // Add AttackManager and register test attacks
        var attackManager = characterObj.AddComponent<AttackManager>();
        RegisterTestAttacks(attackManager);
        
        var controller = characterObj.GetComponent<CharacterController>();
        
        if (controller == null)
        {
            Debug.LogError("Character prefab missing CharacterController component!");
            Destroy(characterObj);
            return null;
        }

        _playerCharacter = controller;
        
        return controller;
    }

    /// <summary>
    /// Gets the current player character controller.
    /// </summary>
    public CharacterController GetPlayer()
    {
        return _playerCharacter;
    }
    
    /// <summary>
    /// Spawns a player with input recording enabled.
    /// </summary>
    public CharacterController SpawnPlayerWithRecording(Vector3 position, string recordingName = "Test Recording")
    {
        var player = SpawnPlayer(position);
        
        if (player != null)
        {
            _recorder = player.gameObject.AddComponent<InputRecorder>();
            _recorder.StartRecording(recordingName);
        }
        
        return player;
    }
    
    /// <summary>
    /// Stops recording and returns the recorded data.
    /// </summary>
    public InputRecording StopRecording(bool saveToFile = true)
    {
        if (_recorder == null)
        {
            Debug.LogWarning("No active recorder to stop");
            return null;
        }
        
        InputRecording recording = saveToFile ? _recorder.StopAndSave("Temporary") : _recorder.StopRecording();
        return recording;
    }
    
    /// <summary>
    /// Spawns a ghost character that plays back a recording.
    /// </summary>
    public CharacterController SpawnGhost(Vector3 position, InputRecording recording)
    {
        if (_characterPrefab == null)
        {
            Debug.LogError("Cannot spawn ghost: character prefab not loaded");
            return null;
        }
        
        if (recording == null)
        {
            Debug.LogError("Cannot spawn ghost: recording is null");
            return null;
        }
        
        var characterObj = Instantiate(_characterPrefab, position, Quaternion.identity);
        characterObj.name = $"Ghost ({recording.recordingName})";
        
        // Add RecordedInput component to make it replay the recording
        var recordedInput = characterObj.AddComponent<RecordedInput>();
        recordedInput.LoadRecording(recording);
        
        // Add AttackManager and register test attacks
        var attackManager = characterObj.AddComponent<AttackManager>();
        RegisterTestAttacks(attackManager);
        
        // Make ghost semi-transparent
        var spriteRenderer = characterObj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = _ghostColor;
        }
        
        var controller = characterObj.GetComponent<CharacterController>();
        
        if (controller == null)
        {
            Debug.LogError("Character prefab missing CharacterController component!");
            Destroy(characterObj);
            return null;
        }
        
        return controller;
    }
    
    /// <summary>
    /// Spawns a ghost at an offset from the player spawn position.
    /// </summary>
    public CharacterController SpawnGhostAtOffset(InputRecording recording)
    {
        Vector3 spawnPos = _playerSpawnPosition + _ghostSpawnOffset;
        return SpawnGhost(spawnPos, recording);
    }
    
    /// <summary>
    /// Quick test: Spawn player with recording, then spawn a ghost from that recording when stopped.
    /// Press R in play mode to start recording, press R again to spawn ghost.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (_recorder == null || !_recorder.IsRecording)
            {
                // Start recording
                if (_playerCharacter != null)
                {
                    _recorder = _playerCharacter.gameObject.AddComponent<InputRecorder>();
                    _recorder.StartRecording("Quick Test");
                }
                else
                {
                    Debug.LogWarning("No player to record!");
                }
            }
            else
            {
                // Stop recording and spawn ghost
                var recording = _recorder.StopRecording();
                if (recording != null)
                {
                    SpawnGhostAtOffset(recording);
                }
            }
        }
    }
    
    /// <summary>
    /// Registers test attacks for proof of concept testing.
    /// In a real game, this would be replaced by a proper attack set/class system.
    /// </summary>
    private void RegisterTestAttacks(AttackManager attackManager)
    {
        if (attackManager == null)
            return;
        
        // Register all possible attack combinations with test behaviors
        attackManager.RegisterAttack(
            new AttackInput(GroundState.Grounded, AttackDirection.Neutral, AttackType.Light),
            new TestAttackBehavior("Grounded Neutral Light")
        );
        
        attackManager.RegisterAttack(
            new AttackInput(GroundState.Grounded, AttackDirection.Side, AttackType.Light),
            new TestAttackBehavior("Grounded Side Light")
        );
        
    }
}

