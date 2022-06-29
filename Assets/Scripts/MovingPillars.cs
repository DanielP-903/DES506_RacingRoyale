using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPillars : MonoBehaviour
{
    public float speed = 2.5f, distance = 10f;
    public bool reverse;
    private Vector3 currentPosition;
    // Start is called before the first frame update
    void Start()
    {
        currentPosition = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = new Vector3(reverse ? -Mathf.PingPong(Time.time * speed, distance) + currentPosition.x : Mathf.PingPong(Time.time * speed, distance) + currentPosition.x, transform.position.y, transform.position.z);
    }
    //private void OnCollisionEnter(Collision collision)
    //{

    //    if (collision.gameObject.tag == "Player")
    //    {
    
    //        GameObject hitPlayer = collision.gameObject;
    //        if (hitPlayer.GetComponent<Rigidbody>())
    //        {
    //            //ContactPoint contactPoint = collision.GetContact(0);
    //            Vector3 direction = (hitPlayer.transform.position - transform.position).normalized;
    //            //hitPlayer.GetComponent<Rigidbody>().AddForce(direction * power);
    //            hitPlayer.GetComponent<Rigidbody>().AddForce(direction * power, ForceMode.Impulse);
    //        }

            
           

    //    }
    //}
}
