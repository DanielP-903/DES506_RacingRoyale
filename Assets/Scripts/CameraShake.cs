using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private GameObject _target;
    private Rigidbody _rb;
    private CarController _cc;

    
    private CinemachineVirtualCamera _virtualCamera;
    private CinemachineBasicMultiChannelPerlin _noiseSettings;

    private float _shakeTimer;
    private float _shakeTime;
    private float _intensity;

    private bool _hasFoundPlayer = false;
    
    private void Start()
    {
        _virtualCamera = GetComponent<CinemachineVirtualCamera>();
        _noiseSettings = _virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _shakeTimer = 0;
        StartCoroutine(WaitForPlayer());
    }

    void OnLevelWasLoaded()
    {
        _virtualCamera = GetComponent<CinemachineVirtualCamera>();
        _noiseSettings = _virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _shakeTimer = 0;
    }
    

    // Update is called once per frame
    void Update()
    {
        if (_hasFoundPlayer)
        {
            _shakeTimer = _shakeTimer <= 0 ? 0 : _shakeTimer - Time.fixedDeltaTime;

            if (_cc.GetGrounded() && (_rb.velocity.magnitude * 2.2369362912f) > 60)
            {
                _noiseSettings.m_AmplitudeGain = (_rb.velocity.magnitude*2) / 100;
            }
            else
            {
                _noiseSettings.m_AmplitudeGain = 0;
            }

            _noiseSettings.m_AmplitudeGain = Mathf.Clamp(_noiseSettings.m_AmplitudeGain, 0, 1);
        }
    }

    public void ShakeImmediate(float length, float intensity)
    {
        _shakeTime = length;
        _shakeTimer = _shakeTime;
        _intensity = intensity;
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
            
            if (player.GetComponent<PhotonView>().IsMine)
            {
                _target = player;
                //Debug.Log("Player Found");
                _hasFoundPlayer = true;
            }
        }
        
        //Debug.Log("There is a QUICK FIX at line 65 of PlayerHUDController");
        //_playerRef = listOfPlayers[0];
        
        //_playerRef = GameObject.FindGameObjectWithTag("Player");
        _rb = _target.GetComponent<Rigidbody>();
        _cc = _target.GetComponent<CarController>();
    }
}
