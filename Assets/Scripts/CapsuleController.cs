using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleController : MonoBehaviour
{
    private new Rigidbody rigidbody;

    private const string LEG_TAG = "CapsuleLeg";
    private const float MAX_LANDING_SPEED = 5;

    private const int N_LEGS = 4;
    private const string LANDING_ZONE_NAME = "LandingZone";
    private const string GROUND_TAG = "Ground";
    enum LandingState { None, InAir, OnGround, OnLandingZone };
    private int[] nLegsInState = new int[Enum.GetNames(typeof(LandingState)).Length];

    public GameObject floor;

    private bool hasExploded = false;
    public GameObject explosionEffect;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();

        InitializeLandingState();
    }

    private void InitializeLandingState()
    {
        nLegsInState[(int)LandingState.InAir] = N_LEGS;
    }

    void FixedUpdate()
    {
        CheckInSceneBounds();

        CheckLanding();
    }

    private void CheckInSceneBounds()
    {
        var positionInFloor = new Vector3(transform.position.x, 0, transform.position.z);
        var floorBounds = floor.GetComponent<Collider>().bounds;

        if (!floorBounds.Contains(positionInFloor))
        {
            Explode();
        }
    }

    private void CheckLanding()
    {
        if (rigidbody.IsSleeping())
        {
            var capsuleState = ComputeCapsuleState();

            if (capsuleState == LandingState.OnLandingZone)
            {
                // TODO: simulation state -> landed on target
            }
            else if (capsuleState == LandingState.OnGround)
            {
                // TODO: simulation state -> landed
            }
        }
    }

    private LandingState ComputeCapsuleState()
    {
        var capsuleState = LandingState.None;
        foreach (var landingState in (LandingState[])Enum.GetValues(typeof(LandingState)))
        {
            if (nLegsInState[(int)landingState] > 0)
            {
                capsuleState = landingState;
                break;
            }
        }
        return capsuleState;
    }


    private void OnCollisionEnter(Collision collision)
    {
        CheckCrash(collision);
        AddLegCollision(collision);
    }

    private void CheckCrash(Collision collision)
    {
        var contactPoints = new ContactPoint[collision.contactCount];
        collision.GetContacts(contactPoints);
        foreach (var contactPoint in contactPoints)
        {
            if (!IsLegContact(contactPoint))
            {
                Explode();
            }
        }

        // Checking impulse would be a more general approach, but harder to learn
        if (IsHighSpeedCollision(collision))
        {
            Explode();
        }
    }

    private bool IsLegContact(ContactPoint contactPoint)
    {
        return contactPoint.thisCollider.tag == LEG_TAG;
    }

    private bool IsHighSpeedCollision(Collision collision)
    {

        return collision.relativeVelocity.magnitude > MAX_LANDING_SPEED;
    }

    private void AddLegCollision(Collision collision)
    {
        if (IsCollisionWithGround(collision))
        {
            var newLegState = IsCollisionWithLandingZone(collision) ? LandingState.OnLandingZone : LandingState.OnGround;
            nLegsInState[(int)LandingState.InAir]--;
            nLegsInState[(int)newLegState]++;
        }
    }

    private bool IsCollisionWithGround(Collision collision)
    {
        return collision.gameObject.tag == GROUND_TAG;
    }

    private bool IsCollisionWithLandingZone(Collision collision)
    {
        return collision.gameObject.name == LANDING_ZONE_NAME;
    }

    private void Explode()
    {
        if (!hasExploded) {
            Instantiate(explosionEffect, transform.position, transform.rotation);
            Destroy(gameObject);
            hasExploded = true;
            // TODO: game state -> crashed
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        RemoveLegCollision(collision);
    }

    private void RemoveLegCollision(Collision collision) {
        if (IsCollisionWithGround(collision))
        {
            var previousLegState = IsCollisionWithLandingZone(collision) ? LandingState.OnLandingZone : LandingState.OnGround;
            nLegsInState[(int)previousLegState]--;
            nLegsInState[(int)LandingState.InAir]++;
        }
    }
}
