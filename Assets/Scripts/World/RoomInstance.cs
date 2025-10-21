// Wrapper for a generated room, tracking its position, state, and portals.

using UnityEngine;

public class RoomInstance
{
    public int RoomX { get; private set; }
    public Vector3 WorldOffset { get; private set; }
    public RoomGenerator Generator { get; private set; }
    public GameObject RootObject { get; private set; }
    public Portal[] Portals { get; private set; }
    public bool IsGenerated { get; private set; }
    public Bounds WorldBounds { get; private set; }
    
    public RoomInstance(int roomX, Vector3 worldOffset, GameObject roomPrefab, Transform parent, int globalSeed)
    {
        RoomX = roomX;
        WorldOffset = worldOffset;
        
        // Instantiate room at world offset
        RootObject = Object.Instantiate(roomPrefab, worldOffset, Quaternion.identity, parent);
        RootObject.name = $"Room_{roomX}";
        
        // Get RoomGenerator component
        Generator = RootObject.GetComponent<RoomGenerator>();
        if (Generator == null)
        {
            Debug.LogError($"Room prefab missing RoomGenerator component!");
            return;
        }
        
        // Configure generator
        Generator.SetRoomCoordinates(roomX, 0, globalSeed);
        
        // Generate the room
        Generator.GenerateRoom();
        IsGenerated = true;
        
        // Calculate and store world bounds
        WorldBounds = Generator.GetWorldBounds();
        
        // Find and setup portal components
        SetupPortals();
        
        Debug.Log($"Room {roomX} bounds: Center={WorldBounds.center}, Size={WorldBounds.size}");
    }
    
    private void SetupPortals()
    {
        // Find portal GameObjects created by RoomGenerator
        Transform[] portalTransforms = new Transform[3];
        for (int i = 0; i < RootObject.transform.childCount; i++)
        {
            Transform child = RootObject.transform.GetChild(i);
            if (child.name.StartsWith("Portal_"))
            {
                int portalIndex = ExtractPortalIndex(child.name);
                if (portalIndex >= 0 && portalIndex < 3)
                {
                    portalTransforms[portalIndex] = child;
                }
            }
        }
        
        // Add Portal components
        Portals = new Portal[3];
        for (int i = 0; i < 3; i++)
        {
            if (portalTransforms[i] == null)
            {
                Debug.LogWarning($"Portal {i} not found in room {RoomX}");
                continue;
            }
            
            GameObject portalObj = portalTransforms[i].gameObject;
            
            // Add Portal component
            Portal portal = portalObj.AddComponent<Portal>();
            typeof(Portal).GetField("_type", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(portal, (Portal.PortalType)i);
            
            portal.OwnerRoom = this;
            
            // Add trigger collider if not present
            CircleCollider2D collider = portalObj.GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = portalObj.AddComponent<CircleCollider2D>();
            }
            collider.isTrigger = true;
            collider.radius = 1.5f;
            
            Portals[i] = portal;
        }
    }
    
    private int ExtractPortalIndex(string portalName)
    {
        // Portal names: "Portal_0_Left", "Portal_1_Right", "Portal_2_Center"
        if (portalName.Length > 7)
        {
            char indexChar = portalName[7];
            if (char.IsDigit(indexChar))
            {
                return indexChar - '0';
            }
        }
        return -1;
    }
    
    public void Destroy()
    {
        if (RootObject != null)
        {
            Object.Destroy(RootObject);
        }
    }
}


