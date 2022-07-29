using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player_Scripts
{
    public class CameraFlyBy : MonoBehaviour
    {
        public bool activateFlyBy = false;
        [SerializeField] private float maxFlyByPathPosition = 16.95f;

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

            if (_vc.GetCinemachineComponent<CinemachineTrackedDolly>().m_PathPosition > maxFlyByPathPosition || Keyboard.current.anyKey.isPressed)
            {
                Debug.Log("Flyby Complete");
                activateFlyBy = false;
                _animator.StopPlayback();
            }
        }
    }
}
