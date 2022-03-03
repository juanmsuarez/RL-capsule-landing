using System;
using UnityEngine;

public class EngineController : MonoBehaviour
{
    public String fireKey;
    private Boolean engineFired = false;

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
        engineFired = true;
    }

    public void Stop()
    {
        engineFired = false;
    }

    private void FixedUpdate()
    {
        if (engineFired)
        {
            capsuleRigidbody.AddForceAtPosition(transform.up * thrustMagnitude, transform.position,
                                            ForceMode.Impulse);

            if (!flameParticleSystem.isPlaying)
            {
                flameParticleSystem.Play();
            }
        } else
        {
            flameParticleSystem.Stop();
        }
    }
}
