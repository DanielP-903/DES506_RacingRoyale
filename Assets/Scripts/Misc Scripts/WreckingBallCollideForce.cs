using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WreckingBallCollideForce : MonoBehaviour
{
    public int power = 10000000;
    
    // Update is called once per frame
    void Start()
    {
    }
    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.tag == "Player")
        {

            GameObject hitPlayer = collision.gameObject;
            if (hitPlayer.GetComponent<Rigidbody>())
            {
                //ContactPoint contactPoint = collision.GetContact(0);
                Vector3 direction = (transform.position - hitPlayer.transform.position).normalized;
                //hitPlayer.GetComponent<Rigidbody>().AddForce(direction * power);
                hitPlayer.GetComponent<Rigidbody>().AddForce(direction * power);
            }




        }
    }
}
