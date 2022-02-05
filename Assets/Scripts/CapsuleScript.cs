using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleScript : MonoBehaviour
{
    private const float fallMagnitude = 12000;
    private Rigidbody capsuleRigidbody;

    // Start is called before the first frame update
    void Start()
    {
        capsuleRigidbody = GetComponent<Rigidbody>();
        capsuleRigidbody.AddForce(-transform.up * fallMagnitude, ForceMode.Impulse);
        Debug.Log(transform.up);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
