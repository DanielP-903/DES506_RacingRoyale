using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PowerupSpawner : MonoBehaviour
{
    public float spawnDelay = 2.0f;
    public List<SO_Powerup> powerups = new List<SO_Powerup>();
    
    private float _spawnTimer = 0.0f;

    private SO_Powerup currentPowerup;

    // Start is called before the first frame update
    void Start()
    {
        _spawnTimer = spawnDelay;
        transform.GetChild(0).gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        _spawnTimer = _spawnTimer <= 0 ? 0 : _spawnTimer - Time.fixedDeltaTime;
        if (_spawnTimer <= 0)
        {
            if (!transform.GetChild(0).gameObject.activeInHierarchy)
            {
                SpawnPowerup();
            }
        }
    }

    private void SpawnPowerup()
    {
        float randomChance = Random.Range(0, 100);
        if (randomChance < 50)
        {
            currentPowerup = powerups[0];
        }
        else
        {
            currentPowerup = powerups[1];
        }
        transform.GetChild(0).gameObject.SetActive(true);
    }

    public void ResetTimer()
    {
        _spawnTimer = spawnDelay;
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public SO_Powerup GetCurrentPowerup()
    {
        return currentPowerup;
    }
}
