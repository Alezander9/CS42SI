// ScriptableObject container for storing recorded input data.
// Can be saved as JSON and loaded for replay/ghost playback.

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recording", menuName = "Speedrun/Input Recording")]
public class InputRecording : ScriptableObject
{
    [Header("Metadata")]
    public string recordingName = "Untitled Recording";
    public string timestamp;
    public int version = 1;
    
    [Header("Recording Info")]
    public string levelName;
    public float completionTime;
    public Vector3 startPosition;
    
    [Header("Settings (for validation)")]
    public int physicsFrameRate = 60;
    
    [Header("Input Data")]
    public List<InputFrame> frames = new List<InputFrame>();
    
    /// <summary>
    /// Initializes a new recording with metadata.
    /// </summary>
    public void Initialize(string name, string level, Vector3 startPos)
    {
        recordingName = name;
        levelName = level;
        startPosition = startPos;
        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        physicsFrameRate = (int)(1f / Time.fixedDeltaTime);
        frames.Clear();
    }
    
    /// <summary>
    /// Adds a frame of input data to the recording.
    /// </summary>
    public void AddFrame(int frameNumber, InputState input)
    {
        frames.Add(new InputFrame(frameNumber, input));
    }
    
    /// <summary>
    /// Finalizes the recording with completion time.
    /// </summary>
    public void Finalize(float time)
    {
        completionTime = time;
    }
    
    /// <summary>
    /// Gets the input state for a specific frame. Returns zero input if frame doesn't exist.
    /// </summary>
    public InputState GetInputAtFrame(int frameNumber)
    {
        if (frameNumber < 0 || frameNumber >= frames.Count)
            return InputState.Zero;
            
        return frames[frameNumber].Input;
    }
    
    /// <summary>
    /// Total number of frames in this recording.
    /// </summary>
    public int FrameCount => frames.Count;
    
    /// <summary>
    /// Validates that the recording data is consistent.
    /// </summary>
    public bool Validate()
    {
        if (frames.Count == 0)
            return false;
            
        // Check frame numbers are sequential
        for (int i = 0; i < frames.Count; i++)
        {
            if (frames[i].FrameNumber != i)
            {
                Debug.LogWarning($"Frame number mismatch at index {i}: expected {i}, got {frames[i].FrameNumber}");
                return false;
            }
        }
        
        return true;
    }
}

