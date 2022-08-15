using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine.PostFX;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.VFX;
using ChromaticAberration = UnityEngine.Rendering.Universal.ChromaticAberration;
using Vignette = UnityEngine.Rendering.Universal.Vignette;

public class CarVFXHandler : MonoBehaviour
{
    [Header("Camera VFX")]
    public VisualEffect speedLinesEffect; 
    public VisualEffect dangerWallEffect;
    
    [Header("Boost VFX")]
    public List<ParticleSystem> boostEffects = new List<ParticleSystem>();
    public List<ParticleSystem> superBoostEffects = new List<ParticleSystem>();
    
    [Header("Impact VFX")]
    public List<VisualEffectAsset> impactEffectAssets = new List<VisualEffectAsset>();
    public List<ParticleSystem> driftSmokeEffects = new List<ParticleSystem>();
    public ParticleSystem confettiParticleSystem;
    public VisualEffect elimEffect;
    public GameObject impactEffectObject;
    public GameObject impactEffectPrefab;

    [Header("Post-Pro Old")]
    // Outside of zone
    public Color vignetteColourOld;
    public float vignetteIntensityOld;
    public float vignetteSmoothnessOld;
    public float chromaticIntensityOld;
    public float colourAdjustmentsPostExposureOld;
    public Color colourAdjustmentsColourFilterOld;
    public float colourAdjustmentsHueShiftOld;
    public float colourAdjustmentsSaturationOld;
    public Color splitToningShadowColourOld;
    public Color splitToningHighlightColourOld;
    public bool splitToningBalanceOld;
    
    [Header("Post-Pro New")]
    // In zone
    public Color vignetteColourNew;
    public float vignetteIntensityNew;
    public float vignetteSmoothnessNew;
    public float chromaticIntensityNew;
    public float colourAdjustmentsPostExposureNew;
    public Color colourAdjustmentsColourFilterNew;
    public float colourAdjustmentsHueShiftNew;
    public float colourAdjustmentsSaturationNew;
    public Color splitToningShadowColourNew;
    public Color splitToningHighlightColourNew;
    public bool splitToningBalanceNew;

    // Current
    private Color _vignetteColourCurrent;
    private float _vignetteIntensityCurrent;
    private float _vignetteSmoothnessCurrent;
    private float _chromaticIntensityCurrent;
    private float _colourAdjustmentsPostExposureCurrent;
    private Color _colourAdjustmentsColourFilterCurrent;
    private float _colourAdjustmentsHueShiftCurrent;
    private float _colourAdjustmentsSaturationCurrent;
    private Color _splitToningShadowColourCurrent;
    private Color _splitToningHighlightColourCurrent;
    private bool _splitToningBalanceCurrent;

    [Header("Other")] 
    [SerializeField] private float maxWallDistanceAlert = 30.0f;
    
    [HideInInspector] public bool boostPlaying;
    
    // Camera VFX
    private VisualEffect _speedCircleEffect;
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
    private bool _inZone;
    private VolumeProfile _profile;
    
    #region VFX-Activation

    // Regular boost FX activation
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

    // Super boost FX
    public void PlaySuperBoostEffectAlt()
    {
        foreach (var effect in superBoostEffects)
        {
            effect.Play();
        }
    }
    public void StopSuperBoostEffect()
    {
        foreach (var effect in superBoostEffects)
        {
            effect.Stop();
        }
    }
    
    // Play a VFX by name
    public void PlayVFX(string vfxName)
    {
        if (_carController)
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
                dangerWallEffect.Play();
                break;
            case "SpeedLinesEffect":
                speedLinesEffect.Play();
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

    // Retrieve a given VFX index by name
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
    
    // Play an effect using the attached impactEffectObject using a given position
    public void PlayVFXAtPosition(string vfxName, Vector3 pos)
    {
        impactEffectObject.transform.position = pos;
        PlayVFX(vfxName);
    }

    // Spawn an object which plays a given VFX (by name) at a defined position and scale (can also use billboarding for 2D effects)
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
            // Get custom player mesh choices
            _dm = GameObject.Find("DataManager").GetComponent<DataManager>();
            _dm.GetMesh();
            _dm.GetMats();
        }

        _currentEffect = impactEffectObject.GetComponent<VisualEffect>();
        
        if (_carController.debug)
        {
            // Debugging only: find the wall
            _wall = GameObject.FindGameObjectWithTag("EliminationZone");
        }
        
        // Get canvas and main camera
        GameObject canvas = GameObject.Find("Canvas");
        _dangerPressureImg = canvas.transform.GetChild(0).GetComponent<Image>();
        var mainCamObject = GameObject.Find("PlayerCamera");
        
        if (!mainCamObject) return;
        
        _mainCam = mainCamObject.GetComponent<Camera>();
        
        // Get vfx from hierarchy
        speedLinesEffect = _mainCam.transform.GetChild(2).gameObject.GetComponent<VisualEffect>();
        _speedCircleEffect = _mainCam.transform.GetChild(3).gameObject.GetComponent<VisualEffect>();
        dangerWallEffect = _mainCam.transform.GetChild(4).gameObject.GetComponent<VisualEffect>();
        _portalEffect = _mainCam.transform.GetChild(5).gameObject.GetComponent<VisualEffect>();
        
        // Initialise outline objects for powerups
        _outlineObject = transform.GetChild(6).gameObject;
        _outlineObject.SetActive(false);        
        _outlineObjectGrapple = transform.GetChild(7).gameObject;
        _outlineObjectGrapple.SetActive(false);
        
        // Initialise additional vfx
        impactEffectObject.GetComponent<VisualEffect>();
        _speedCircleEffect.Stop();
        _portalEffect.Stop();
        
        if (SceneManager.GetActiveScene().name == "WaitingArea")
        {
            dangerWallEffect.SetVector2("Alpha Values", new Vector2(0,0));
        }
        
        StopDriftEffects();

        // Get the current profile then reset it to default values
        _profile = _mainCam.GetComponent<CinemachineVolumeSettings>().m_Profile;
        ResetCameraProfile();
    }

    void OnLevelWasLoaded()
    {
        SetUp();
    }

    public void SetUp()
    {
        // Get relevant objects and vfx
        var canvas = GameObject.Find("Canvas");
        _wall = GameObject.FindGameObjectWithTag("EliminationZone");
        _dangerPressureImg = canvas.transform.GetChild(0).GetComponent<Image>();
        _mainCam =  GameObject.Find("PlayerCamera").GetComponent<Camera>();
        _carController = GetComponent<CarController>();
        speedLinesEffect = _mainCam.transform.GetChild(2).gameObject.GetComponent<VisualEffect>();
        _speedCircleEffect = _mainCam.transform.GetChild(3).gameObject.GetComponent<VisualEffect>();
        dangerWallEffect = _mainCam.transform.GetChild(4).gameObject.GetComponent<VisualEffect>();
        impactEffectObject.GetComponent<VisualEffect>();
        _speedCircleEffect.Stop();
        _portalEffect.Stop();

        // Disable VFX based on stage
        if (SceneManager.GetActiveScene().name == "WaitingArea")
        {
            dangerWallEffect.SetVector2("Alpha Values", new Vector2(0,0));
        }
        else if (SceneManager.GetActiveScene().name == "EndStage")
        {
            dangerWallEffect.SetVector2("Alpha Values", new Vector2(0,0));
            _speedCircleEffect.Stop();
            speedLinesEffect.Stop();
            dangerWallEffect.Stop();
        }
        
        ResetCameraProfile();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_photonView.IsMine) return;
        if (!_wall) return;
        if (_carController && _carController.bot) return;


        // Calculate wall danger vfx transparency based on player distance to wall
        var distanceToWall = Vector3.Distance(transform.position, _wall.transform.position);
        distanceToWall = Mathf.Clamp(distanceToWall, 0, maxWallDistanceAlert);
        _dangerPressureImg.color = Color.Lerp(Color.clear, Color.magenta, (maxWallDistanceAlert - distanceToWall) / maxWallDistanceAlert);
        
        Vector2 newAlphaWall;
        newAlphaWall.x = Mathf.Lerp(0,0.5f, ((maxWallDistanceAlert-30.0f) - distanceToWall) / (maxWallDistanceAlert-30.0f));
        newAlphaWall.y = Mathf.Lerp(0,1,  ((maxWallDistanceAlert-30.0f) - distanceToWall) / (maxWallDistanceAlert-30.0f));
        dangerWallEffect.SetVector2("Alpha Values", newAlphaWall);
        
        UpdateCameraProfile();
    }

    // System for lerping between two different camera profiles (in the zone vs outside the zone)
    private void UpdateCameraProfile()
    {
        _vignetteColourCurrent = Color.Lerp(_vignetteColourCurrent, _inZone ? vignetteColourNew : vignetteColourOld , Time.deltaTime);
        _vignetteIntensityCurrent = Mathf.Lerp(_vignetteIntensityCurrent, _inZone ? vignetteIntensityNew : vignetteIntensityOld, Time.deltaTime);
        _vignetteSmoothnessCurrent = Mathf.Lerp(_vignetteSmoothnessCurrent, _inZone ? vignetteSmoothnessNew : vignetteSmoothnessOld, Time.deltaTime);
        _chromaticIntensityCurrent = Mathf.Lerp(_chromaticIntensityCurrent, _inZone ? chromaticIntensityNew : chromaticIntensityOld, Time.deltaTime);
        _colourAdjustmentsPostExposureCurrent = Mathf.Lerp(_colourAdjustmentsPostExposureCurrent, _inZone ? colourAdjustmentsPostExposureNew : colourAdjustmentsPostExposureOld, Time.deltaTime);
        _colourAdjustmentsColourFilterCurrent = Color.Lerp(_colourAdjustmentsColourFilterCurrent, _inZone ? colourAdjustmentsColourFilterNew : colourAdjustmentsColourFilterOld , Time.deltaTime);
        _colourAdjustmentsHueShiftCurrent = Mathf.Lerp(_colourAdjustmentsHueShiftCurrent, _inZone ? colourAdjustmentsHueShiftNew : colourAdjustmentsHueShiftOld, Time.deltaTime);
        _colourAdjustmentsSaturationCurrent = Mathf.Lerp(_colourAdjustmentsSaturationCurrent, _inZone ? colourAdjustmentsSaturationNew : colourAdjustmentsSaturationOld, Time.deltaTime);
        _splitToningHighlightColourCurrent = Color.Lerp(_splitToningHighlightColourCurrent, _inZone ? splitToningHighlightColourNew : splitToningHighlightColourOld , Time.deltaTime);
        _splitToningShadowColourCurrent = Color.Lerp(_splitToningShadowColourCurrent, _inZone ? splitToningShadowColourNew : splitToningShadowColourOld , Time.deltaTime);
        _splitToningBalanceCurrent = _inZone ? splitToningBalanceNew : splitToningBalanceOld;
        
        if (_profile.TryGet<Vignette>(out var vign))
        {
            vign.color.value = _vignetteColourCurrent;
            vign.intensity.value = _vignetteIntensityCurrent;
            vign.smoothness.value = _vignetteSmoothnessCurrent;
        }

        if (_profile.TryGet<ChromaticAberration>(out var chrom))
        {
            chrom.intensity.value = _chromaticIntensityCurrent;
        }
        
        if (_profile.TryGet<ColorAdjustments>(out var colo))
        {
            colo.postExposure.value = _colourAdjustmentsPostExposureCurrent;
            colo.colorFilter.value = _colourAdjustmentsColourFilterCurrent;
            colo.hueShift.value = _colourAdjustmentsPostExposureCurrent;
            colo.saturation.value = _colourAdjustmentsPostExposureCurrent;
        }
        
        if (_profile.TryGet<SplitToning>(out var split))
        {
            split.shadows.value = _splitToningShadowColourCurrent;
            split.highlights.value = _splitToningHighlightColourCurrent;
            split.balance.overrideState = _splitToningBalanceCurrent;
        }
    }

    // Reset camera profile to outside the zone profile
    private void ResetCameraProfile()
    {
        if (_profile.TryGet<Vignette>(out var vign))
        {
            vign.color.value = vignetteColourOld;
            vign.intensity.value = vignetteIntensityOld;
            vign.smoothness.value = vignetteSmoothnessOld;
        }

        if (_profile.TryGet<ChromaticAberration>(out var chrom))
        {
            chrom.intensity.value = chromaticIntensityOld;
        }
        
        if (_profile.TryGet<ColorAdjustments>(out var colo))
        {
            colo.postExposure.value = colourAdjustmentsPostExposureOld;
            colo.colorFilter.value = colourAdjustmentsColourFilterOld;
            colo.hueShift.value = colourAdjustmentsHueShiftOld;
            colo.saturation.value = colourAdjustmentsSaturationOld;
        }
        
        if (_profile.TryGet<SplitToning>(out var split))
        {
            split.shadows.value = splitToningShadowColourOld;
            split.highlights.value = splitToningHighlightColourOld;
            split.balance.overrideState = splitToningBalanceOld;
        }
    }
    
    private void FixedUpdate()
    {
        if (!_photonView.IsMine) return;
        if (_carController && _carController.bot) return;
        
        // Calculate transparency of speed line vfx on camera based on player velocity
        float clampedVelocity = Mathf.Clamp((_rigidbody.velocity.magnitude * 2.2369362912f) - 60, 0, 100);
        _newAlpha.x = Mathf.Lerp(0.2f, 0, (100 - clampedVelocity) / 100);
        _newAlpha.y = Mathf.Lerp(0.5f, 0, (100 - clampedVelocity) / 100);
        
        if (!speedLinesEffect) return;
        
        speedLinesEffect.SetVector2("Alpha Values", _newAlpha);
    }
    
    // Toggle activation of punching glove and grappling hook outlines
    public void SetOutlineActive(bool active, GameObject target)
    {
        _outlineObject.GetComponent<MeshFilter>().sharedMesh = target.transform.Find("Outline Punch").GetComponent<MeshFilter>().sharedMesh;
        _outlineObject.SetActive(active);
    }
    public void SetGrappleOutlineActive(bool active, GameObject target)
    {
        _outlineObjectGrapple.GetComponent<MeshFilter>().sharedMesh = target.transform.Find("Outline Grapple").GetComponent<MeshFilter>().sharedMesh;
        _outlineObjectGrapple.SetActive(active);
    }
    
    // For post-processing profiles
    public void SetInZone(bool isInZone)
    {
        _inZone = isInZone;
    }
}
