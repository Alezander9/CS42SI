// Portal component that detects player collision and triggers room transitions.

using UnityEngine;

public class Portal : MonoBehaviour
{
    public enum PortalType { Left, Right, Center }
    
    [SerializeField] private PortalType _type;
    
    public PortalType Type => _type;
    public RoomInstance OwnerRoom { get; set; }
    public Portal LinkedPortal { get; set; }
    
    private WorldManager _worldManager;
    
    private void Start()
    {
        _worldManager = FindObjectOfType<WorldManager>();
        if (_worldManager == null)
        {
            Debug.LogError("WorldManager not found in scene!");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only left/right portals trigger room transitions
        if (_type == PortalType.Center)
            return;
            
        if (other.CompareTag("Player"))
        {
            _worldManager?.OnPortalEntered(this, other.transform);
        }
    }
    
    private void OnDrawGizmos()
    {
        // Visualize portal type
        Gizmos.color = _type == PortalType.Left ? Color.red : 
                       _type == PortalType.Right ? Color.green : 
                       Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw link to connected portal
        if (LinkedPortal != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, LinkedPortal.transform.position);
        }
    }
}


