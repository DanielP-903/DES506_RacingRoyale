using System.Collections;
using UnityEngine;

/// <summary>
/// Handles dissolving of track elements as the wall moves over them
/// </summary>
public class Dissolve : MonoBehaviour
{
    public bool dissolve;
    [SerializeField] private bool isCheckpoint; 
    [SerializeField] private  bool isStartingLocation;
     
    private MeshRenderer _meshRenderer;
    private MeshRenderer _meshRendererClip;
    private MeshCollider _meshCollider;
    private static readonly int FadeOutSlider = Shader.PropertyToID("FadeOutSlider");
    private float _dissolveTimer;
    private bool _canDetect = true; 
    private CheckpointSystem _cs;
    private WallFollow _wallFollow;
    
    // Start is called before the first frame update
    void Start()
    {
        if (isCheckpoint)
        {
            _cs = transform.parent.Find("CheckpointSystem").GetComponent<CheckpointSystem>();
            _canDetect = true;
            dissolve = false;
        }
        else if (isStartingLocation)
        {
            _dissolveTimer = 0.0f;
            _canDetect = true;
            dissolve = false;
        }
        else
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            GameObject meshRendererClipObject = transform.parent.GetChild(0).gameObject;
            if (!meshRendererClipObject) Debug.LogError("meshRendererClipObject DOES NOT exist!");
            _meshRendererClip = meshRendererClipObject.GetComponent<MeshRenderer>();
            _meshCollider = GetComponent<MeshCollider>();
            _dissolveTimer = 0.0f;
            _canDetect = true;
            dissolve = false;
        }

        GameObject wall = GameObject.FindGameObjectWithTag("EliminationZone");
        if (wall)
            _wallFollow = wall.GetComponent<WallFollow>();
    }

    void OnLevelWasLoaded()
    {
        GameObject wall = GameObject.FindGameObjectWithTag("EliminationZone");
        if (wall)
            _wallFollow = wall.GetComponent<WallFollow>();
    }

    // Update is called once per frame
    void Update()
    {
        if (dissolve && !isStartingLocation)
        {
            _dissolveTimer += Time.deltaTime;
            _dissolveTimer = Mathf.Clamp(_dissolveTimer, 0, _wallFollow.dissolveTime);
            float dissolveFloat = Mathf.Lerp(1, 0, (_wallFollow.dissolveTime - _dissolveTimer) / _wallFollow.dissolveTime);
            foreach (var material in _meshRenderer.materials)
            {
                material.SetFloat(FadeOutSlider, dissolveFloat);
            }
            if (_meshRendererClip != null)
            {
                _meshRendererClip.materials[0].SetFloat(FadeOutSlider, dissolveFloat);
            }
            if (dissolveFloat >= _wallFollow.colliderDissolveThreshold)
            {
                _meshCollider.enabled = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EliminationZone") && _canDetect)
        {
            if (isCheckpoint)
            {
                Debug.Log("Eliminated Checkpoint");
                _cs.EliminateCheckpoint(this.gameObject);
            }
            else
            {
                StartCoroutine(DelayDissolve());
            }
        }
    }

    /// <summary>
    /// Delays the dissolving of the track piece based on a pre-defined amount of time
    /// </summary>
    private IEnumerator DelayDissolve()
    {
        _canDetect = false;
        yield return new WaitForSeconds(_wallFollow.timeUntilDissolve);
        dissolve = true;
        _dissolveTimer = 0.0f;
    }
}
