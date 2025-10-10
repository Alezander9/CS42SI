// Bootstrap script for test scenes to setup various character scenarios.
// Spawns characters with different input configurations for testing.

using UnityEngine;

public class TestSceneBootstrap : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Vector3 _playerSpawnPosition = new Vector3(0, 2, 0);
    [SerializeField] private string _characterPrefabPath = "Prefabs/Character";
    
    [Header("Test Scenarios")]
    [SerializeField] private bool _spawnPlayer = true;
    
    private GameObject _characterPrefab;
    private CharacterController _playerCharacter;

    private void Start()
    {
        LoadPrefab();
        
        if (_spawnPlayer)
        {
            SpawnPlayer();
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
        
        var controller = characterObj.GetComponent<CharacterController>();
        
        if (controller == null)
        {
            Debug.LogError("Character prefab missing CharacterController component!");
            Destroy(characterObj);
            return null;
        }

        _playerCharacter = controller;
        Debug.Log($"Player spawned at {position}");
        
        return controller;
    }

    /// <summary>
    /// Gets the current player character controller.
    /// </summary>
    public CharacterController GetPlayer()
    {
        return _playerCharacter;
    }

    // Future methods for different test scenarios:
    // public CharacterController SpawnGhost(Vector3 position, RecordedInputData replay) { }
    // public CharacterController SpawnAI(Vector3 position, AIBehavior behavior) { }
    // public void SpawnMultiplePlayers(int count) { }
}

