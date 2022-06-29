using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseGate : MonoBehaviour
{
    public float speed = 2.5f, distance = 50f, waitTime = 10f;
    public bool gateClosed,reverse;
    public GameObject counterpartGate;
    private Vector3 startPosition, endPosition;
    private Coroutine gateRoutine;

    // Start is called before the first frame update
    void Start()
    {
        if (reverse)
        {
            distance = distance * -1;
        }
        startPosition = transform.localPosition;
        endPosition = new Vector3(transform.localPosition.x, transform.localPosition.y , transform.localPosition.z + distance);
        gateRoutine = StartCoroutine(GateUp());
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

            gateClosed = !gateClosed;
        }

    }

    void MoveGate()
    {

        if (gateClosed)
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (!counterpartGate.GetComponent<ChooseGate>()) return;
            ChooseGate choosegate = counterpartGate.GetComponent<ChooseGate>();
            if (choosegate.gateClosed == true)
            {
                choosegate.gateClosed = false;
                choosegate.StopCoroutine(choosegate.gateRoutine);
                choosegate.gateRoutine = choosegate.StartCoroutine(choosegate.GateUp());
            }
            gateClosed = true;
            StopCoroutine(gateRoutine);
            gateRoutine = StartCoroutine(GateUp());
        }
    }


}
    
