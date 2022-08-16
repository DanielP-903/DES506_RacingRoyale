using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Powerup spawning functionality
/// </summary>
public class PowerupSpawner : MonoBehaviour
{
    [SerializeField] private float spawnDelay = 2.0f;
    [SerializeField] private List<SO_Powerup> powerups = new List<SO_Powerup>();
    private float _spawnTimer;
    private SO_Powerup _currentPowerup;
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

    /// <summary>
    /// Spawn a powerup and assign a random powerup type
    /// </summary>
    private void SpawnPowerup()
    {
        float randomChance = Random.Range(0, 100);
        if (randomChance < 25)
        {
            _currentPowerup = powerups[0];
        }
        else if (randomChance >= 25 && randomChance < 50)
        {
            _currentPowerup = powerups[1];
        }
        else if (randomChance >= 50 && randomChance < 75)
        {
            _currentPowerup = powerups[2];
        }
        else if (randomChance >= 75)
        {
            _currentPowerup = powerups[3];
        }

        _powerupCube.SetActive(true);
    }

    /// <summary>
    /// Reset the spawning timer delay on the powerup object
    /// </summary>
    public void ResetTimer()
    {
        _spawnTimer = spawnDelay;
        _powerupCube.SetActive(false);
    }
    
    /// <summary>
    /// Retrieves the current powerup
    /// </summary>
    /// <returns>A powerup scriptable object</returns>
    public SO_Powerup GetCurrentPowerup()
    {
        return _currentPowerup;
    }
}