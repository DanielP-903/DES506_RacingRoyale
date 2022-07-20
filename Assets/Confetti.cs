using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Confetti : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> particleSystems;
    
    public void ActivateConfetti()
    {
        foreach (var p in particleSystems)
        {
            p.Play();
        }
    }
}
