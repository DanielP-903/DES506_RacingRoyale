using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.VFX;

public class TimerCube : MonoBehaviour
{
    [SerializeField] private List<Material> materials = new List<Material>();

    private MeshRenderer _meshRenderer;

    private PlayerManager _playerRef;
    private bool _hasFoundPlayer;
    private GameManager _gm;

    private float _currentTimerValue;

    private VisualEffect _effect;
    
    // Start is called before the first frame update
    void Start()
    {
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
        //StartCoroutine(WaitForPlayer());
        _currentTimerValue = 3;
    }

    // Update is called once per frame
    void Update()
    {
        if ((int)PhotonNetwork.CurrentRoom.CustomProperties[("Timer"+_gm.GetStageNum())] != 0)
        {
            int hit = (int)PhotonNetwork.CurrentRoom.CustomProperties[("Timer"+_gm.GetStageNum())];
            _currentTimerValue = _gm.GetStartDelay() - ((PhotonNetwork.ServerTimestamp - hit) / 1000f);
            // if (_playerRef.startDelayText.text != "Go!")
            // {
            //     _currentTimerValue = _playerRef.timer;
            // }
            // else
            // {
            //     _currentTimerValue = 0;
            //     
            // }
            if (_currentTimerValue <= 3 && _currentTimerValue > 2)
            {
                if (!_meshRenderer.enabled)
                    _meshRenderer.enabled = true;
                var meshMats = _meshRenderer.materials;
                meshMats[1] = materials[0];
                _meshRenderer.materials = meshMats; 
                //Debug.Log("Changed to material 1");
            }
            else if (_currentTimerValue <= 2 && _currentTimerValue > 1)
            {
                if (!_meshRenderer.enabled)
                    _meshRenderer.enabled = true;
                var meshMats = _meshRenderer.materials;
                meshMats[1] = materials[1];
                _meshRenderer.materials = meshMats; 
                //Debug.Log("Changed to material 2");
            }
            else if (_currentTimerValue <= 1 && _currentTimerValue > 0.05f)
            {              
                if (!_meshRenderer.enabled)
                    _meshRenderer.enabled = true;
                var meshMats = _meshRenderer.materials;
                meshMats[1] = materials[2];
                _meshRenderer.materials = meshMats; 
                //Debug.Log("Changed to material 3");
            }
            else
            {
                //Destroy(this.gameObject);
                if (_meshRenderer.enabled)
                {
                    _effect.Play();
                    _meshRenderer.enabled = false;
                }
            }
        }
    }
    
    IEnumerator WaitForPlayer()
    {
        yield return new WaitForSeconds(1);
        
        GameObject[] listOfPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in listOfPlayers)
        {
            //Debug.Log("Player: " + player);
            if (!player.GetComponent<PhotonView>())
            {
                continue;
            }
            
            if (player.GetComponent<PhotonView>().IsMine && !player.GetComponent<CarController>().bot)
            {
                _playerRef = player.GetComponent<PlayerManager>();
                //Debug.Log("Player Found");
                _hasFoundPlayer = true;
            }
        }

        if (_playerRef.startDelayText.text == "Go!")
        {
            _playerRef.startDelayText.text = "3";
            _playerRef.timer = 3;
        }
    }
}
