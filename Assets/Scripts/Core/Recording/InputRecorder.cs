// Records input from any ICharacterInput implementation.
// Can record player input, AI input, or even replay input for debugging.

using UnityEngine;

[RequireComponent(typeof(ICharacterInput))]
public class InputRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    [SerializeField] private string _recordingName = "New Recording";
    [SerializeField] private bool _autoStart = false;
    
    [Header("Recording State")]
    [SerializeField] private bool _isRecording = false;
    
    private ICharacterInput _input;
    private InputRecording _currentRecording;
    private int _frameNumber = 0;
    private float _startTime = 0f;
    
    private void Awake()
    {
        _input = GetComponent<ICharacterInput>();
    }
    
    private void Start()
    {
        if (_autoStart)
        {
            StartRecording(_recordingName);
        }
    }
    
    private void FixedUpdate()
    {
        if (!_isRecording || _currentRecording == null)
            return;
            
        // Record current input state
        InputState inputState = _input.GetInputState();
        _currentRecording.AddFrame(_frameNumber, inputState);
        _frameNumber++;
    }
    
    /// <summary>
    /// Starts recording input.
    /// </summary>
    public void StartRecording(string recordingName = null)
    {
        if (_isRecording)
        {
            Debug.LogWarning("Already recording! Stop current recording first.");
            return;
        }
        
        if (_input == null)
        {
            Debug.LogError("Cannot start recording: no ICharacterInput component found!");
            return;
        }
        
        // Create new recording
        _currentRecording = ScriptableObject.CreateInstance<InputRecording>();
        _currentRecording.Initialize(
            recordingName ?? _recordingName,
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            transform.position
        );
        
        _isRecording = true;
        _frameNumber = 0;
        _startTime = Time.time;
        
        Debug.Log($"Started recording '{_currentRecording.recordingName}'");
    }
    
    /// <summary>
    /// Stops recording and returns the completed recording.
    /// </summary>
    public InputRecording StopRecording()
    {
        if (!_isRecording)
        {
            Debug.LogWarning("Not currently recording!");
            return null;
        }
        
        _isRecording = false;
        float completionTime = Time.time - _startTime;
        _currentRecording.Finalize(completionTime);
        
        Debug.Log($"Stopped recording '{_currentRecording.recordingName}' - {_frameNumber} frames, {completionTime:F2}s");
        
        InputRecording recording = _currentRecording;
        _currentRecording = null;
        
        return recording;
    }
    
    /// <summary>
    /// Stops recording and saves to a JSON file in StreamingAssets.
    /// </summary>
    public InputRecording StopAndSave(string subfolder = "Temporary")
    {
        InputRecording recording = StopRecording();
        
        if (recording != null)
        {
            SaveRecording(recording, subfolder);
        }
        
        return recording;
    }
    
    /// <summary>
    /// Saves a recording to a JSON file.
    /// </summary>
    public void SaveRecording(InputRecording recording, string subfolder = "Temporary")
    {
        if (recording == null)
        {
            Debug.LogError("Cannot save null recording");
            return;
        }
        
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "Recordings", subfolder);
        
        // Create directory if it doesn't exist
        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }
        
        string filename = $"{recording.recordingName.Replace(" ", "_")}_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
        string fullPath = System.IO.Path.Combine(path, filename);
        
        // Serialize to JSON
        string json = JsonUtility.ToJson(recording, true);
        System.IO.File.WriteAllText(fullPath, json);
        
        Debug.Log($"Saved recording to: {fullPath}");
    }
    
    /// <summary>
    /// Loads a recording from a JSON file.
    /// </summary>
    public static InputRecording LoadRecording(string filepath)
    {
        if (!System.IO.File.Exists(filepath))
        {
            Debug.LogError($"Recording file not found: {filepath}");
            return null;
        }
        
        string json = System.IO.File.ReadAllText(filepath);
        InputRecording recording = ScriptableObject.CreateInstance<InputRecording>();
        JsonUtility.FromJsonOverwrite(json, recording);
        
        if (!recording.Validate())
        {
            Debug.LogError($"Loaded recording failed validation: {filepath}");
        }
        
        Debug.Log($"Loaded recording '{recording.recordingName}' ({recording.FrameCount} frames)");
        return recording;
    }
    
    // Public properties for debugging
    public bool IsRecording => _isRecording;
    public int CurrentFrame => _frameNumber;
    public float RecordingTime => _isRecording ? (Time.time - _startTime) : 0f;
}

