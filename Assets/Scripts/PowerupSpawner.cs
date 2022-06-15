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
    private GameObject _powerupCube;

    // Start is called before the first frame update
    void Start()
    {
        _spawnTimer = spawnDelay;
        _powerupCube = transform.GetChild(0).gameObject;
        _powerupCube.SetActive(false);
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
        if (randomChance < 33)
        {
            currentPowerup = powerups[0];
        }
        else if (randomChance >= 33 && randomChance < 66)
        {
            currentPowerup = powerups[1];
        }
        else if (randomChance >= 66)
        {
            currentPowerup = powerups[2];
        }
        
        // MANUAL ASSIGNATION OF POWERUP
        
        //currentPowerup = powerups[3];
        
        _powerupCube.SetActive(true);
    }

    public void ResetTimer()
    {
        _spawnTimer = spawnDelay;
        _powerupCube.SetActive(false);
    }

    public SO_Powerup GetCurrentPowerup()
    {
        return currentPowerup;
    }
}
