using UnityEngine;

public class PowerupCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider trigger)
    {
        if (trigger.CompareTag("Player"))
        {
            if (trigger.TryGetComponent<PlayerPowerups>(out var playerPowerups))
            {
                if (!playerPowerups.IsUsingAnyPowerup())
                    transform.parent.GetComponent<PowerupSpawner>().ResetTimer();
            }
        }
    }
}
