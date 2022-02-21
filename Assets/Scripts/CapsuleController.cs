using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleController : MonoBehaviour
{
    private Rigidbody capsuleRigidbody;

    // Start is called before the first frame update
    void Start()
    {
        // TODO: set the capsule's initial velocity when not training
        // capsuleRigidbody = GetComponent<Rigidbody>();
        // capsuleRigidbody.AddForce(-transform.up * 5);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
