using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            transform.parent.GetComponent<PowerupSpawner>().ResetTimer();
        }
    }
}
