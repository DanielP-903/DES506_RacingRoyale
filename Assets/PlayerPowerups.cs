using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerPowerups : MonoBehaviour
{
    [Header("Powerup Properties")] 
    [Header("Super Boost")] 
    public float superBoostForce = 50.0f;
    [Header("Bouncy Wall Shield")]
    public float wallShieldTime = 15.0f;
    public float wallShieldBounciness = 10.0f;

    [Header("Air Blast")] 
    public float airBlastRadius = 10.0f;
    public float airBlastForce = 10.0f;
    public float airBlastTime = 2.0f;
    
    [Header("Other")]
    public Image powerupIcon;

    private SO_Powerup _currentPowerup;
    private PowerupType _currentPowerupType;    
    private float _wallShieldTimer = 0.0f;
    private float _airBlastTimer = 0.0f;

    private CarController _carController;
    private Rigidbody _rigidbody;

    private bool _airBlasting = false;
    private SphereCollider _blastCollider; 
    
    // Start is called before the first frame update
    void Start()
    {
        _carController = GetComponent<CarController>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        PhysUpdatePowerups();
        
    }
    
     private void PhysUpdatePowerups()
    {
        _wallShieldTimer = _wallShieldTimer <= 0 ? 0 : _wallShieldTimer - Time.fixedDeltaTime;
        _airBlastTimer = _airBlastTimer <= 0 ? 0 : _airBlastTimer - Time.fixedDeltaTime;
        //Debug.Log("Shield Timer: " + _wallShieldTimer);
        
        if (_wallShieldTimer > 0)
        {
            if (!transform.GetChild(0).gameObject.activeInHierarchy)
            {
                transform.GetChild(0).gameObject.SetActive(true);
            }
        }
        else
        {
            if (transform.GetChild(0).gameObject.activeInHierarchy)
            {
                transform.GetChild(0).gameObject.SetActive(false);
                powerupIcon.gameObject.SetActive(false);
            }
        }

        if (_airBlasting)
        {
            _blastCollider.radius = Mathf.Lerp(_blastCollider.radius, airBlastRadius, Time.deltaTime);
            if (_airBlastTimer <= 0)
            {
                 Destroy(_blastCollider);
                _airBlasting = false;
                _airBlastTimer = 0;
                powerupIcon.gameObject.SetActive(false);
            }
        }
        
        if (_carController.GetActivate())
        {
            switch (_currentPowerupType)
            {
                case PowerupType.None: Debug.Log("No powerup equipped!"); break;
                case PowerupType.Superboost: SuperBoost(); break;
                case PowerupType.BouncyWallShield: BouncyWallShield(); break;
                case PowerupType.AirBlast: AirBlast(); break;
                case PowerupType.GrapplingHook: GrapplingHook(); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
        
        
    }

     #region Powerup_Functionality

     private void SuperBoost()
     {
         StartCoroutine(DelayRemoveIcon());
         _currentPowerupType = PowerupType.None;
         foreach (var effect in _carController.boostEffects)
         {
             effect.Play();
         }
         if (_rigidbody.velocity.magnitude * 2.2369362912f < 0.1f)
         {                
             _rigidbody.velocity = transform.forward * superBoostForce;
         }
         else
         {
             _rigidbody.AddForce(transform.forward * superBoostForce, ForceMode.VelocityChange);
         }
     }

     private void BouncyWallShield()
     {
         _wallShieldTimer = wallShieldTime;
         _currentPowerupType = PowerupType.None;
     }

     private void GrapplingHook()
     {
         _currentPowerupType = PowerupType.None;
     }
     
     private void AirBlast()
     {
         _blastCollider = gameObject.AddComponent<SphereCollider>();
         _blastCollider.isTrigger = true;
         _blastCollider.transform.tag = "Blast";
         _airBlasting = true;
         _airBlastTimer = airBlastTime;
         _currentPowerupType = PowerupType.None;
     }
     
     #endregion
     
     
     private IEnumerator DelayRemoveIcon()
     {
         yield return new WaitForSeconds(1);
         powerupIcon.gameObject.SetActive(false);
         foreach (var effect in _carController.boostEffects)
         {
             effect.Stop();
         }
     }

     private void OnTriggerEnter(Collider collider)
     {
         if (collider.transform.CompareTag("Powerup"))
         {
             _currentPowerup = collider.transform.parent.GetComponent<PowerupSpawner>().GetCurrentPowerup();
             _currentPowerupType = _currentPowerup.powerupType;
             collider.transform.parent.GetComponent<PowerupSpawner>().ResetTimer();
             powerupIcon.sprite = _currentPowerup.powerupUIImage;
             powerupIcon.gameObject.SetActive(true);
         }
        
         if (collider.transform.CompareTag("WallShield"))
         {
             if (_rigidbody.velocity.magnitude * 2.2369362912f < 0.1f)
             {                
                 _rigidbody.velocity = -collider.transform.forward * wallShieldBounciness * 2;
             }
             else
             {
                 _rigidbody.AddForce(-collider.transform.forward * wallShieldBounciness * 2, ForceMode.VelocityChange);
             }        
         }
     }
}
