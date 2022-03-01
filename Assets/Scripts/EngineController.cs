using System;
using UnityEngine;

public class EngineController : MonoBehaviour
{
    public float thrustMagnitude = 100;
    public Rigidbody capsuleRigidbody;
    public String fireKey;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis(fireKey) > 0)
        {
            Fire();
        }
    }

    public void Fire()
    {
        capsuleRigidbody.AddForceAtPosition(transform.up * thrustMagnitude, transform.position, 
                                            ForceMode.Impulse);
    }
}
