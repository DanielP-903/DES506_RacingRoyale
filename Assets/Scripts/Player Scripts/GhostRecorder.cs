using UnityEngine;

namespace Player_Scripts
{
    public class GhostRecorder : MonoBehaviour
    {
        public SO_CarGhost ghost;
        private float timer;
        private float timeStampValue;
        private void Awake()
        {
            if (ghost.mode == GhostMode.Record)
            {
                ghost.ResetVars();
                timeStampValue = 0.0f;
                timer = 0.0f;
            }
        }

        private void Update()
        {
            timer += Time.unscaledDeltaTime;
            timeStampValue += Time.unscaledDeltaTime;

            if (ghost.mode == GhostMode.Record && timer >= (1/ghost.frequency))
            {
                ghost.timeStamp.Add(timeStampValue);
                ghost.position.Add(transform.position);
                ghost.rotation.Add(transform.rotation);
                timer = 0;
            }
        }
    }
}
