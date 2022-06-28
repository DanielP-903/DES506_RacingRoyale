using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseGate : MonoBehaviour
{
    public float speed = 2.5f, distance = 50f, waitTime = 5f;
    public bool moveGate = true, reverse;
    private Vector3 startPosition, endPosition;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.localPosition;
        if (reverse)
        {
            distance = distance * -1;
        }
        endPosition = new Vector3(transform.localPosition.x, transform.localPosition.y , transform.localPosition.z + distance);
        StartCoroutine(GateUp());
    }

    // Update is called once per frame
    void Update()
    {

        MoveGate();

    }

    IEnumerator GateUp()
    {

        while (true)
        {
            yield return new WaitForSeconds(waitTime);

            moveGate = !moveGate;
        }
        
    }

    void MoveGate()
    {

        if (moveGate)
        {
            if (Vector3.Distance(transform.localPosition, endPosition) > 0.1f)
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, endPosition, speed * Time.deltaTime);
            
                
        }
        else
        {
            if (Vector3.Distance(transform.localPosition, startPosition) > 0.1f)
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, startPosition, speed * Time.deltaTime);
        }
            
    }
}
    
