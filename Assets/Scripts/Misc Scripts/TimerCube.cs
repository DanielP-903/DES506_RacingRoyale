using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Handles the timer cubes at the start of levels 1 and 2
/// </summary>
public class TimerCube : MonoBehaviour
{
    [SerializeField] private List<Material> materials = new List<Material>();

    private MeshRenderer _meshRenderer;
    private PlayerManager _playerRef;
    private AudioManager _audioRef;
    private bool _hasFoundPlayer;
    private GameManager _gm;
    private GameObject _startBlocker;
    private float _currentTimerValue;
    private float _previousTimerValue;
    private VisualEffect _effect;
    private bool _played;

    // Start is called before the first frame update
    void Start()
    {
        _startBlocker = GameObject.Find("StartBlocker");
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.enabled = true;
        _effect = transform.GetChild(0).GetComponent<VisualEffect>();
        StartCoroutine(WaitForPlayer());
    }

    void OnLevelWasLoaded()
    {
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.enabled = true;
        _effect = transform.GetChild(0).GetComponent<VisualEffect>();
        _effect.Stop();
        _currentTimerValue = 3;
        _played = false;
    }

    // Update is called once per frame
    void Update()
    {
        int timerVal = 0;
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Timer" + _gm.GetStageNum()))
        {
            timerVal = (int)PhotonNetwork.CurrentRoom.CustomProperties[("Timer" + _gm.GetStageNum())];
        }

        if (timerVal != 0 && _hasFoundPlayer)
        {
            if (!_played)
            {
                _audioRef.PlaySound("GameStartFirst");
                _played = true;
            }
            
            int hit = (int)PhotonNetwork.CurrentRoom.CustomProperties[("Timer"+_gm.GetStageNum())];
            _currentTimerValue = _gm.GetStartDelay() - ((PhotonNetwork.ServerTimestamp - hit) / 1000f);
            if (_currentTimerValue <= 3 && _currentTimerValue > 2)
            {
                if (!_meshRenderer.enabled)
                    _meshRenderer.enabled = true;
                var meshMats = _meshRenderer.materials;
                meshMats[1] = materials[0];
                _meshRenderer.materials = meshMats; 
            }
            else if (_currentTimerValue <= 2 && _currentTimerValue > 1)
            {
                if (_previousTimerValue > 2)
                {
                    _audioRef.PlaySound("GameStartFirst");
                }
                if (!_meshRenderer.enabled)
                    _meshRenderer.enabled = true;
                var meshMats = _meshRenderer.materials;
                meshMats[1] = materials[1];
                _meshRenderer.materials = meshMats; 
            }
            else if (_currentTimerValue <= 1 && _currentTimerValue > 0.05f)
            {              
                if (_previousTimerValue > 1)
                {
                    _audioRef.PlaySound("GameStartFirst");
                }
                if (!_meshRenderer.enabled)
                    _meshRenderer.enabled = true;
                var meshMats = _meshRenderer.materials;
                meshMats[1] = materials[2];
                _meshRenderer.materials = meshMats; 
            }
            else
            {
                if (_meshRenderer.enabled)
                {
                    _audioRef.PlaySound("GameStartFinal");
                    _effect.Play();
                    _meshRenderer.enabled = false;
                    _startBlocker.SetActive(false);
                }
            }
        }

        _previousTimerValue = _currentTimerValue;
    }
    
    /// <summary>
    /// Wait for player to spawn then get a reference
    /// </summary>
    IEnumerator WaitForPlayer()
    {
        yield return new WaitForSeconds(1);
        
        GameObject[] listOfPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in listOfPlayers)
        {
            if (!player.GetComponent<PhotonView>())
            {
                continue;
            }
            
            if (player.GetComponent<PhotonView>().IsMine && !player.GetComponent<CarController>().bot)
            {
                _playerRef = player.GetComponent<PlayerManager>();
                _audioRef = player.GetComponent<CarController>().audioManager;
                _hasFoundPlayer = true;
                if (_playerRef.startDelayText.text == "Go!")
                {
                    _playerRef.startDelayText.text = "3";
                    _playerRef.timer = 3;
                }
            }
        }
    }
}
