using System.Collections;
using UnityEngine;

namespace Player_Scripts.Powerup_Scripts
{
    public class BoxingGloveObstacle : MonoBehaviour
    {
        public float outSpeed = 100f, inSpeed = 10f, distance = 50f, outTime = 5f, inTime = 3f;
        public enum Axis
        {
            x,
            y,
            z
        }
        public Axis currentAxis;
        private float currentWaitTime;
        private bool doPunch;
        private Vector3 startPosition, endPosition;

        // Start is called before the first frame update
        void Start()
        {
            currentWaitTime = inTime;
            startPosition = transform.localPosition;
            if (currentAxis == Axis.x)
            {
                endPosition = new Vector3(transform.localPosition.x + distance, transform.localPosition.y , transform.localPosition.z);
            }
            else if (currentAxis == Axis.y)
            {
                endPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + distance, transform.localPosition.z);
            }
            else
            {
                endPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + distance);
            }
           
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
                currentWaitTime = doPunch?  outTime :  inTime;
                yield return new WaitForSeconds(currentWaitTime);

                doPunch = !doPunch;
            }
        
        }

        void MovePunch()
        {
            if(doPunch)
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, endPosition, outSpeed * Time.deltaTime);
            else
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, startPosition, inSpeed * Time.deltaTime);
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
}
    
