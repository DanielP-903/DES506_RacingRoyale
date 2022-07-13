using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseGate : MonoBehaviour
{
    public float speed = 2.5f, distance = 50f, waitTime = 10f;
    public bool gateClosed,reverse;
    public GameObject counterpartGate;
    private Vector3 _startPosition, _endPosition;
    private Coroutine _gateRoutine;

    // Start is called before the first frame update
    void Start()
    {
        if (reverse)
        {
            distance = distance * -1;
        }
        _startPosition = transform.localPosition;
        _endPosition = new Vector3(transform.localPosition.x, transform.localPosition.y , transform.localPosition.z + distance);
        _gateRoutine = StartCoroutine(GateUp());
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
            if (Vector3.Distance(transform.localPosition, _endPosition) > 0.1f)
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, _endPosition, speed * Time.deltaTime);
            
                
        }
        else
        {
            if (Vector3.Distance(transform.localPosition, _startPosition) > 0.1f)
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, _startPosition, speed * Time.deltaTime);
        }
            
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && Vector3.Distance(transform.localPosition, _startPosition) < 0.1f)
        {
            if (!counterpartGate.GetComponent<ChooseGate>()) return;
            ChooseGate choosegate = counterpartGate.GetComponent<ChooseGate>();
            if (choosegate.gateClosed == true)
            {
                choosegate.gateClosed = false;
                choosegate.StopCoroutine(choosegate._gateRoutine);
                choosegate._gateRoutine = choosegate.StartCoroutine(choosegate.GateUp());
            }
            gateClosed = true;
            StopCoroutine(_gateRoutine);
            _gateRoutine = StartCoroutine(GateUp());
        }
    }


}
    
