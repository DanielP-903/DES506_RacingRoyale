using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;

[Serializable]
public class StringAudioDictionary : SerializableDictionary<string, AudioSource> { }

public class AudioManager : MonoBehaviour
{
    private AudioSource _engineSounds;
    private Rigidbody _rb;
    
    [SerializeField]
    private AudioClip engineClip;

    [SerializeField]
    public new StringAudioDictionary audio;

    private StringAudioDictionary _audioStringDictionary;

    private IDictionary<string,AudioSource> StringAudioDictionary
    {
        get => _audioStringDictionary;
        set => _audioStringDictionary.CopyFrom(value);
    }

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
        _engineSounds = GetComponent<AudioSource>();
        _rb = GetComponentInParent<Rigidbody>();
        _engineSounds.clip = engineClip;
        _engineSounds.loop = true;
        _engineSounds.Play();
    }

    void Update()
    {
        Vector2 horizontalSpeed = new Vector2(_rb.velocity.x, _rb.velocity.z);
        _engineSounds.volume = Mathf.Min(horizontalSpeed.magnitude, 5f)/9;
        _engineSounds.pitch = Mathf.Max(Mathf.Min(horizontalSpeed.magnitude, 50) / 50, 1);
    }
}
