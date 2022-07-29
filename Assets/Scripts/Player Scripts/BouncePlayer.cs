using UnityEngine;

namespace Player_Scripts
{
    public class BouncePlayer : MonoBehaviour
    {
        [SerializeField] private float jumpValue = 1000000;
    
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Debug.Log("Collided with Player");
                collision.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpValue);
            }
        }
    }
}
