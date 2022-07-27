using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WreckingBallCollideForce : MonoBehaviour
{

    
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
                Vector3 direction = (hitPlayer.transform.position - transform.position).normalized;
                //hitPlayer.GetComponent<Rigidbody>().AddForce(direction * power);
                Debug.Log("test + " + direction * 100);
                hitPlayer.GetComponent<Rigidbody>().AddForce(Vector3.up * 10000, ForceMode.Impulse);
            }




        }
    }
}
