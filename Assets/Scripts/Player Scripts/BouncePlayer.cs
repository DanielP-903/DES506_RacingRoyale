using UnityEngine;

public class BouncePlayer : MonoBehaviour
{
    [SerializeField] private float jumpValue = 1000000;
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Collided with Player");
            collision.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpValue);
        }
    }
}