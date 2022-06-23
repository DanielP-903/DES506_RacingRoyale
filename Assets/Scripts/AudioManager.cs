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
    [SerializeField]
    public StringAudioDictionary audio;
    StringAudioDictionary audioStringDictionary;
    public IDictionary<string,AudioSource> StringAudioDictionary
    {
        get => audioStringDictionary;
        set => audioStringDictionary.CopyFrom(value);
    }

    private void PlaySound(string soundName)
    {
        if (StringAudioDictionary.ContainsKey(name))
        {
        }
        else
        {
            Debug.LogError("ERROR: Cannot find sound name '" + soundName + "' in dictionary");
        }

    }
}
