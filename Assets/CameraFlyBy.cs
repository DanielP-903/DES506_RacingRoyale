using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraFlyBy : MonoBehaviour
{
    public bool activateFlyBy = false;

    private Animator _animator;
    private CinemachineVirtualCamera _vc;
    // Start is called before the first frame update
    void Start()
    {
        _vc = GetComponent<CinemachineVirtualCamera>();
        _animator = GetComponent<Animator>();
        //activateFlyBy = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (activateFlyBy)
        {
            _animator.Play("FlyBy");
        }

        if (_vc.GetCinemachineComponent<CinemachineTrackedDolly>().m_PathPosition > 16.95f)
        {
            activateFlyBy = false;
        }
    }
}
