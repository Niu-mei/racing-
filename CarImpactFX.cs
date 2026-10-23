using UnityEngine;

public class CarImpactFX : MonoBehaviour
{
    public AudioSource impactAudio;
    public ParticleSystem impactParticles;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude < 1f)
            return;

        if (impactAudio != null && !impactAudio.isPlaying)
            impactAudio.Play();

        if (impactParticles != null)
        {
            impactParticles.transform.position = collision.contacts[0].point;
            impactParticles.Play();
        }
    }
}
