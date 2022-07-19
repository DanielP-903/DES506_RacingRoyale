using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.VFX;

public class CarVFXHandler : MonoBehaviour
{
    [Header("Boost VFX")]
    public List<ParticleSystem> boostEffects = new List<ParticleSystem>();
    
    [Header("Impact VFX")]
    public List<VisualEffectAsset> impactEffectAssets = new List<VisualEffectAsset>();
    public GameObject impactEffectObject;
    public GameObject impactEffectPrefab;
    
    [Header("Other")] 
    public float maxWallDistanceAlert = 30.0f;
    
    [HideInInspector] public bool boostPlaying;
    
    // Camera VFX
    private VisualEffect _speedLinesEffect;
    private VisualEffect _speedCircleEffect;
    private VisualEffect _dangerWallEffect;
    private VisualEffect _portalEffect;
    
    private GameObject _outlineObject;
    
    private CarController _carController;
    private Rigidbody _rigidbody;
    private Camera _mainCam;
    private GameObject _wall;
    private Image _dangerPressureImg;
    private Vector2 _newAlpha;
    private VisualEffect _currentEffect;

    #region VFX-Activation

    public IEnumerator ActivateBoostEffect()
    {
        foreach (var effect in boostEffects)
        {
            effect.Play();
        }

        boostPlaying = true;

        yield return new WaitForSeconds(1);

        boostPlaying = false;

        foreach (var effect in boostEffects)
        {
            effect.Stop();
        }
    }

    public void PlayBoostEffectAlt()
    {
        foreach (var effect in boostEffects)
        {
            effect.Play();
        }
    }

    public void StopBoostEffect()
    {
        foreach (var effect in boostEffects)
        {
            effect.Stop();
        }
    }

    public void PlayVFX(string vfxName)
    {
        if (_carController.bot) return;

        switch (vfxName)
        {
            case "Impact":
                _currentEffect.visualEffectAsset = impactEffectAssets[0];
                _currentEffect.Play();
                break;
            case "SoftImpact":
                _currentEffect.visualEffectAsset = impactEffectAssets[1];
                _currentEffect.Play();
                break;
            case "GroundImpact":
                _currentEffect.visualEffectAsset = impactEffectAssets[2];
                _currentEffect.Play();
                break;
            case "ItemBoxImpact":
                _currentEffect.visualEffectAsset = impactEffectAssets[3];
                _currentEffect.Play();
                break;
            case "PowerImpact":
                _currentEffect.visualEffectAsset = impactEffectAssets[4];
                _currentEffect.Play();
                break;
            case "PunchImpact":
                _currentEffect.visualEffectAsset = impactEffectAssets[5];
                _currentEffect.Play();
                break;
            case "BoostEffect":
                _speedCircleEffect.Play();
                break;
            case "DangerWallEffect":
                _dangerWallEffect.Play();
                break;
            case "SpeedLinesEffect":
                _speedLinesEffect.Play();
                break;
            case "PortalEffect":
                _portalEffect.Play();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public int GetVFXIndex(string vfxName)
    {
        switch (vfxName)
        {
            case "Impact":
                return 0;
            case "SoftImpact":
                return 1;
            case "GroundImpact":
                return 2;
            case "ItemBoxImpact":
                return 3;
            case "PowerImpact":
                return 4;
            case "PunchImpact":
                return 5;
            case "BoostEffect":
                return -1;
            case "DangerWallEffect":
                return -1;
            case "SpeedLinesEffect":
                return -1;         
            case "PortalEffect":
                return -1;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public void PlayVFXAtPosition(string vfxName, Vector3 pos)
    {
        impactEffectObject.transform.position = pos;
        PlayVFX(vfxName);
    }

    public void SpawnVFXAtPosition(string vfxName, Vector3 pos, int scaleFactor, bool isBillboard)
    {
        GameObject newSpawn = Instantiate(impactEffectPrefab, pos, Quaternion.identity);
        newSpawn.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        newSpawn.GetComponent<TemporaryEffect>().doNotDelete = false;
        newSpawn.GetComponent<TemporaryEffect>().isBillboard = isBillboard;
        newSpawn.GetComponent<VisualEffect>().visualEffectAsset = impactEffectAssets[GetVFXIndex(vfxName)]; //impactEffectAssets[2];
        //newSpawn.GetComponent<VisualEffect>().Play();
    }
    
    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        _carController = GetComponent<CarController>();
        _rigidbody = GetComponent<Rigidbody>();

        _currentEffect = impactEffectObject.GetComponent<VisualEffect>();
        

        if (_carController.debug)
        {
            _wall = GameObject.FindGameObjectWithTag("EliminationZone");
        }

        GameObject canvas = GameObject.Find("Canvas");
        _dangerPressureImg = canvas.transform.GetChild(0).GetComponent<Image>();

        var mainCamObject = GameObject.FindGameObjectWithTag("MainCamera");
        
        if (!mainCamObject) return;
        
        _mainCam = mainCamObject.GetComponent<Camera>();
        _speedLinesEffect = _mainCam.transform.GetChild(2).gameObject.GetComponent<VisualEffect>();
        _speedCircleEffect = _mainCam.transform.GetChild(3).gameObject.GetComponent<VisualEffect>();
        _dangerWallEffect = _mainCam.transform.GetChild(4).gameObject.GetComponent<VisualEffect>();
        _portalEffect = _mainCam.transform.GetChild(5).gameObject.GetComponent<VisualEffect>();
        
        _outlineObject = transform.GetChild(6).gameObject;
        _outlineObject.SetActive(false);
        
        impactEffectObject.GetComponent<VisualEffect>();
        _speedCircleEffect.Stop();
        _portalEffect.Stop();

        if (SceneManager.GetActiveScene().name == "WaitingArea")
        {
            _dangerWallEffect.SetVector2("Alpha Values", new Vector2(0,0));
        }
    }

    void OnLevelWasLoaded()
    {
        SetUp();
    }

    public void SetUp()
    {
        GameObject canvas = GameObject.Find("Canvas");

        _wall = GameObject.FindGameObjectWithTag("EliminationZone");
        _dangerPressureImg = canvas.transform.GetChild(0).GetComponent<Image>();
        _mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        _carController = GetComponent<CarController>();
        _speedLinesEffect = _mainCam.transform.GetChild(2).gameObject.GetComponent<VisualEffect>();
        _speedCircleEffect = _mainCam.transform.GetChild(3).gameObject.GetComponent<VisualEffect>();
        _dangerWallEffect = _mainCam.transform.GetChild(4).gameObject.GetComponent<VisualEffect>();
        impactEffectObject.GetComponent<VisualEffect>();
        _speedCircleEffect.Stop();
        _portalEffect.Stop();

        if (SceneManager.GetActiveScene().name == "WaitingArea")
        {
            _dangerWallEffect.SetVector2("Alpha Values", new Vector2(0,0));
        }

        if (SceneManager.GetActiveScene().name == "EndStage")
        {
            _dangerWallEffect.SetVector2("Alpha Values", new Vector2(0,0));
            _speedCircleEffect.Stop();
            _speedLinesEffect.Stop();
            _dangerWallEffect.Stop(); // Note this might stop it from working entirely
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!_wall) return;
        if (_carController && _carController.bot) return;


        float distanceToWall = Vector3.Distance(transform.position, _wall.transform.position);
        distanceToWall = Mathf.Clamp(distanceToWall, 0, maxWallDistanceAlert);
        _dangerPressureImg.color = Color.Lerp(Color.clear, Color.magenta, (maxWallDistanceAlert - distanceToWall) / maxWallDistanceAlert);
        Vector2 newAlphaWall;
        newAlphaWall.x = Mathf.Lerp(0,0.5f, ((maxWallDistanceAlert-30.0f) - distanceToWall) / (maxWallDistanceAlert-30.0f));
        newAlphaWall.y = Mathf.Lerp(0,1,  ((maxWallDistanceAlert-30.0f) - distanceToWall) / (maxWallDistanceAlert-30.0f));
        _dangerWallEffect.SetVector2("Alpha Values", newAlphaWall);

        if (SceneManager.GetActiveScene().name == "EndStage")
        {
            _speedLinesEffect.Stop();
        }
        else
        {
            _speedLinesEffect.Play();
        }
    }

    private void FixedUpdate()
    {
        if (_carController && _carController.bot) return;
        
        float clampedVelocity = Mathf.Clamp((_rigidbody.velocity.magnitude * 2.2369362912f) - 60, 0, 100);
        _newAlpha.x = Mathf.Lerp(0.2f, 0, (100 - clampedVelocity) / 100);
        _newAlpha.y = Mathf.Lerp(0.5f, 0, (100 - clampedVelocity) / 100);

        if (!_speedLinesEffect) return;
        
        _speedLinesEffect.SetVector2("Alpha Values", _newAlpha);
    }
    
    public void SetOutlineActive(bool active)
    {
        _outlineObject.SetActive(active);
    }
}
