using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;
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
    
    
    [Header("OBSELETE POWERUPS")]
    public float wallShieldTime = 15.0f;
    public float wallShieldBounciness = 10.0f;
    public float warpPortalTime = 15.0f;

    [Header("Other")]
    [Tooltip("UI icon for powerups")]
    public Image powerupIcon;
    [Tooltip("Powerup scriptable objects")]
    public List<SO_Powerup> powerups = new List<SO_Powerup>();
  
    private GameObject grappleLineObject;
    private GameObject punchObject;
    private GameObject punchGlove;
    private GameObject blastObject;
    private GameObject wallObject;
    private GameObject warpObject;

    private SO_Powerup currentPowerup;
    private PowerupType currentPowerupType;
    private bool _airBlasting = false;
    private bool _boosting = false;
    private bool _grappling = false;
    private bool _punching = false;
    private float _wallShieldTimer = 0.0f;
    private float _warpPortalTimer = 0.0f;
    private float _airBlastTimer = 0.0f;
    private float _superBoostTimer = 0.0f;
    private float _grappleTimer = 0.0f;
    private float _punchTimer = 0.0f;
    private Image _powerupIconMask;
    private CarController _carController;
    private CarVFXHandler _vfxHandler;
    private Rigidbody _rigidbody;
    private SphereCollider _blastObjectCollider;
    private RaycastHit _nearestHit;
    private LineRenderer _grappleLine;
    private LineRenderer _punchLine; // haha
    private AudioManager _audioManager;
    //private GameManager _gm;
    private GameObject _currentTarget;
    private PhotonView _photonView;
    
    // Start is called before the first frame update
    void Start()
    {
        _carController = GetComponent<CarController>();
        _audioManager = GetComponent<CarController>().audioManager;
        _vfxHandler = GetComponent<CarVFXHandler>();
        _rigidbody = GetComponent<Rigidbody>();
        wallObject = transform.GetChild(0).gameObject;
        blastObject = transform.GetChild(1).gameObject;
        _blastObjectCollider = blastObject.GetComponent<SphereCollider>();
        powerupIcon = GameObject.FindGameObjectWithTag("MainCanvas").transform.GetChild(4).GetComponent<Image>();
        _powerupIconMask = powerupIcon.transform.GetChild(0).GetComponent<Image>();
        grappleLineObject = transform.GetChild(2).gameObject;
        _grappleLine = grappleLineObject.GetComponent<LineRenderer>();
        punchObject = transform.GetChild(2).gameObject;
        punchGlove = transform.GetChild(3).gameObject;
        _punchLine = grappleLineObject.GetComponent<LineRenderer>();
        warpObject = transform.GetChild(4).gameObject;
        blastObject.SetActive(false);
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
        wallObject = transform.GetChild(0).gameObject;
        blastObject = transform.GetChild(1).gameObject;
        _blastObjectCollider = blastObject.GetComponent<SphereCollider>();
        powerupIcon = GameObject.FindGameObjectWithTag("MainCanvas").transform.GetChild(4).GetComponent<Image>();
        _powerupIconMask = powerupIcon.transform.GetChild(0).GetComponent<Image>();
        grappleLineObject = transform.GetChild(2).gameObject;
        _grappleLine = grappleLineObject.GetComponent<LineRenderer>();
        punchObject = transform.GetChild(2).gameObject;
        punchGlove = transform.GetChild(3).gameObject;
        _punchLine = grappleLineObject.GetComponent<LineRenderer>();
        warpObject = transform.GetChild(4).gameObject;
        blastObject.SetActive(false);

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
            punchGlove.transform.position = positions[1];

            _photonView.RPC("UpdatePunchingGlove", RpcTarget.All, _photonView.ViewID, positions);
        }

        if (currentPowerupType == PowerupType.PunchingGlove || currentPowerupType == PowerupType.GrapplingHook)
        {
            // Detect opponent to grapple to
            RaycastHit[] hits;
            if (currentPowerupType == PowerupType.PunchingGlove)
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
                        if (currentPowerupType == PowerupType.PunchingGlove)
                            _currentTarget.GetComponent<CarVFXHandler>().SetOutlineActive(false);
                        else
                            _currentTarget.GetComponent<CarVFXHandler>().SetGrappleOutlineActive(false);
                    }

                    _currentTarget = _nearestHit.transform.gameObject;
                    if (currentPowerupType == PowerupType.PunchingGlove)
                        _currentTarget.GetComponent<CarVFXHandler>().SetOutlineActive(true);
                    else
                        _currentTarget.GetComponent<CarVFXHandler>().SetGrappleOutlineActive(true);
                    Debug.Log("HIT!!!");
                }
                else
                {
                    if (_currentTarget)
                    {
                        if (currentPowerupType == PowerupType.PunchingGlove)
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
                    if (currentPowerupType == PowerupType.PunchingGlove)
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
                if (currentPowerupType == PowerupType.PunchingGlove)
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
                grappleLineObject.SetActive(false);
                powerupIcon.gameObject.SetActive(false);
                _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.GrapplingHook, false);

            }
        }
        
        if (_boosting)
        {
            _powerupIconMask.fillAmount = (1 - _superBoostTimer);
        }
        
        if (_wallShieldTimer > 0)
        {
            _powerupIconMask.fillAmount = (wallShieldTime - _wallShieldTimer) / wallShieldTime;
            if (!wallObject.activeInHierarchy)
            {
                wallObject.SetActive(true);
                _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.BouncyWallShield, true);
            }
        }
        else
        {
            if (wallObject.activeInHierarchy)
            {
                _powerupIconMask.fillAmount = 0;
                wallObject.SetActive(false);
                powerupIcon.gameObject.SetActive(false);
                _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.BouncyWallShield, false);
            }
        }

        if (_warpPortalTimer > 0)
        {
            _powerupIconMask.fillAmount = (warpPortalTime - _warpPortalTimer) / warpPortalTime;
            if (!warpObject.activeInHierarchy)
            {
                warpObject.SetActive(true);
                _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.WarpPortal, true);
            }
        }
        else
        {
            if (warpObject.activeInHierarchy)
            {
                _powerupIconMask.fillAmount = 0;
                warpObject.SetActive(false);
                powerupIcon.gameObject.SetActive(false);
                _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.WarpPortal, false);
            }
        }

        if (_airBlasting)
        {
            _powerupIconMask.fillAmount = (airBlastTime - _airBlastTimer) / airBlastTime;
            _blastObjectCollider.radius = Mathf.Lerp(_blastObjectCollider.radius, airBlastRadius, Time.deltaTime);
            _photonView.RPC("UpdateAirBlast", RpcTarget.All, _photonView.ViewID,  airBlastRadius);
            if (_airBlastTimer <= 0)
            {
                _powerupIconMask.fillAmount = 0;
                blastObject.SetActive(false);
                _blastObjectCollider.radius = 2;
                _airBlasting = false;
                _airBlastTimer = 0;
                powerupIcon.gameObject.SetActive(false);
                _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.AirBlast, false);
            }
        }

        if (!_punching && punchGlove.activeInHierarchy)
        {
            punchGlove.SetActive(false);
        }
        
        if (_carController.GetActivate())
        {
            switch (currentPowerupType)
            {
                case PowerupType.None: break; //Debug.Log("No powerup equipped!"); break;
                case PowerupType.Superboost: SuperBoost(); break;
                case PowerupType.BouncyWallShield: BouncyWallShield(); break;
                case PowerupType.AirBlast: AirBlast(); break;
                case PowerupType.GrapplingHook: GrapplingHook(); break;
                case PowerupType.PunchingGlove: PunchingGlove(); break;
                case PowerupType.WarpPortal: WarpPortal(); break;
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
         StartCoroutine(DelayRemoveIcon());
         currentPowerupType = PowerupType.None;
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
         currentPowerupType = PowerupType.None;
         _audioManager.PlaySound("BouncyWallShield");
     }
     
     private void WarpPortal()
     {
         _warpPortalTimer = warpPortalTime;
         currentPowerupType = PowerupType.None;
         _audioManager.PlaySound("WarpPortal");
     }

     private void DetectPunch()
     {
         // Reset line between them
         Vector3[] positions = new Vector3[2];
         positions[0] = transform.position + transform.forward;;
         positions[1] = transform.position + transform.forward;
            
         _punchLine.SetPositions(positions);
         punchGlove.transform.position = positions[1];
         _photonView.RPC("UpdatePunchingGlove", RpcTarget.All, _photonView.ViewID, positions);

         _punching = false;
         punchObject.SetActive(false);
         punchGlove.SetActive(false);
         _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.PunchingGlove, true);

         powerupIcon.gameObject.SetActive(false);
         _powerupIconMask.fillAmount = 0;
         currentPowerupType = PowerupType.None;
         _audioManager.PlaySound("PunchingGlove");
     }
     
     private void PunchingGlove()
     {
         // Detect opponent to grapple to
         RaycastHit[] hits;
         hits = Physics.SphereCastAll(transform.position, detectionRadius, transform.forward,  achievablePunchRange);
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
                 // Found a player to punch!

                 punchObject.transform.position += transform.forward;
                 punchGlove.transform.position += transform.forward;
                 punchObject.SetActive(true);
                 punchGlove.SetActive(true);
                 _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.PunchingGlove, true);

                 StartCoroutine(Punch());
                 //currentPowerupType = PowerupType.None;
                 _punchTimer = punchTime;
                 Debug.Log("HIT!!!");
             }
             else
             {
                 Debug.Log("No hit!");
                 punchObject.SetActive(false);
                 punchGlove.SetActive(false);
                 _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.PunchingGlove, false);
             }
         }
         else
         {
             Debug.Log("No hit!");
             punchObject.SetActive(false);
             punchGlove.SetActive(false);
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
                 grappleLineObject.SetActive(true);
                 StartCoroutine(Grapple());
                 currentPowerupType = PowerupType.None;
                 Debug.Log("HIT!!!");
                 _audioManager.PlaySound("GrapplingHook");
                 _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.GrapplingHook, true);

             }
             else
             {
                 Debug.Log("No hit!");
                 grappleLineObject.SetActive(false);
                 _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.GrapplingHook, false);
             }
         }
         else
         {
             Debug.Log("No hit!");
             grappleLineObject.SetActive(false);
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
            grappleLineObject.SetActive(false);
            powerupIcon.gameObject.SetActive(false);
            _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.GrapplingHook, false);
         }
     }
     private IEnumerator Punch()
     {            
         // Reset line
         Vector3[] positions = new Vector3[2];
         positions[0] = transform.position + transform.forward;;
         positions[1] = transform.position + transform.forward;
            
         _punchLine.SetPositions(positions);
         punchGlove.transform.position = positions[1];
         _photonView.RPC("UpdatePunchingGlove", RpcTarget.All, _photonView.ViewID, positions);

         _punching = true;
         yield return new WaitForSeconds(3.0f);
         if (_punching)
         {    
             // Reset line between them
             Vector3[] positions2 = new Vector3[2];
             positions2[0] = transform.position + transform.forward;;
             positions2[1] = transform.position + transform.forward;
    
             currentPowerupType = PowerupType.None;
             _punchLine.SetPositions(positions2);
             _photonView.RPC("UpdatePunchingGlove", RpcTarget.All, _photonView.ViewID, positions2);
             
             punchGlove.transform.position = positions2[1];
             //_photonView.RPC("ResetPunchingGlove", RpcTarget.All, punchGlove, positions2[1]);

             _punching = false;
             punchObject.SetActive(false);
             punchGlove.SetActive(false);
             _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.PunchingGlove, false);
             powerupIcon.gameObject.SetActive(false);
         }
     }

     public void ResetPunch()
     {
         // Reset line between them
         Vector3[] positions2 = new Vector3[2];
         positions2[0] = transform.position + transform.forward;;
         positions2[1] = transform.position + transform.forward;
 
         currentPowerupType = PowerupType.None;
         _punchLine.SetPositions(positions2);
         _photonView.RPC("UpdatePunchingGlove", RpcTarget.All, _photonView.ViewID, positions2);
         
         punchGlove.transform.position = positions2[1];
         //_photonView.RPC("ResetPunchingGlove", RpcTarget.All, punchGlove, positions2[1]);

         _punching = false;
         punchObject.SetActive(false);
         punchGlove.SetActive(false);
         _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.PunchingGlove, false);
         powerupIcon.gameObject.SetActive(false);
     }
     
     private void AirBlast()
     {
         blastObject.SetActive(true);
         _blastObjectCollider.radius = 2;
         _photonView.RPC("Powerup", RpcTarget.All, _photonView.ViewID, PowerupType.AirBlast, true);
         _airBlasting = true;
         _airBlastTimer = airBlastTime;
         currentPowerupType = PowerupType.None;
         foreach (var effect in airBlastEffects)
         {
             effect.Play();
         }
         _audioManager.PlaySound("AirBlast");
     }
     
     #endregion

     private IEnumerator DelayRemoveIcon()
     {
         _superBoostTimer = 1;
         _boosting = true;
         yield return new WaitForSeconds(1);
         powerupIcon.gameObject.SetActive(false);
         
         _vfxHandler.StopBoostEffect();
         
         _powerupIconMask.fillAmount = 0;

         _boosting = false;
     }

     public void DebugSetCurrentPowerup(PowerupType powerupType)
     {
         switch (powerupType)
         {
             case PowerupType.None:
                 break;
             case PowerupType.Superboost:
                 currentPowerup = powerups[0];
                 break;
             case PowerupType.BouncyWallShield:
                 currentPowerup = powerups[1];
                 break;
             case PowerupType.AirBlast:
                 currentPowerup = powerups[2];
                 break;
             case PowerupType.GrapplingHook:
                 currentPowerup = powerups[3];
                 break;
             case PowerupType.PunchingGlove:
                 currentPowerup = powerups[4];
                 break;
             case PowerupType.WarpPortal:
                 currentPowerup = powerups[5];
                 break;
             default:
                 throw new ArgumentOutOfRangeException(nameof(powerupType), powerupType, null);
         }
         currentPowerupType = currentPowerup.powerupType;
         powerupIcon.sprite = currentPowerup.powerupUIImage;
         _powerupIconMask.sprite = currentPowerup.powerupUIImage;
         powerupIcon.gameObject.SetActive(true);
         _powerupIconMask.fillAmount = 0;
         _vfxHandler.PlayVFXAtPosition("ItemBoxImpact", transform.position);
         _audioManager.PlaySound("PowerUpCollected");
     }
     
     private void OnTriggerEnter(Collider collider)
     {
         if (collider.transform.CompareTag("Powerup") && _warpPortalTimer <= 0 && _wallShieldTimer <= 0 && !_boosting && !_grappling && !_punching && !_airBlasting && !_carController.bot)
         {
             currentPowerup = collider.transform.parent.GetComponent<PowerupSpawner>().GetCurrentPowerup();
             currentPowerupType = currentPowerup.powerupType;
             collider.transform.parent.GetComponent<PowerupSpawner>().ResetTimer();
             powerupIcon.sprite = currentPowerup.powerupUIImage;
             _powerupIconMask.sprite = currentPowerup.powerupUIImage;
             powerupIcon.gameObject.SetActive(true);
             _powerupIconMask.fillAmount = 0;
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
             collider.transform.parent.GetComponent<PlayerPowerups>().DetectPunch();
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