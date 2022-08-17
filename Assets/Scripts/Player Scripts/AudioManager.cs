using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A dictionary to represent sound effects and their names.
/// Key: String, Value: AudioSource.
/// (Serializable Dictionary package)
/// </summary>
[Serializable]
public class StringAudioDictionary : SerializableDictionary<string, AudioSource> { }

/// <summary>
/// Handles all audio within the game
/// </summary>
public class AudioManager : MonoBehaviour
{
    [SerializeField] private bool isUI;
    [SerializeField] private AudioClip engineClip;
    public new StringAudioDictionary audio;
    private AudioSource _engineSounds;
    private Rigidbody _rb;
    private StringAudioDictionary _audioStringDictionary;

    /// <summary>
    /// Setup for a string, audio dictionary
    /// </summary>
    private IDictionary<string, AudioSource> StringAudioDictionary
    {
        get => _audioStringDictionary;
        set => _audioStringDictionary.CopyFrom(value);
    }

    /// <summary>
    /// Plays a sound based on a given name
    /// </summary>
    /// <param name="soundName">Name of the sound to play as defined in the dictionary</param>
    public void PlaySound(string soundName)
    {
        if (audio.ContainsKey(soundName))
        {
            audio[soundName].Play();
        }
        else
        {
            Debug.LogError("ERROR: Cannot find sound name '" + soundName + "' in dictionary");
        }
    }

    /// <summary>
    /// Stops a sound based on a given name
    /// </summary>
    /// <param name="soundName">Name of the sound to stop as defined in the dictionary</param>
    public void StopSound(string soundName)
    {
        if (audio.ContainsKey(soundName))
        {
            audio[soundName].Stop();
        }
        else
        {
            Debug.LogError("ERROR: Cannot find sound name '" + soundName + "' in dictionary");
        }
    }

    /// <summary>
    /// Checks if a specific sound is currently playing
    /// </summary>
    /// <param name="soundName">Name of the sound to check as defined in the dictionary</param>
    /// <returns></returns>
    public bool IsPlayingSound(string soundName)
    {
        if (audio.ContainsKey(soundName))
        {
            return audio[soundName].isPlaying;
        }
        else
        {
            Debug.LogError("ERROR: Cannot find sound name '" + soundName + "' in dictionary");
        }

        return false;
    }

    /// <summary>
    /// Sets the volume of a specific sound
    /// </summary>
    /// <param name="soundName">Name of the sound to change the volume of as defined in the dictionary</param>
    /// <param name="volume">Volume to set the sound to (0.0 to 1.0)</param>
    public void SetSoundVolume(string soundName, float volume)
    {
        if (audio.ContainsKey(soundName))
        {
            audio[soundName].volume = volume;
        }
        else
        {
            Debug.LogError("ERROR: Cannot find sound name '" + soundName + "' in dictionary");
        }

    }

    void Start()
    {
        if (!isUI)
        {
            _engineSounds = GetComponent<AudioSource>();
            _rb = GetComponentInParent<Rigidbody>();
            _engineSounds.clip = engineClip;
            _engineSounds.loop = true;
            _engineSounds.Play();
        }
    }

    void Update()
    {
        if (!isUI)
        {
            // Engine sound loop volume and pitch alterations based on speed
            Vector2 horizontalSpeed = new Vector2(_rb.velocity.x, _rb.velocity.z);
            _engineSounds.volume = Mathf.Min(horizontalSpeed.magnitude, 5f) / 9;
            _engineSounds.pitch = Mathf.Max(Mathf.Min(horizontalSpeed.magnitude, 50) / 50, 1);
        }
    }
}