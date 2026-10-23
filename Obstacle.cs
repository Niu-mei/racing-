using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float hitForce = 5f;

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;
        if (rb == null)
            return;

        Vector3 dir = (collision.transform.position - transform.position).normalized;
        rb.AddForce(dir * hitForce, ForceMode.Impulse);
    }
}
