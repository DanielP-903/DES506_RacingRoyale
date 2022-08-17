using System.Linq;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the camera flybys at the beginning of each level
/// </summary>
public class CameraFlyBy : MonoBehaviour
{
    public bool activateFlyBy;
    [SerializeField] private float maxFlyByPathPosition = 16.95f;
    private Animator _animator;
    private CinemachineVirtualCamera _vc;

    // Start is called before the first frame update
    void Start()
    {
        _vc = GetComponent<CinemachineVirtualCamera>();
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (activateFlyBy)
        {
            _animator.Play("FlyBy");
        }

        // Stop the flyby at the appropriate moment (reached end of path or input detected)
        if (_vc.GetCinemachineComponent<CinemachineTrackedDolly>().m_PathPosition > maxFlyByPathPosition ||
            Keyboard.current.anyKey.isPressed || (Gamepad.all.Count>0 && Gamepad.current.allControls.Any()))
        {
            activateFlyBy = false;
            _animator.StopPlayback();
        }
    }
}
