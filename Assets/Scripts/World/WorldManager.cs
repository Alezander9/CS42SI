// Manages multiple rooms, portal connections, and player transitions between rooms.

using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorldManager : MonoBehaviour
{
    [Header("World Settings")]
    [SerializeField] private int _globalSeed = 12345;
    [SerializeField] private float _roomSpacing = 10000f;
    [SerializeField] private int _startingRoomX = 0;
    
    [Header("References")]
    [SerializeField] private GameObject _roomPrefab;
    [SerializeField] private Transform _roomsContainer;
    
    [Header("Player")]
    [SerializeField] private float _portalExitOffset = 2f;
    
    private int _currentRoomX;
    private Dictionary<int, RoomInstance> _loadedRooms = new Dictionary<int, RoomInstance>();
    private Transform _playerTransform;
    
    private void Awake()
    {
        if (_roomsContainer == null)
        {
            _roomsContainer = transform;
        }
    }
    
    /// <summary>
    /// Initialize world by generating the starting three rooms (left, current, right).
    /// Current room starts at origin (0,0).
    /// </summary>
    public void InitializeWorld()
    {
        _currentRoomX = _startingRoomX;
        
        // Generate initial three rooms with current room at origin
        // Left room at -roomSpacing, current at 0, right at +roomSpacing
        GenerateRoomAtPosition(_currentRoomX - 1, new Vector3(-_roomSpacing, 0, 0));
        GenerateRoomAtPosition(_currentRoomX, Vector3.zero);
        GenerateRoomAtPosition(_currentRoomX + 1, new Vector3(_roomSpacing, 0, 0));
        
        // Link portals between adjacent rooms
        LinkPortals();
        
        Debug.Log($"World initialized at room {_currentRoomX} with rooms [{_currentRoomX - 1}, {_currentRoomX}, {_currentRoomX + 1}] (current at origin)");
    }
    
    /// <summary>
    /// Get the world position of the center portal in the current room for player spawning.
    /// </summary>
    public Vector3 GetCurrentRoomCenterPortalPosition()
    {
        if (_loadedRooms.TryGetValue(_currentRoomX, out RoomInstance room))
        {
            if (room.Portals[2] != null)
            {
                return room.Portals[2].transform.position;
            }
        }
        
        Debug.LogWarning("Could not get center portal position, using origin");
        return Vector3.zero;
    }
    
    /// <summary>
    /// Called when player enters a portal. Handles teleportation and room shifting.
    /// </summary>
    public void OnPortalEntered(Portal portal, Transform player)
    {
        _playerTransform = player;
        
        if (portal.LinkedPortal == null)
        {
            Debug.LogWarning($"Portal {portal.Type} in room {portal.OwnerRoom.RoomX} has no linked portal!");
            return;
        }
        
        // Shift rooms if entering edge portal
        int newRoomX = portal.LinkedPortal.OwnerRoom.RoomX;
        if (newRoomX != _currentRoomX)
        {
            // Calculate player's offset from destination portal before shift
            Vector3 destinationPortal = portal.LinkedPortal.transform.position;
            Vector3 exitDirection = portal.LinkedPortal.Type == Portal.PortalType.Left ? Vector3.right : Vector3.left;
            Vector3 offsetFromPortal = exitDirection * _portalExitOffset;
            
            // Shift world so new room is at origin
            ShiftWorldToRoom(newRoomX);
            
            // Teleport player to linked portal (now at new position after shift)
            Vector3 newDestination = portal.LinkedPortal.transform.position + offsetFromPortal;
            player.position = newDestination;
            
            // Instantly move camera to player position (no lerp)
            CameraManager cameraManager = FindObjectOfType<CameraManager>();
            if (cameraManager != null)
            {
                Camera cam = cameraManager.GetComponent<Camera>();
                if (cam != null)
                {
                    Vector3 camPos = cam.transform.position;
                    camPos.x = newDestination.x;
                    camPos.y = newDestination.y;
                    cam.transform.position = camPos;
                }
            }
            
            Debug.Log($"Teleported player to room {newRoomX} at {newDestination}");
        }
    }
    
    /// <summary>
    /// Shift the entire world so the specified room becomes centered at origin.
    /// This keeps the player's current room always near (0,0) for better floating point precision.
    /// </summary>
    private void ShiftWorldToRoom(int newCurrentRoomX)
    {
        int oldCurrentRoomX = _currentRoomX;
        
        // Calculate the offset needed to move new current room to origin
        RoomInstance newCurrentRoom = _loadedRooms[newCurrentRoomX];
        Vector3 shiftOffset = -newCurrentRoom.WorldOffset;
        
        // Shift all loaded rooms
        foreach (var kvp in _loadedRooms)
        {
            RoomInstance room = kvp.Value;
            room.RootObject.transform.position += shiftOffset;
            room.UpdateWorldOffset(room.WorldOffset + shiftOffset);
        }
        
        // Shift player
        if (_playerTransform != null)
        {
            _playerTransform.position += shiftOffset;
        }
        
        // Update current room
        _currentRoomX = newCurrentRoomX;
        
        // Determine which room to unload and which to generate
        if (newCurrentRoomX > oldCurrentRoomX)
        {
            // Moved right: unload left, generate new right
            int unloadX = newCurrentRoomX - 2;
            int generateX = newCurrentRoomX + 1;
            Vector3 newRoomPosition = new Vector3(_roomSpacing, 0, 0); // Right of origin
            
            UnloadRoom(unloadX);
            GenerateRoomAtPosition(generateX, newRoomPosition);
        }
        else
        {
            // Moved left: unload right, generate new left
            int unloadX = newCurrentRoomX + 2;
            int generateX = newCurrentRoomX - 1;
            Vector3 newRoomPosition = new Vector3(-_roomSpacing, 0, 0); // Left of origin
            
            UnloadRoom(unloadX);
            GenerateRoomAtPosition(generateX, newRoomPosition);
        }
        
        // Re-link portals
        LinkPortals();
        
        Debug.Log($"Shifted world: room {_currentRoomX} now at origin. Loaded rooms [{_currentRoomX - 1}, {_currentRoomX}, {_currentRoomX + 1}]");
    }
    
    private void GenerateRoomAtPosition(int roomX, Vector3 worldPosition)
    {
        if (_loadedRooms.ContainsKey(roomX))
        {
            Debug.LogWarning($"Room {roomX} already loaded!");
            return;
        }
        
        RoomInstance room = new RoomInstance(roomX, worldPosition, _roomPrefab, _roomsContainer, _globalSeed);
        _loadedRooms[roomX] = room;
        
        Debug.Log($"Generated room {roomX} at world position {worldPosition}");
    }
    
    private void UnloadRoom(int roomX)
    {
        if (!_loadedRooms.TryGetValue(roomX, out RoomInstance room))
        {
            return;
        }
        
        room.Destroy();
        _loadedRooms.Remove(roomX);
        
        // TODO: Serialize room state before destroying
        // TODO: Save destructible terrain changes
        // TODO: Save entity positions/states
        
        Debug.Log($"Unloaded room {roomX}");
    }
    
    private void LinkPortals()
    {
        foreach (var kvp in _loadedRooms)
        {
            int roomX = kvp.Key;
            RoomInstance room = kvp.Value;
            
            // Link left portal to right neighbor's right portal
            if (_loadedRooms.TryGetValue(roomX - 1, out RoomInstance leftNeighbor))
            {
                if (room.Portals[0] != null && leftNeighbor.Portals[1] != null)
                {
                    room.Portals[0].LinkedPortal = leftNeighbor.Portals[1];
                }
            }
            
            // Link right portal to left neighbor's left portal
            if (_loadedRooms.TryGetValue(roomX + 1, out RoomInstance rightNeighbor))
            {
                if (room.Portals[1] != null && rightNeighbor.Portals[0] != null)
                {
                    room.Portals[1].LinkedPortal = rightNeighbor.Portals[0];
                }
            }
        }
    }
    
    /// <summary>
    /// Get the bounds of a loaded room, or empty bounds if not loaded.
    /// </summary>
    public Bounds GetRoomBounds(int roomX)
    {
        if (_loadedRooms.TryGetValue(roomX, out RoomInstance room))
        {
            return room.WorldBounds;
        }
        return new Bounds(Vector3.zero, Vector3.zero);
    }
    
    /// <summary>
    /// Check if a world position is within the current room's bounds.
    /// </summary>
    public bool IsPositionInCurrentRoom(Vector3 worldPosition)
    {
        if (_loadedRooms.TryGetValue(_currentRoomX, out RoomInstance room))
        {
            return room.WorldBounds.Contains(worldPosition);
        }
        return false;
    }
    
    private void OnDrawGizmos()
    {
        if (_loadedRooms == null || _loadedRooms.Count == 0)
            return;
        
        // Draw actual room bounds from tilemap
        foreach (var kvp in _loadedRooms)
        {
            int roomX = kvp.Key;
            RoomInstance room = kvp.Value;
            
            // Color based on room type
            Gizmos.color = roomX == _currentRoomX ? Color.green : Color.yellow;
            
            // Draw actual room boundary from calculated bounds
            Gizmos.DrawWireCube(room.WorldBounds.center, room.WorldBounds.size);
            
            // Draw room number label at top of room
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(
                room.WorldBounds.center + Vector3.up * (room.WorldBounds.size.y / 2 + 2),
                $"Room {roomX}",
                new GUIStyle() { normal = new GUIStyleState() { textColor = Color.white }, fontSize = 16 }
            );
            #endif
        }
        
        // Draw player position indicator if available
        if (_playerTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_playerTransform.position, 1f);
        }
    }
}


