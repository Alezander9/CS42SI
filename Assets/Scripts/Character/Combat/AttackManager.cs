 // Manages attack execution and coordinates between input and attack behaviors.
// Sits alongside CharacterController without coupling to movement logic.

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CharacterCollision))]
public class AttackManager : MonoBehaviour
{
    private CharacterController _character;
    private CharacterCollision _collision;
    private ICharacterInput _input;
    
    private IAttackBehavior _currentAttack;
    private AttackContext _context;
    private int _attackFrameCounter;
    
    // Map of attack inputs to behaviors (will be populated later with attack sets)
    private Dictionary<AttackInput, IAttackBehavior> _attackMap;
    
    private void Awake()
    {
        _character = GetComponent<CharacterController>();
        _collision = GetComponent<CharacterCollision>();
        
        _context = new AttackContext
        {
            Character = _character,
            Collision = _collision,
            Transform = transform
        };
        
        _attackMap = new Dictionary<AttackInput, IAttackBehavior>();
    }
    
    private void Start()
    {
        _input = GetComponent<ICharacterInput>();
        
        if (_input == null)
        {
            Debug.LogError("AttackManager requires a component implementing ICharacterInput");
            enabled = false;
            return;
        }
    }
    
    private void FixedUpdate()
    {
        UpdateContext();
        ProcessAttackInput();
        TickCurrentAttack();
    }
    
    private void ProcessAttackInput()
    {
        InputState input = _input.GetInputState();
        
        // Don't start new attack if one is active and can't be interrupted
        if (_currentAttack != null && !_currentAttack.CanBeInterrupted)
            return;
        
        // Check for attack button press
        AttackType? attackType = null;
        if (input.LightAttackPressed) 
            attackType = AttackType.Light;
        else if (input.HeavyAttackPressed) 
            attackType = AttackType.Heavy;
        
        if (!attackType.HasValue)
            return;
        
        // Build attack input from current context
        AttackInput attackInput = new AttackInput(
            DetermineGroundState(),
            DetermineDirection(input),
            attackType.Value
        );
        
        // Get corresponding attack behavior
        if (!_attackMap.TryGetValue(attackInput, out IAttackBehavior attack))
            return;
        
        if (!attack.CanExecute(_context))
            return;
        
        // Execute attack
        if (attack.Execute(_context))
        {
            _currentAttack = attack;
            _attackFrameCounter = 0;
        }
    }
    
    private void TickCurrentAttack()
    {
        if (_currentAttack == null)
            return;
        
        _currentAttack.FixedTick(_context);
        _attackFrameCounter++;
        
        // End attack when duration is reached
        if (_attackFrameCounter >= _currentAttack.DurationFrames)
        {
            _currentAttack = null;
        }
    }
    
    private GroundState DetermineGroundState()
    {
        return _collision.DownCollision.Colliding 
            ? GroundState.Grounded 
            : GroundState.Airborne;
    }
    
    private AttackDirection DetermineDirection(InputState input)
    {
        // Prioritize vertical over horizontal (typical for platform fighters)
        if (input.Vertical > 0.5f) 
            return AttackDirection.Up;
        if (input.Vertical < -0.5f) 
            return AttackDirection.Down;
        if (Mathf.Abs(input.Horizontal) > 0.5f) 
            return AttackDirection.Side;
        
        return AttackDirection.Neutral;
    }
    
    private void UpdateContext()
    {
        _context.CurrentFrame = _attackFrameCounter;
        
        InputState input = _input.GetInputState();
        _context.InputDirection = new Vector2(input.Horizontal, input.Vertical);
        
        // Get facing direction from character controller
        _context.FacingDirection = _character.GetFacingDirection();
    }
    
    /// <summary>
    /// Registers an attack behavior for a specific input combination.
    /// </summary>
    public void RegisterAttack(AttackInput input, IAttackBehavior behavior)
    {
        _attackMap[input] = behavior;
    }
    
    public bool IsAttacking() => _currentAttack != null;
}

