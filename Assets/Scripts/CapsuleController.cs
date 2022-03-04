using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleController : MonoBehaviour
{
    private const float MAX_LANDING_SPEED = 5;
    private const float RAYCAST_EPS = .1f;

    private Bounds bounds;

    private bool hasExploded = false;
    public GameObject explosionEffect;

    // Start is called before the first frame update
    void Start()
    {
        ComputeBounds();
    }

    private void ComputeBounds()
    {
        bounds = new Bounds();
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in childRenderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        Debug.Log(bounds.extents.y);
    }

    // Update is called once per frame
    void Update()
    {
        if (HasLanded())
        {
            if (IsOnLandingZone())
            {
                // TODO: simulation state -> landed on target
            } else
            {
                // TODO: simulationstate -> landed
            }
        }
    }

    private bool HasLanded()
    {
        if (Physics.Raycast(transform.position, Vector3.down, bounds.extents.y + RAYCAST_EPS))
        {
            // TODO: debug
        }
        return false;
    }
    
    private bool IsOnLandingZone()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit);
        return hit.collider.gameObject.name == "LandingZone";
        // TODO: debug
    }

    // TODO: check if leaves simulation zone
    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint[] contactPoints = new ContactPoint[collision.contactCount];
        collision.GetContacts(contactPoints);
        foreach (var contactPoint in contactPoints)
        {
            // Collision with capsule body
            if (contactPoint.thisCollider.tag != "CapsuleLeg")
            {
                Explode();
            }
        }

        // Collision at high speed (checking impulse would be a more general approach, but harder to learn)
        if (collision.relativeVelocity.magnitude > MAX_LANDING_SPEED) 
        {
            Explode();
        }
    }

    private void Explode() // TODO: game state -> crashed
    {
        if (!hasExploded) {
            Instantiate(explosionEffect, transform.position, transform.rotation);
            Destroy(gameObject);
            hasExploded = true;
        }
    }
}
