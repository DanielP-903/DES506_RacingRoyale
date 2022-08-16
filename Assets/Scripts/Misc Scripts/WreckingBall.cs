using UnityEngine;

/// <summary>
/// Handles the pendulum obstacle functionality
/// </summary>
public class WreckingBall : MonoBehaviour
{
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float distance = 10f;
    [SerializeField] private bool x = false;
    [SerializeField] private bool y = true;
    [SerializeField] private bool z = false;
    [SerializeField] private float force = 5000000;
    private float _time = 0;
    private bool _reverse = false;
    private float _angle = 0;

    // Update is called once per frame
    void Update()
    {
        if (!_reverse)
            _time += Time.deltaTime;
        else
            _time -= Time.deltaTime;
        _angle = distance * Mathf.Sin(_time * speed);
        transform.localRotation = Quaternion.Euler(x ? _angle : 0, y ? _angle : 0, z ? _angle : 0);


    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _reverse = !_reverse;
            GameObject hitPlayer = collision.gameObject;
            if (hitPlayer.GetComponent<Rigidbody>())
            {
                Vector3 contactPoint = collision.GetContact(0).point;
                Vector3 direction = contactPoint - transform.position;
                if (direction.z < 1 && direction.z > -1) direction.z = 1f;
                direction = new Vector3(0, 0, Mathf.Sin(_time * speed) < 0f ? Mathf.Abs(direction.z) * -1 : Mathf.Abs(direction.z));
                hitPlayer.GetComponent<Rigidbody>().AddForce(direction * force, ForceMode.Impulse);
            }
        }
    }
}
