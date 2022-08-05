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
    [SerializeField] private float force = 5000000;
    private float time = 0;
    private bool reverse = false;
    private float angle = 0;

    // Update is called once per frame
    void Update()
    {
        if (!reverse)
            time = time + Time.deltaTime;
        else
            time = time - Time.deltaTime;
        angle = distance * Mathf.Sin(time * speed);
        transform.localRotation = Quaternion.Euler(x ? angle : 0, y ? angle : 0, z ? angle : 0);


    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            reverse = !reverse;
            GameObject hitPlayer = collision.gameObject;
            if (hitPlayer.GetComponent<Rigidbody>())
            {
                Vector3 contactPoint = collision.GetContact(0).point;
                Vector3 direction = contactPoint - transform.position;
                if (direction.z < 1 && direction.z > -1) direction.z = 1f;
                direction = new Vector3(0, 0, Mathf.Sin(time * speed) < 0f ? Mathf.Abs(direction.z) * -1 : Mathf.Abs(direction.z));
                //Debug.Log("test " + direction);
                hitPlayer.GetComponent<Rigidbody>().AddForce(direction * force, ForceMode.Impulse);
            }
        }
    }
}
