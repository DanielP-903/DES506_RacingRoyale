using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPowerups : MonoBehaviour
{
    [Header("Super Boost")]
    [Tooltip("Amount of force given to the player when activating Super Boost")]
    public float superBoostForce = 50.0f;

    [Header("Air Blast")] 
    [Tooltip("Radius area of effect for the Air Blast")]
    public float airBlastRadius = 10.0f;
    [Tooltip("Force applied to players who collide with the blast")]
    public float airBlastForce = 10.0f;
    [Tooltip("Time given for the Air Blast area of effect to expand")]
    public float airBlastTime = 2.0f;
    [Tooltip("Air Blast VFX")]
    public List<ParticleSystem> airBlastEffects;

    [Header("Grapple Hook")] 
    [Tooltip("Max time allowed before grapple disconnects")]
    public float grappleTime = 2.0f;
    [Tooltip("Force applied to the player when grappling towards the target")]
    public float grappleForce = 10.0f;
    [Tooltip("Maximum distance to look for targets")]
    public float achievableDistance = 30.0f;
    [Tooltip("Minimum distance hook is effective")]
    public float grappleThreshold = 1.0f;
    
    [Header("Punching Glove")] 
    [Tooltip("Max time allowed before punching glove disconnects")]
    public float punchTime = 2.0f;
    [Tooltip("Force applied to the target when they are hit with the glove")]
    public float punchingForce = 10.0f;
    [Tooltip("Maximum distance to look for targets")]
    public float achievablePunchRange = 30.0f;
    [Tooltip("Radius area of effect for targets")]
    public float detectionRadius = 5.0f;
    
    
    [Header("OBSOLETE POWERUPS")]
    public float wallShieldTime = 15.0f;
    public float wallShieldBounciness = 10.0f;
    public float warpPortalTime = 15.0f;

    [Header("Other")]
    [Tooltip("UI icon for powerups")]
    public Image powerupIcon;
    [Tooltip("Powerup scriptable objects")]
    public List<SO_Powerup> powerups = new List<SO_Powerup>();
  
    private GameObject _grappleLineObject;
    private GameObject _punchObject;
    private GameObject _punchGlove;
    private GameObject _blastObject;
    private GameObject _wallObject;
    private GameObject _warpObject;

    private SO_Powerup _currentPowerup;
    private PowerupType _currentPowerupType;
    private bool _airBlasting;
    private bool _boosting;
    private bool _grappling;
    private bool _punching;
    private bool _usingPowerup;
    private float _wallShieldTimer;
    private float _warpPortalTimer;
    private float _airBlastTimer;
    private float _superBoostTimer;
    private float _grappleTimer;
    private float _punchTimer;
    private Image _powerupIconMask;
    private CarController _carController;
    private CarVFXHandler _vfxHandler;
    private Rigidbody _rigidbody;
    private SphereCollider _blastObjectCollider;
    private RaycastHit _nearestHit;
    private LineRenderer _grappleLine;
    private LineRenderer _punchLine; // haha
    private AudioManager _audioManager;
    private GameObject _currentTarget;
    private PhotonView _photonView;
    
    // Start is called before the first frame update
    void Start()
    {
        _carController = GetComponent<CarController>();
        _audioManager = GetComponent<CarController>().audioManager;
        _vfxHandler = GetComponent<CarVFXHandler>();
        _rigidbody = GetComponent<Rigidbody>();
        _wallObject = transform.GetChild(0).gameObject;
        _blastObject = transform.GetChild(1).gameObject;
        _blastObjectCollider = _blastObject.GetComponent<SphereCollider>();
        powerupIcon = GameObject.FindGameObjectWithTag("MainCanvas").transform.GetChild(4).GetComponent<Image>();
        _powerupIconMask = powerupIcon.transform.GetChild(0).GetComponent<Image>();
        _grappleLineObject = transform.GetChild(2).gameObject;
        _grappleLine = _grappleLineObject.GetComponent<LineRenderer>();
        _punchObject = transform.GetChild(2).gameObject;
        _punchGlove = transform.GetChild(3).gameObject;
        _punchLine = _grappleLineObject.GetComponent<LineRenderer>();
        _warpObject = transform.GetChild(4).gameObject;
        _blastObject.SetActive(false);
        _photonView = GetComponent<PhotonView>();
    }

    void OnLevelWasLoaded()
    {
        SetUp();
    }

    public void SetUp()
    {
        _carController = GetComponent<CarController>();
        _audioManager = GetComponent<CarController>().audioManager;
        _vfxHandler = GetComponent<CarVFXHandler>();
        _rigidbody = GetComponent<Rigidbody>();
        _wallObject = transform.GetChild(0).gameObject;
        _blastObject = transform.GetChild(1).gameObject;
        _blastObjectCollider = _blastObject.GetComponent<SphereCollider>();
        powerupIcon = GameObject.FindGameObjectWithTag("MainCanvas").transform.GetChild(4).GetComponent<Image>();
        _powerupIconMask = powerupIcon.transform.GetChild(0).GetComponent<Image>();
        _grappleLineObject = transform.GetChild(2).gameObject;
        _grappleLine = _grappleLineObject.GetComponent<LineRenderer>();
        _punchObject = transform.GetChild(2).gameObject;
        _punchGlove = transform.GetChild(3).gameObject;
        _punchLine = _grappleLineObject.GetComponent<LineRenderer>();
        _warpObject = transform.GetChild(4).gameObject;
        _blastObject.SetActive(false);

        //Debug.Log("Blast object is active? " + blastObject.activeInHierarchy);
    }

    void FixedUpdate()
    {
        if (_carController.bot) return;
        
        PhysUpdatePowerups();
    }
    
    private void PhysUpdatePowerups()
    {
        _wallShieldTimer = _wallShieldTimer <= 0 ? 0 : _wallShieldTimer - Time.fixedDeltaTime;
        _airBlastTimer = _airBlastTimer <= 0 ? 0 : _airBlastTimer - Time.fixedDeltaTime;
        _superBoostTimer = _superBoostTimer <= 0 ? 0 : _superBoostTimer - Time.fixedDeltaTime;
        _grappleTimer = _grappleTimer <= 0 ? 0 : _grappleTimer - Time.fixedDeltaTime;
        _punchTimer = _punchTimer <= 0 ? 0 : _punchTimer - Time.fixedDeltaTime;
        _warpPortalTimer = _warpPortalTimer <= 0 ? 0 : _warpPortalTimer - Time.fixedDeltaTime;

        if (_punching && _nearestHit.transform != null)
        {
            _powerupIconMask.fillAmount = (punchTime - _punchTimer)/punchTime;
            _punchLine.startColor = Color.green;
            _punchLine.endColor = Color.red;
            
            // Draw line between them
            Vector3[] positions = new Vector3[2];
            positions[0] = transform.position;
            positions[1] = Vector3.Lerp(transform.position + transform.forward,  _nearestHit.transform.position, (punchTime - _punchTimer) / punchTime );
            
            _punchLine.SetPositions(positions);
            _punchGlove.transform.position = positions[1];

            _photonView.RPC("UpdatePunchingGlove", RpcTarget.All, _photonView.ViewID, positions);
        }

        if (_currentPowerupType == PowerupType.PunchingGlove || _currentPowerupType == PowerupType.GrapplingHook)
        {
            // Detect opponent to grapple to
            RaycastHit[] hits;
            if (_currentPowerupType == PowerupType.PunchingGlove)
                hits = Physics.SphereCastAll(transform.position, detectionRadius, transform.forward,  achievablePunchRange);
            else
                hits = Physics.SphereCastAll(transform.position, detectionRadius, transform.forward,  achievableDistance);
            
            if (hits.Length > 0)
            {
                float distance = 1000000.0f;
                _nearestHit = hits[0];
                foreach (var hit in hits)
                {
                    if (hit.distance < distance && hit.transform.CompareTag("Player") && hit.transform.gameObject != transform.gameObject)
                    {
                        distance = hit.distance;
                        _nearestHit = hit;
                    }
                }

                if (_nearestHit.transform.CompareTag("Player") && _nearestHit.transform.gameObject != transform.gameObject)
                {
                    if (_currentTarget)
                    {
                        if (_currentPowerupType == PowerupType.PunchingGlove)
                            _currentTarget.GetComponent<CarVFXHandler>().SetOutlineActive(false);
                        else
                            _currentTarget.GetComponent<CarVFXHandler>().SetGrappleOutlineActive(false);
                    }

                    _currentTarget = _nearestHit.transform.gameObject;
                    if (_currentPowerupType == PowerupType.PunchingGlove)
                        _currentTarget.GetComponent<CarVFXHandler>().SetOutlineActive(true);
                    else
                        _currentTarget.GetComponent<CarVFXHandler>().SetGrappleOutlineActive(true);
                    Debug.Log("HIT!!!");
                }
                else
                {
                    if (_currentTarget)
                    {
                        if (_currentPowerupType == PowerupType.PunchingGlove)
                            _currentTarget.GetComponent<CarVFXHandler>().SetOutlineActive(false);
                        else
                            _currentTarget.GetComponent<CarVFXHandler>().SetGrappleOutlineActive(false);

                        _currentTarget = null;
                    }

                    Debug.Log("No hit!");
                }
            }
            else
            {
                if (_currentTarget)
                {
                    if (_currentPowerupType == PowerupType.PunchingGlove)
                        _currentTarget.GetComponent<CarVFXHandler>().SetOutlineActive(false);
                    else
                        _currentTarget.GetComponent<CarVFXHandler>().SetGrappleOutlineActive(false);
                    _currentTarget = null;
                }
                Debug.Log("No hit!");
            }
        }
        else
        {
            if (_currentTarget)
            {
                if (_currentPowerupType == PowerupType.PunchingGlove)
                    _currentTarget.GetComponent<CarVFXHandler>().SetOutlineActive(false);
                else
                    _currentTarget.GetComponent<CarVFXHandler>().SetGrappleOutlineActive(false);
                _currentTarget = null;
            }
        }

        if (_grappling && _nearestHit.transform != null)
        {      
            _powerupIconMask.fillAmount = (grappleTime - _grappleTimer)/grappleTime;

            _grappleLine.startColor = Color.green;
            _grappleLine.endColor = Color.red;
            
            // Draw line between them
            Vector3[] positions = new Vector3[2];
            positions[0] = transform.position;
            positions[1] = _nearestHit.transform.position;
                 
            _grappleLine.SetPositions(positions);
            _photonView.RPC("UpdateGrappleHook", RpcTarget.All, _photonView.ViewID, positions);

            // Drag player towards grappled player
            if (_rigidbody.velocity.magnitude * 2.2369362912f < 0.1f)
            {
                _rigidbody.velocity = (_nearestHit.transform.position - transform.position).normalized * grappleForce;
            }
            else
            {
                _rigidbody.AddForce((_nearestHit.transform.position - transform.position).normalized * grappleForce, ForceMode.VelocityChange);
            }
            
            if ((_nearestHit.transform.position - transform.position).magnitude < grappleThreshold)
            {
                _grappling = false;
                _grappleLineObject.SetActive(false);
                StartCoroutine(DelayRemoveIcon());
                _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.GrapplingHook, false, transform.position);

            }
        }
        
        if (_boosting)
        {
            _powerupIconMask.fillAmount = (1 - _superBoostTimer);
        }
        
        if (_wallShieldTimer > 0)
        {
            _powerupIconMask.fillAmount = (wallShieldTime - _wallShieldTimer) / wallShieldTime;
            if (!_wallObject.activeInHierarchy)
            {
                _wallObject.SetActive(true);
                _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.BouncyWallShield, true, transform.position);
            }
        }
        else
        {
            if (_wallObject.activeInHierarchy)
            {
                //_powerupIconMask.fillAmount = 0;
                _wallObject.SetActive(false);
                StartCoroutine(DelayRemoveIcon());
                _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.BouncyWallShield, false, transform.position);
            }
        }

        if (_warpPortalTimer > 0)
        {
            _powerupIconMask.fillAmount = (warpPortalTime - _warpPortalTimer) / warpPortalTime;
            if (!_warpObject.activeInHierarchy)
            {
                _warpObject.SetActive(true);
                _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.WarpPortal, true, transform.position);
            }
        }
        else
        {
            if (_warpObject.activeInHierarchy)
            {
                //_powerupIconMask.fillAmount = 0;
                _warpObject.SetActive(false);
                StartCoroutine(DelayRemoveIcon());
                _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.WarpPortal, false, transform.position);
            }
        }

        if (_airBlasting)
        {
            _powerupIconMask.fillAmount = (airBlastTime - _airBlastTimer) / airBlastTime;
            _blastObjectCollider.radius = Mathf.Lerp(_blastObjectCollider.radius, airBlastRadius, Time.deltaTime);
            _photonView.RPC("UpdateAirBlast", RpcTarget.All, _photonView.ViewID,  airBlastRadius);
            if (_airBlastTimer <= 0)
            {
                //_powerupIconMask.fillAmount = 0;
                _blastObject.SetActive(false);
                _blastObjectCollider.radius = 2;
                _airBlasting = false;
                _airBlastTimer = 0;
                StartCoroutine(DelayRemoveIcon());
                _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.AirBlast, false);
            }
        }

        if (!_punching && _punchGlove.activeInHierarchy)
        {
            _punchGlove.SetActive(false);
        }
        
        if (_carController.GetActivate() && !_usingPowerup)
        {
            switch (_currentPowerupType)
            {
                case PowerupType.None: break; //Debug.Log("No powerup equipped!"); break;
                case PowerupType.Superboost: SuperBoost(); _usingPowerup =true; break;
                case PowerupType.BouncyWallShield: BouncyWallShield(); _usingPowerup =true; break;
                case PowerupType.AirBlast: AirBlast(); _usingPowerup =true; break;
                case PowerupType.GrapplingHook: GrapplingHook(); _usingPowerup =true; break;
                case PowerupType.PunchingGlove: PunchingGlove(); _usingPowerup =true; break;
                case PowerupType.WarpPortal: WarpPortal(); _usingPowerup =true; break;
                default: throw new ArgumentOutOfRangeException();
            }

            if (_wallShieldTimer < wallShieldTime - 1.0f)
            {
                _wallShieldTimer = 0.0f;
            }
        }
        
        
    }

     #region Powerup_Functionality

     private void SuperBoost()
     {
         StartCoroutine(DelayRemoveIconBoost());
         _vfxHandler.PlayBoostEffectAlt();
         _vfxHandler.PlayVFX("BoostEffect");
         if (_rigidbody.velocity.magnitude * 2.2369362912f < 0.1f)
         {                
             _rigidbody.velocity = transform.forward * superBoostForce;
         }
         else
         {
             _rigidbody.AddForce(transform.forward * superBoostForce, ForceMode.VelocityChange);
         }
         _audioManager.PlaySound("SuperBoostLoop");
     }

     private void BouncyWallShield()
     {
         _wallShieldTimer = wallShieldTime;
         _audioManager.PlaySound("BouncyWallShield");
     }
     
     private void WarpPortal()
     {
         _warpPortalTimer = warpPortalTime;
         _audioManager.PlaySound("WarpPortal");
     }

 
     
     private void PunchingGlove()
     {
         // Detect opponent to grapple to
         RaycastHit[] hits;
         hits = Physics.SphereCastAll(transform.position, detectionRadius, transform.forward,  achievablePunchRange);
         if (hits.Length > 0)
         {
             _carController.audioManager.PlaySound("PunchingGloveLaunch");
             float distance = 1000000.0f;
             _nearestHit = hits[0];
             foreach (var hit in hits)
             {
                 if (hit.distance < distance && hit.transform.CompareTag("Player") && hit.transform.gameObject != transform.gameObject)
                 {
                     distance = hit.distance;
                     _nearestHit = hit;
                 }
             }

             if (_nearestHit.transform.CompareTag("Player") && _nearestHit.transform.gameObject != transform.gameObject)
             {
                 // Found a player to punch!

                 _punchObject.transform.position += transform.forward;
                 _punchGlove.transform.position += transform.forward;
                 _punchObject.SetActive(true);
                 _punchGlove.SetActive(true);
                 _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.PunchingGlove, true);

                 StartCoroutine(Punch());
                 //currentPowerupType = PowerupType.None;
                 _punchTimer = punchTime;
                 Debug.Log("HIT!!!");
             }
             else
             {
                 Debug.Log("No hit!");
                 _punchObject.SetActive(false);
                 _punchGlove.SetActive(false);
                 _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.PunchingGlove, false);
             }
         }
         else
         {
             Debug.Log("No hit!");
             _punchObject.SetActive(false);
             _punchGlove.SetActive(false);
             _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.PunchingGlove, false);
         }
     }

     private void GrapplingHook()
     {
         // Detect opponent to grapple to
         RaycastHit[] hits;
         hits = Physics.SphereCastAll(transform.position, 5, transform.forward,  achievableDistance);
         if (hits.Length > 0)
         {
             _carController.audioManager.PlaySound("GrapplingHookLaunch");
             float distance = 1000000.0f;
             _nearestHit = hits[0];
             foreach (var hit in hits)
             {
                 if (hit.distance < distance && hit.transform.CompareTag("Player") && hit.transform.gameObject != transform.gameObject)
                 {
                     distance = hit.distance;
                     _nearestHit = hit;
                 }
             }

             if (_nearestHit.transform.CompareTag("Player") && _nearestHit.transform.gameObject != transform.gameObject)
             {
                 // Found a player to grapple!
                 _grappleLineObject.SetActive(true);
                 StartCoroutine(Grapple());
                 Debug.Log("HIT!!!");
                 _audioManager.PlaySound("GrapplingHook");
                 _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.GrapplingHook, true);
                 _carController.audioManager.PlaySound("GrapplingHook");
             }
             else
             {
                 Debug.Log("No hit!");
                 _grappleLineObject.SetActive(false);
                 _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.GrapplingHook, false);
             }
         }
         else
         {
             Debug.Log("No hit!");
             _grappleLineObject.SetActive(false);
             _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.GrapplingHook, false);
         }
     }

     private IEnumerator Grapple()
     {
         // Apply a constant acceleration force towards the player for a limited time
         _grappling = true;
         yield return new WaitForSeconds(grappleTime);
         if (_grappling)
         {
            _grappling = false;
            _grappleLineObject.SetActive(false);
            StartCoroutine(DelayRemoveIcon());
            _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.GrapplingHook, false);
         }
     }
     private IEnumerator Punch()
     {            
         // Reset line
         Vector3[] positions = new Vector3[2];
         positions[0] = transform.position + transform.forward;
         positions[1] = transform.position + transform.forward;
            
         _punchLine.SetPositions(positions);
         _punchGlove.transform.position = positions[1];
         _photonView.RPC("UpdatePunchingGlove", RpcTarget.All, _photonView.ViewID, positions);

         _punching = true;
         yield return new WaitForSeconds(3.0f);
         if (_punching)
         {    
             // Reset line between them
             Vector3[] positions2 = new Vector3[2];
             positions2[0] = transform.position + transform.forward;
             positions2[1] = transform.position + transform.forward;
    
             _punchLine.SetPositions(positions2);
             _photonView.RPC("UpdatePunchingGlove", RpcTarget.All, _photonView.ViewID, positions2);
             
             _punchGlove.transform.position = positions2[1];
             //_photonView.RPC("ResetPunchingGlove", RpcTarget.All, punchGlove, positions2[1]);

             _punching = false;
             _punchObject.SetActive(false);
             _punchGlove.SetActive(false);
             _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.PunchingGlove, false);
             StartCoroutine(DelayRemoveIcon());
         }
     }

     public void ResetPunch()
     {
         // Reset line between them
         Vector3[] positions2 = new Vector3[2];
         positions2[0] = transform.position + transform.forward;
         positions2[1] = transform.position + transform.forward;
 
         _punchLine.SetPositions(positions2);
         _photonView.RPC("UpdatePunchingGlove", RpcTarget.All, _photonView.ViewID, positions2);
         
         _punchGlove.transform.position = positions2[1];
         //_photonView.RPC("ResetPunchingGlove", RpcTarget.All, punchGlove, positions2[1]);

         _punching = false;
         _punchObject.SetActive(false);
         _punchGlove.SetActive(false);
         _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.PunchingGlove, false);
         StartCoroutine(DelayRemoveIcon());
     }
     
     private void AirBlast()
     {
         _blastObject.SetActive(true);
         _blastObjectCollider.radius = 2;
         _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.AirBlast, true);
         _airBlasting = true;
         _airBlastTimer = airBlastTime;
         foreach (var effect in airBlastEffects)
         {
             effect.Play();
         }
         _audioManager.PlaySound("AirBlast");
     }
     
     #endregion

     private IEnumerator DelayRemoveIconBoost()
     {
         _superBoostTimer = 1;
         _boosting = true;
         yield return new WaitForSeconds(3);
         StartCoroutine(DelayRemoveIcon());
         _vfxHandler.StopBoostEffect();
         _boosting = false;
     }

     private IEnumerator DelayRemoveIcon()
     {
         powerupIcon.gameObject.GetComponent<Animator>().Play("PowerupPopOut");
         yield return new WaitForSeconds(0.25f);
         powerupIcon.gameObject.SetActive(false);
         _powerupIconMask.fillAmount = 0;
         _currentPowerupType = PowerupType.None;
         _usingPowerup = false;
     }

     public void DebugSetCurrentPowerup(PowerupType powerupType)
     {
         switch (powerupType)
         {
             case PowerupType.None:
                 break;
             case PowerupType.Superboost:
                 _currentPowerup = powerups[0];
                 break;
             case PowerupType.BouncyWallShield:
                 _currentPowerup = powerups[1];
                 break;
             case PowerupType.AirBlast:
                 _currentPowerup = powerups[2];
                 break;
             case PowerupType.GrapplingHook:
                 _currentPowerup = powerups[3];
                 break;
             case PowerupType.PunchingGlove:
                 _currentPowerup = powerups[4];
                 break;
             case PowerupType.WarpPortal:
                 _currentPowerup = powerups[5];
                 break;
             default:
                 throw new ArgumentOutOfRangeException(nameof(powerupType), powerupType, null);
         }
         _currentPowerupType = _currentPowerup.powerupType;
         powerupIcon.sprite = _currentPowerup.powerupUIImage;
         _powerupIconMask.sprite = _currentPowerup.powerupUIImage;
         powerupIcon.gameObject.SetActive(true);
         _powerupIconMask.fillAmount = 0;
         _vfxHandler.PlayVFXAtPosition("ItemBoxImpact", transform.position);
         _audioManager.PlaySound("PowerUpCollected");
     }
     
     private void OnTriggerEnter(Collider collider)
     {
         if (collider.transform.CompareTag("Powerup") && _warpPortalTimer <= 0 && _wallShieldTimer <= 0 && !_boosting && !_grappling && !_punching && !_airBlasting && !_carController.bot)
         {
             _currentPowerup = collider.transform.parent.GetComponent<PowerupSpawner>().GetCurrentPowerup();
             _currentPowerupType = _currentPowerup.powerupType;
             collider.transform.parent.GetComponent<PowerupSpawner>().ResetTimer();
             powerupIcon.sprite = _currentPowerup.powerupUIImage;
             _powerupIconMask.sprite = _currentPowerup.powerupUIImage;
             powerupIcon.gameObject.SetActive(true);
             _powerupIconMask.fillAmount = 0;
             powerupIcon.gameObject.GetComponent<Animator>().Play("PowerupPopIn");
             _vfxHandler.SpawnVFXAtPosition("ItemBoxDisappear", collider.transform.position, 0.5f,false);
             _vfxHandler.PlayVFXAtPosition("ItemBoxImpact", transform.position);
             _audioManager.PlaySound("PowerUpCollected");
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
             _audioManager.PlaySound("BouncyWallShield");
         }
         
         if (collider.transform.CompareTag("Blast") && collider.transform.parent.gameObject != transform.gameObject)
         {
             Vector3 direction = collider.transform.position - transform.position;
             _rigidbody.velocity = -(direction.normalized * airBlastForce);
             _audioManager.PlaySound("AirBlast");
         }
         
         if (collider.transform.CompareTag("PunchingGlove") && collider.transform.parent.gameObject != transform.gameObject)
         {
             Vector3 direction = collider.transform.position - transform.position;
             _rigidbody.velocity = -(direction.normalized * punchingForce); //Time.fixedDeltaTime * 50;
             //_rigidbody.AddForce(-(direction.normalized * punchingForce)); 
             Debug.Log("Ouch!! Hit with velocity: " + _rigidbody.velocity);
             //collider.transform.parent.gameObject.GetComponent<PlayerPowerups>().DetectPunch();
             _photonView.RPC("PunchingGloveHit", RpcTarget.All, collider.transform.parent.gameObject.GetComponent<PhotonView>().ViewID,
                 collider.transform.position);
             _photonView.RPC("Powerup", RpcTarget.All, collider.transform.parent.gameObject.GetComponent<PhotonView>().ViewID,
                 PowerupType.PunchingGlove, false);
             _vfxHandler.SpawnVFXAtPosition("PunchImpact", transform.position, 1,true);
             _audioManager.PlaySound("PunchingGlove");
         }
         
           
         if (collider.transform.CompareTag("WarpPortal"))
         {
             _vfxHandler.PlayVFX("PortalEffect");
             _carController.ResetPlayer();
             _audioManager.PlaySound("WarpPortal");
         }
     }
}