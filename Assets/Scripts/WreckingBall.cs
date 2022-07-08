using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WreckingBall : MonoBehaviour
{
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float distance = 10f;
    [SerializeField] private bool x = false;
    [SerializeField] private bool y = true;
    [SerializeField] private bool z = false;
    
    // Update is called once per frame
    void Update()
    {
        float angle = distance * Mathf.Sin( Time.time * speed);
        transform.localRotation = Quaternion.Euler( x ? angle : 0, y ? angle : 0, z ? angle : 0);
    }
}
