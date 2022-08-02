using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine.PostFX;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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
    public List<ParticleSystem> driftSmokeEffects = new List<ParticleSystem>();
    public ParticleSystem confettiParticleSystem;
    public VisualEffect elimEffect;
    public GameObject impactEffectObject;
    public GameObject impactEffectPrefab;

    [Header("Camera Profiles (Post-Pro)")] 
    public List<VolumeProfile> profiles;

    [Header("NORMAL VALUES")]
    [SerializeField] private Color vignColourBefore;
    [SerializeField] private float vignIntensityBefore;
    [SerializeField] private float vignSmoothnessBefore;
    [SerializeField] private bool vignRoundedBefore;
    [SerializeField] private float chromIntensityBefore;
    
    [Header("IN-ZONE VALUES")] 
    [SerializeField] private Color vignColourAfter;
    [SerializeField] private float vignIntensityAfter;
    [SerializeField] private float vignSmoothnessAfter;
    [SerializeField] private bool vignRoundedAfter;
    [SerializeField] private float chromIntensityAfter;
    
    [Header("Other")] 
    public float maxWallDistanceAlert = 30.0f;
    
    [HideInInspector] public bool boostPlaying;
    
    // Camera VFX
    private VisualEffect _speedLinesEffect;
    private VisualEffect _speedCircleEffect;
    private VisualEffect _dangerWallEffect;
    private VisualEffect _portalEffect;
    
    
    private GameObject _outlineObject;
    private GameObject _outlineObjectGrapple;
 
    private CarController _carController;
    private Rigidbody _rigidbody;
    private Camera _mainCam;
    private GameObject _wall;
    private Image _dangerPressureImg;
    private Vector2 _newAlpha;
    private VisualEffect _currentEffect;
    private PhotonView _photonView;
    private DataManager _dm;
    private Mesh[] _meshArray;
    private VolumeProfile _profile;
    
    // Profile components
    private Vignette _vignette;
    private ChromaticAberration _chromaticAberration;
    private ColorAdjustments _colourAdjustments;
    private SplitToning _splitToning;
    private DepthOfField _depthOfField;
    private ShadowsMidtonesHighlights _shadowsMidtonesHighlights;
    
    
    private bool _inZone;
    
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
            case "Confetti":
                confettiParticleSystem.Play();
                break;
            case "Elimination":
                elimEffect.Play();
                break;
            case "DriftSmoke":
                foreach (var effect in driftSmokeEffects)
                {
                    effect.Play();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private int GetVFXIndex(string vfxName)
    {
        return vfxName switch
        {
            "Impact" => 0,
            "SoftImpact" => 1,
            "GroundImpact" => 2,
            "ItemBoxImpact" => 3,
            "PowerImpact" => 4,
            "PunchImpact" => 5,
            "BoostEffect" => -1,
            "DangerWallEffect" => -1,
            "SpeedLinesEffect" => -1,
            "PortalEffect" => -1,
            "Confetti" => -1,
            "DriftSmoke" => -1,
            "ItemBoxDisappear" => 6,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void StopDriftEffects()
    {
        foreach (var effect in driftSmokeEffects)
        {
            effect.Stop();
        }
    }
    
    public void PlayVFXAtPosition(string vfxName, Vector3 pos)
    {
        impactEffectObject.transform.position = pos;
        PlayVFX(vfxName);
    }

    public void SpawnVFXAtPosition(string vfxName, Vector3 pos, float scaleFactor, bool isBillboard)
    {
        GameObject newSpawn = Instantiate(impactEffectPrefab, pos, Quaternion.identity);
        newSpawn.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        newSpawn.GetComponent<TemporaryEffect>().doNotDelete = false;
        newSpawn.GetComponent<TemporaryEffect>().isBillboard = isBillboard;
        newSpawn.GetComponent<VisualEffect>().visualEffectAsset = impactEffectAssets[GetVFXIndex(vfxName)];
    }
    
    #endregion

    private void Awake()
    {
        _carController = GetComponent<CarController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _photonView = GetComponent<PhotonView>();
        _rigidbody = GetComponent<Rigidbody>();

        if (!_carController.debug)
        {
            _dm = GameObject.Find("DataManager").GetComponent<DataManager>();
            _meshArray = _dm.GetMesh();
        }


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
        _outlineObjectGrapple = transform.GetChild(7).gameObject;
        _outlineObjectGrapple.SetActive(false);
        
        impactEffectObject.GetComponent<VisualEffect>();
        _speedCircleEffect.Stop();
        _portalEffect.Stop();
        
        if (SceneManager.GetActiveScene().name == "WaitingArea")
        {
            _dangerWallEffect.SetVector2("Alpha Values", new Vector2(0,0));
        }

        if (!_carController.bot && !_carController.debug)
        {
            _outlineObject.GetComponent<MeshFilter>().sharedMesh =
                _meshArray[(int)PhotonNetwork.LocalPlayer.CustomProperties["Skin"]];
            _outlineObjectGrapple.GetComponent<MeshFilter>().sharedMesh =
                _meshArray[(int)PhotonNetwork.LocalPlayer.CustomProperties["Skin"]];
        }

        if (!_carController.debug)
            _photonView.RPC("UpdateOutlineMeshes", RpcTarget.All, _photonView.ViewID, (int)PhotonNetwork.LocalPlayer.CustomProperties["Skin"]);

        StopDriftEffects();
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

        _profile = _mainCam.GetComponent<CinemachineVolumeSettings>().m_Profile;

        #region PostProComponentGetters
        if( _profile.TryGet<Vignette>( out var vign ) )
        {
            _vignette = vign;
        }        
        if( _profile.TryGet<ChromaticAberration>( out var chrom ) )
        {
            _chromaticAberration = chrom;
        }
        if( _profile.TryGet<ColorAdjustments>( out var colo ) )
        {
            _colourAdjustments = colo;
        }
        if( _profile.TryGet<SplitToning>( out var spli ) )
        {
            _splitToning = spli;
        }  
        if( _profile.TryGet<DepthOfField>( out var dept ) )
        {
            _depthOfField = dept;
        }
        if( _profile.TryGet<ShadowsMidtonesHighlights>( out var shad ) )
        {
            _shadowsMidtonesHighlights = shad;
        }  
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        if (!_photonView.IsMine) return;
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

        if (_inZone)
        {
            _vignette.color.value = Color.Lerp(_vignette.color.value, vignColourAfter, Time.deltaTime);
            _vignette.intensity.value = Mathf.Lerp(_vignette.intensity.value, vignIntensityAfter, Time.deltaTime);
            _vignette.smoothness.value = Mathf.Lerp(_vignette.smoothness.value, vignSmoothnessAfter, Time.deltaTime);
            _vignette.rounded.value = vignRoundedAfter;
            _chromaticAberration.intensity.value = Mathf.Lerp(_chromaticAberration.intensity.value, chromIntensityAfter, Time.deltaTime);

            _colourAdjustments.active = true;
            _splitToning.active = true;
            _depthOfField.active = true;
            _shadowsMidtonesHighlights.active = true;
        }
        else
        {
            _vignette.color.value = Color.Lerp(_vignette.color.value, vignColourBefore, Time.deltaTime);
            _vignette.intensity.value = Mathf.Lerp(_vignette.intensity.value, vignIntensityBefore, Time.deltaTime);
            _vignette.smoothness.value = Mathf.Lerp(_vignette.smoothness.value, vignSmoothnessBefore, Time.deltaTime);
            _vignette.rounded.value = vignRoundedBefore;
            _chromaticAberration.intensity.value = Mathf.Lerp(_chromaticAberration.intensity.value,  chromIntensityBefore, Time.deltaTime);
            
            _colourAdjustments.active = false;
            _splitToning.active = false;
            _depthOfField.active = false;
            _shadowsMidtonesHighlights.active = false;
        }
    }

    private void FixedUpdate()
    {
        if (!_photonView.IsMine) return;
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
    public void SetGrappleOutlineActive(bool active)
    {
        _outlineObjectGrapple.SetActive(active);
    }
    
    public void SetInZone(bool isInZone)
    {
         //= isInZone ? profiles[1] : profiles[0];
         _inZone = isInZone;

    }
}
