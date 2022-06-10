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
    void Update()
    {
        transform.position = new Vector3(reverse ? -Mathf.PingPong(Time.time * speed, distance) + currentPosition.x : Mathf.PingPong(Time.time * speed, distance) + currentPosition.x, transform.position.y, transform.position.z);
    }
}
