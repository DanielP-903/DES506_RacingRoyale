

// <summary>
// Custom Altered Component to synchronize Transforms via PUN PhotonView with a more in depth Interpolation and Extrapolation.
// </summary>

namespace Photon.Pun
{
    using UnityEngine;

    [AddComponentMenu("Photon Networking/Photon Transform View Custom")]
    [HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
    public class PhotonTransformViewCustom : MonoBehaviourPun, IPunObservable
    {
        private float m_Distance;
        private float m_Angle;

        private Vector3 m_Direction;
        private Vector3 m_NetworkPosition;
        private Vector3 m_StoredPosition;

        private Quaternion m_NetworkRotation;

        public bool m_SynchronizePosition = true;
        public bool m_SynchronizeRotation = true;
        public bool m_SynchronizeScale = false;

        public float m_DistanceMod = 2;
        public float m_RotMod = 2;

        [Tooltip("Indicates if localPosition and localRotation should be used. Scale ignores this setting, and always uses localScale to avoid issues with lossyScale.")]
        public bool m_UseLocal;

        bool m_firstTake = false;

        /// <summary>
        /// Upon awake, store position and reset network position and rotation
        /// </summary>
        /// <returns></returns>
        public void Awake()
        {
            m_StoredPosition = transform.localPosition;
            m_NetworkPosition = Vector3.zero;

            m_NetworkRotation = Quaternion.identity;
        }

        /// <summary>
        /// Set useLocal to true upon reset
        /// </summary>
        /// <returns></returns>
        private void Reset()
        {
            // Only default to true with new instances. useLocal will remain false for old projects that are updating PUN.
            m_UseLocal = true;
        }

        /// <summary>
        /// On Enable, set firstTake to true so that the first read/write from the server can be differentiated
        /// </summary>
        /// <returns></returns>
        void OnEnable()
        {
            m_firstTake = true;
        }

        /// <summary>
        /// On update, if view is not owned by local player, update position based on if local position is updated or not
        /// </summary>
        /// <returns></returns>
        public void Update()
        {
            Transform tr = transform;

            if (!this.photonView.IsMine)
            {
                // If LocalTransform is to be used
                if (m_UseLocal)
                {
                    // Use Fixed Distance Modifier for Difference
                    tr.localPosition = Vector3.MoveTowards(tr.localPosition, this.m_NetworkPosition, m_DistanceMod * this.m_Distance  * Time.deltaTime * PhotonNetwork.SerializationRate);
                    tr.localRotation = Quaternion.RotateTowards(tr.localRotation, this.m_NetworkRotation, this.m_Angle * Time.deltaTime * PhotonNetwork.SerializationRate);
                    
                    // Use Average Between Positions Magnitude for Difference
                    //tr.position = Vector3.Lerp(tr.localPosition, this.m_NetworkPosition, (this.m_NetworkPosition - tr.localPosition).magnitude * m_DistanceMod * this.m_Distance * Time.deltaTime * PhotonNetwork.SerializationRate);
                    //tr.rotation = Quaternion.Lerp(tr.localRotation, this.m_NetworkRotation, m_RotMod * this.m_Angle * Time.deltaTime *  PhotonNetwork.SerializationRate);
                    
                    // Use half way point between positions for Difference
                    //tr.localPosition = tr.localPosition + (this.m_NetworkPosition - tr.localPosition) / 2;
                    //tr.localRotation = Quaternion.RotateTowards(tr.localRotation, this.m_NetworkRotation, this.m_Angle * Time.deltaTime * PhotonNetwork.SerializationRate);
                }
                else
                {
                    // Use Fixed Distance Modifier for Difference
                    tr.position = Vector3.MoveTowards(tr.position, this.m_NetworkPosition, m_DistanceMod * this.m_Distance * Time.deltaTime * PhotonNetwork.SerializationRate);
                    tr.rotation = Quaternion.RotateTowards(tr.rotation, this.m_NetworkRotation, this.m_Angle * Time.deltaTime *  PhotonNetwork.SerializationRate);
                    
                    // Use Average Between Positions Magnitude for Difference
                    //tr.position = Vector3.Lerp(tr.position, this.m_NetworkPosition, (this.m_NetworkPosition - tr.position).magnitude * this.m_Distance * Time.deltaTime * PhotonNetwork.SerializationRate);
                    //tr.rotation = Quaternion.Lerp(tr.rotation, this.m_NetworkRotation, this.m_Angle * Time.deltaTime *  PhotonNetwork.SerializationRate);
                    
                    // Use half way point between positions for Difference
                    //tr.position = tr.position + (this.m_NetworkPosition - tr.position) / 2;
                    //tr.localRotation = Quaternion.RotateTowards(tr.localRotation, this.m_NetworkRotation, this.m_Angle * Time.deltaTime * PhotonNetwork.SerializationRate);
                }
                
            }
        }

        /// <summary>
        /// Sets up how the view interacts with the server, sending data if owned, receiving data if not
        /// </summary>
        /// <param name="stream">Data stream determining whether data is being received or sent</param>
        /// <param name="info">Info being sent to or received by the server</param>
        /// <returns></returns>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            Transform tr = transform;

            // Write to Server
            if (stream.IsWriting)
            {
                // If position to be syncronised
                if (this.m_SynchronizePosition)
                {
                    // If LocalTransform is to be used
                    if (m_UseLocal)
                    {
                        this.m_Direction = tr.localPosition - this.m_StoredPosition;
                        this.m_StoredPosition = tr.localPosition;
                        // Send Position to others for Interpolation
                        stream.SendNext(tr.localPosition);
                        // Send Position to others for Extrapolation
                        stream.SendNext(this.m_Direction);
                    }
                    else
                    {
                        this.m_Direction = tr.position - this.m_StoredPosition;
                        this.m_StoredPosition = tr.position;
                        // Send Position to others for Interpolation
                        stream.SendNext(tr.position);
                        // Send Position to others for Extrapolation
                        stream.SendNext(this.m_Direction);
                    }
                }
                // If rotation to be syncronised
                if (this.m_SynchronizeRotation)
                {
                    // If LocalTransform is to be used
                    if (m_UseLocal)
                    {
                        // Send Rotation to others for Interpolation
                        stream.SendNext(tr.localRotation);
                    }
                    else
                    {
                        // Send Rotation to others for Interpolation
                        stream.SendNext(tr.rotation);
                    }
                }
                // If scale to be syncronised
                if (this.m_SynchronizeScale)
                {
                    stream.SendNext(tr.localScale);
                }
            }
            // Read from Server
            else
            {
                // If position to be syncronised
                if (this.m_SynchronizePosition)
                {
                    this.m_NetworkPosition = (Vector3)stream.ReceiveNext();
                    this.m_Direction = (Vector3)stream.ReceiveNext();

                    // If first take, set position to server position
                    if (m_firstTake)
                    {
                        if (m_UseLocal)
                            tr.localPosition = this.m_NetworkPosition;
                        else
                            tr.position = this.m_NetworkPosition;

                        this.m_Distance = 0f;
                    }
                    else
                    {
                        // Interpolate received data
                        float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                        this.m_NetworkPosition += this.m_Direction * lag;
                        // If LocalTransform is to be used
                        if (m_UseLocal)
                        {
                            // Set extrapolation modifier
                            this.m_Distance = Vector3.Distance(tr.localPosition, this.m_NetworkPosition);
                        }
                        else
                        {
                            // Set extrapolation modifier
                            this.m_Distance = Vector3.Distance(tr.position, this.m_NetworkPosition);
                        }
                    }
                }
                // If rotation to be syncronised
                if (this.m_SynchronizeRotation)
                {
                    this.m_NetworkRotation = (Quaternion)stream.ReceiveNext();
                    // If first take, set rotation to server rotation
                    if (m_firstTake)
                    {
                        this.m_Angle = 0f;
                        // If LocalTransform is to be used
                        if (m_UseLocal)
                        {
                            tr.localRotation = this.m_NetworkRotation;
                        }
                        else
                        {
                            tr.rotation = this.m_NetworkRotation;
                        }
                    }
                    else
                    {
                        // If LocalTransform is to be used
                        if (m_UseLocal)
                        {
                            // Set extrapolation modifier
                            this.m_Angle = Quaternion.Angle(tr.localRotation, this.m_NetworkRotation);
                        }
                        else
                        {
                            // Set extrapolation modifier
                            this.m_Angle = Quaternion.Angle(tr.rotation, this.m_NetworkRotation);
                        }
                    }
                }
                // If scale to be syncronised
                if (this.m_SynchronizeScale)
                {
                    // No Interpolation or Extrapolation
                    tr.localScale = (Vector3)stream.ReceiveNext();
                }
                // If first take has just occured, set first take to false
                if (m_firstTake)
                {
                    m_firstTake = false;
                }
            }
        }
    }
}