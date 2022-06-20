using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CarEffectManager : MonoBehaviourPunCallbacks, IPunObservable
{
    private ParticleSystem[] flame;
    private ParticleSystem air;
    private PhotonView pv;
    private bool boosting;
    private bool airblasting;
    private bool backWall;
    private bool hooking;
    
    #region IPunObservable implementation
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(boosting);
            stream.SendNext(airblasting);
            Debug.Log("Stream Sending");
        }
        else
        {
            // Network player, receive data
            this.boosting = (bool)stream.ReceiveNext();
            this.airblasting = (bool)stream.ReceiveNext();
        }
    }
    #endregion
    
    void Start()
    {
        flame = transform.Find("BoostEffect").GetComponentsInChildren<ParticleSystem>();
        air = transform.Find("AirBlast").GetComponent<ParticleSystem>();
        pv = GetComponent<PhotonView>();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (pv.IsMine)
        {
            //Debug.Log(boosting +" :Boost");
            if (flame[0].isPlaying)
            {
                boosting = true;
            }
            else
            {
                boosting = false;
            }
            if (air.isPlaying)
            {
                airblasting = true;
            }
            else
            {
                airblasting = false;
            }
        }
        else
        {
            if (boosting)
            {
                foreach (ParticleSystem ps in flame)
                {
                    ps.Play();
                }
            }
            else
            {
                foreach (ParticleSystem ps in flame)
                {
                    ps.Stop();
                }
            }
            if (airblasting)
            {
                air.Play();
            }
            else
            {
                air.Stop();
            }
        }
    }
}
