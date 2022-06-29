using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxingGloveObstacle : MonoBehaviour
{
    public float outSpeed = 100f, inSpeed = 10f, distance = 50f, waitTime = 5f;
    public bool doPunch = true;
    //public GameObject BoxingGloveEnd;
    private Vector3 startPosition, endPosition;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        endPosition = new Vector3(transform.position.x, transform.position.y , transform.position.z + +distance);
        StartCoroutine(GateUp());
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        MovePunch();

    }

    IEnumerator GateUp()
    {

        while (true)
        {
            yield return new WaitForSeconds(waitTime);

            doPunch = !doPunch;
        }
        
    }

    void MovePunch()
    {
        if(doPunch)
            transform.position = Vector3.MoveTowards(transform.position, endPosition, outSpeed * Time.deltaTime);
        else
            transform.position = Vector3.MoveTowards(transform.position, startPosition, inSpeed * Time.deltaTime);
    }

    //private void OnCollisionEnter(Collision collision)
    //{
        
    //    if(collision.gameObject.tag == "Player")
    //    {
    //        //Debug.Log("collided");
    //        //GameObject hitPlayer = collision.gameObject;
    //        //if (hitPlayer.GetComponent<Rigidbody>())
    //        //{
    //        //    hitPlayer.GetComponent<Rigidbody>().AddForce(Vector3.right);
    //        //}
            
            
    //        //ContactPoint contactPoint = collision.GetContact(0);
            
    //    }
    //}

}
    
