using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxingGloveObstacle : MonoBehaviour
{
    public float speed = 2.5f, distance = 50f, waitTime = 5f;
    public bool doUp = true;
    public GameObject BoxingGloveEnd;
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

        MoveGate();

    }

    IEnumerator GateUp()
    {

        while (true)
        {
            yield return new WaitForSeconds(waitTime);

            doUp = !doUp;
        }
        
    }

    void MoveGate()
    {
        if(doUp)
            transform.position = Vector3.MoveTowards(transform.position, endPosition, speed * Time.deltaTime);
        else
            transform.position = Vector3.MoveTowards(transform.position, startPosition, speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collided");
    }
}
    
