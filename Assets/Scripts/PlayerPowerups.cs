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
    public List<ParticleSystem> airBlastEffects;

    [Header("Grapple Hook")] 
    public float grappleTime = 2.0f;
    public float grappleForce = 10.0f;
    public float achievableDistance = 30.0f;
    
    [Header("Other")]
    public Image powerupIcon;
    public GameObject _blastObject; 

    
    public SO_Powerup _currentPowerup;
    public PowerupType _currentPowerupType;    
    
    private bool _airBlasting = false;
    private bool _boosting = false;
    private float _wallShieldTimer = 0.0f;
    private float _airBlastTimer = 0.0f;
    private float _superBoostTimer = 0.0f;
    private float _grappleTimer = 0.0f;
    private Image _powerupIconMask;
    private CarController _carController;
    private Rigidbody _rigidbody;
    private SphereCollider _blastObjectCollider;
    private RaycastHit nearestHit;

    // Start is called before the first frame update
    void Start()
    {
        _carController = GetComponent<CarController>();
        _rigidbody = GetComponent<Rigidbody>();
        _blastObject = transform.GetChild(1).gameObject;//GameObject.Find("Air Blast Object");
        _blastObjectCollider = _blastObject.GetComponent<SphereCollider>();
        _powerupIconMask = powerupIcon.transform.GetChild(0).GetComponent<Image>();
    }

    void FixedUpdate()
    {
        PhysUpdatePowerups();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        if (nearestHit.transform.CompareTag("Player"))
        {
            Gizmos.DrawLine(transform.position, nearestHit.transform.position);
        }
    }

    private void PhysUpdatePowerups()
    {
        _wallShieldTimer = _wallShieldTimer <= 0 ? 0 : _wallShieldTimer - Time.fixedDeltaTime;
        _airBlastTimer = _airBlastTimer <= 0 ? 0 : _airBlastTimer - Time.fixedDeltaTime;
        _superBoostTimer = _superBoostTimer <= 0 ? 0 : _superBoostTimer - Time.fixedDeltaTime;
        //Debug.Log("Shield Timer: " + _wallShieldTimer);

        if (_boosting)
        {
            _powerupIconMask.fillAmount = (1 - _superBoostTimer);
        }
        
        if (_wallShieldTimer > 0)
        {
            _powerupIconMask.fillAmount = (wallShieldTime - _wallShieldTimer) / wallShieldTime;
            if (!transform.GetChild(0).gameObject.activeInHierarchy)
            {
                transform.GetChild(0).gameObject.SetActive(true);
            }
        }
        else
        {
            if (transform.GetChild(0).gameObject.activeInHierarchy)
            {
                _powerupIconMask.fillAmount = 0;
                transform.GetChild(0).gameObject.SetActive(false);
                powerupIcon.gameObject.SetActive(false);
            }
        }

        if (_airBlasting)
        {
            _powerupIconMask.fillAmount = (airBlastTime - _airBlastTimer) / airBlastTime;
            _blastObjectCollider.radius = Mathf.Lerp(_blastObjectCollider.radius, airBlastRadius, Time.deltaTime);
            if (_airBlastTimer <= 0)
            {
                _powerupIconMask.fillAmount = 0;
                _blastObject.SetActive(false);
                _blastObjectCollider.radius = 2;
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
         // Detect opponent to grapple to
         RaycastHit[] hits;
         hits = Physics.SphereCastAll(transform.position, 5, transform.forward,  achievableDistance);
         if (hits.Length > 0)
         {
             float distance = 1000000.0f;
             nearestHit = hits[0];
             foreach (var hit in hits)
             {
                 if (hit.distance < distance && hit.transform.CompareTag("Player") && hit.transform.gameObject != transform.gameObject)
                 {
                     distance = hit.distance;
                     nearestHit = hit;
                 }
             }

             if (nearestHit.transform.CompareTag("Player") && nearestHit.transform.gameObject != transform.gameObject)
             {
                 // Found a player to grapple!
                 
  
                 //_currentPowerupType = PowerupType.None;
                 Debug.Log("HIT!!!");
             }
         }
         else
         {
             Debug.Log("No hit!");
         }
         
         // Draw line between them
         
         
         // Apply a constant acceleration force towards the player for a limited time
         
         
     }
     
     private void AirBlast()
     {
        _blastObject.SetActive(true);
        _blastObjectCollider.radius = 2;
         _airBlasting = true;
         _airBlastTimer = airBlastTime;
         _currentPowerupType = PowerupType.None;
         foreach (var effect in airBlastEffects)
         {
             effect.Play();
         }
     }
     
     #endregion
     
     
     private IEnumerator DelayRemoveIcon()
     {
         _superBoostTimer = 1;
         _boosting = true;
         yield return new WaitForSeconds(1);
         powerupIcon.gameObject.SetActive(false);
         foreach (var effect in _carController.boostEffects)
         {
             effect.Stop();
         }
         
         _powerupIconMask.fillAmount = 0;

         _boosting = false;
     }

     private void OnTriggerEnter(Collider collider)
     {
         if (collider.transform.CompareTag("Powerup") && _airBlastTimer <= 0 && _wallShieldTimer <= 0)
         {
             _currentPowerup = collider.transform.parent.GetComponent<PowerupSpawner>().GetCurrentPowerup();
             _currentPowerupType = _currentPowerup.powerupType;
             collider.transform.parent.GetComponent<PowerupSpawner>().ResetTimer();
             powerupIcon.sprite = _currentPowerup.powerupUIImage;
             _powerupIconMask.sprite = _currentPowerup.powerupUIImage;
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

     private void OnTriggerStay(Collider collider)
     {
         if (collider.transform.CompareTag("Blast") && collider.transform.parent != transform)
         {
             Vector3 direction = collider.transform.position - transform.position;
             _rigidbody.velocity = -(direction.normalized * airBlastForce);
         }
     }
}
