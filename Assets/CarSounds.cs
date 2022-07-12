using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSounds : MonoBehaviour
{
    private AudioSource engineSounds;
    private AudioSource powerUpSounds;
    private Rigidbody rb;
    [SerializeField]
    private AudioClip engineClip;
    [SerializeField]
    private AudioClip[] powerUpClip;
    
    // Start is called before the first frame update
    void Start()
    {
        engineSounds = GetComponents<AudioSource>()[0];
        powerUpSounds = GetComponents<AudioSource>()[1];
        rb = GetComponent<Rigidbody>();
        engineSounds.clip = engineClip;
        engineSounds.loop = true;
        engineSounds.Play();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 horizontalSpeed = new Vector2(rb.velocity.x, rb.velocity.z);
        engineSounds.volume = Mathf.Min(horizontalSpeed.magnitude, 5f)/5;
        engineSounds.pitch = Mathf.Max(Mathf.Min(horizontalSpeed.magnitude, 50) / 50, 1);
    }
}
