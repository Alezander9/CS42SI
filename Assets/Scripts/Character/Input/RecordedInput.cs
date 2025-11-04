// Plays back recorded input data as if it were live input.
// Implements ICharacterInput to work with CharacterController.

using System;
using UnityEngine;

public class RecordedInput : MonoBehaviour, ICharacterInput
{
    public event Action onJumpPressed;
    public event Action onJumpReleased;
    public event Action onDashPressed;
    
    [Header("Recording")]
    [SerializeField] private InputRecording _recording;
    
    [Header("Playback State")]
    [SerializeField] private bool _isPlaying = false;
    [SerializeField] private bool _loop = false;
    
    private int _currentFrame = 0;
    private InputState _currentInput;
    private InputState _previousInput;
    
    private void Start()
    {
        if (_recording != null)
        {
            StartPlayback();
        }
    }
    
    private void FixedUpdate()
    {
        if (!_isPlaying || _recording == null)
            return;
            
        // Store previous input to detect events
        _previousInput = _currentInput;
        
        // Get current frame input
        if (_currentFrame < _recording.FrameCount)
        {
            _currentInput = _recording.GetInputAtFrame(_currentFrame);
            _currentFrame++;
        }
        else
        {
            // Recording finished
            if (_loop)
            {
                _currentFrame = 0;
                _currentInput = _recording.GetInputAtFrame(_currentFrame);
            }
            else
            {
                _isPlaying = false;
                _currentInput = InputState.Zero;
                return;
            }
        }
        
        // Fire events for one-frame inputs
        if (_currentInput.JumpPressed && !_previousInput.JumpPressed)
            onJumpPressed?.Invoke();
            
        if (_currentInput.JumpReleased && !_previousInput.JumpReleased)
            onJumpReleased?.Invoke();
            
        if (_currentInput.DashPressed && !_previousInput.DashPressed)
            onDashPressed?.Invoke();
    }
    
    /// <summary>
    /// Loads a recording and prepares for playback.
    /// </summary>
    public void LoadRecording(InputRecording recording)
    {
        _recording = recording;
        _currentFrame = 0;
        _currentInput = InputState.Zero;
        _previousInput = InputState.Zero;
        
        if (_recording != null && !_recording.Validate())
        {
            Debug.LogError($"Recording '{_recording.recordingName}' failed validation!");
        }
    }
    
    /// <summary>
    /// Starts playback of the loaded recording.
    /// </summary>
    public void StartPlayback()
    {
        if (_recording == null)
        {
            Debug.LogError("Cannot start playback: no recording loaded");
            return;
        }
        
        _isPlaying = true;
        _currentFrame = 0;
        _currentInput = InputState.Zero;
        _previousInput = InputState.Zero;
    }
    
    /// <summary>
    /// Stops playback.
    /// </summary>
    public void StopPlayback()
    {
        _isPlaying = false;
    }
    
    /// <summary>
    /// Resets playback to the beginning.
    /// </summary>
    public void ResetPlayback()
    {
        _currentFrame = 0;
        _currentInput = InputState.Zero;
        _previousInput = InputState.Zero;
    }
    
    // ICharacterInput implementation
    public InputState GetInputState() => _currentInput;
    
    // Public properties for debugging
    public bool IsPlaying => _isPlaying;
    public int CurrentFrame => _currentFrame;
    public int TotalFrames => _recording?.FrameCount ?? 0;
    public float Progress => _recording != null ? (float)_currentFrame / _recording.FrameCount : 0f;
}

