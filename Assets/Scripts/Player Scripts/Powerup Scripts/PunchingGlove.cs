using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace Player_Scripts.Powerup_Scripts
{
    public class PunchingGlove : MonoBehaviour
    {
        private GameObject _playerRef;
        private PlayerPowerups _playerPowerups;
        private bool _hasFoundPlayer = false;

        private void Start()
        {
            _hasFoundPlayer = false;
            StartCoroutine(WaitForPlayer());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") && other.gameObject.layer != 7 && _hasFoundPlayer) // Player layer
            {
                _playerPowerups.ResetPunch();
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
                    _playerRef = player;
                    //Debug.Log("Player Found");
                    _hasFoundPlayer = true;
                    _playerPowerups = _playerRef.GetComponent<PlayerPowerups>();
                }
            }
        }
    }
}
