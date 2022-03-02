using System;
using UnityEngine;

public class EngineController : MonoBehaviour
{
    public String fireKey;

    public float thrustMagnitude = 100;
    public Rigidbody capsuleRigidbody;
    
    private ParticleSystem flameParticleSystem;

    // Start is called before the first frame update
    void Start()
    {
        flameParticleSystem = GetComponentInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis(fireKey) > 0)
        {
            Fire();
        } else {
            Stop();
        }
    }

    public void Fire()
    {
        capsuleRigidbody.AddForceAtPosition(transform.up * thrustMagnitude, transform.position, 
                                            ForceMode.Impulse);
        if (!flameParticleSystem.isPlaying)
        {
            flameParticleSystem.Play();
        }
    }

    public void Stop()
    {
        flameParticleSystem.Stop();
    }
}
