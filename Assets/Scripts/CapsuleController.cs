using System;
using UnityEngine;

public class CapsuleController : MonoBehaviour
{
    private const string LEG_TAG = "CapsuleLeg";
    private const int N_LEGS = 4;

    private const float MAX_LANDING_SPEED = 5;

    private const string LANDING_ZONE_NAME = "LandingZone";
    private const string GROUND_TAG = "Ground";

    public SimulationManager simulationManager;

    private new Rigidbody rigidbody;

    enum LandingState { None, InAir, OnGround, OnLandingZone };
    private int[] nLegsInState = new int[Enum.GetNames(typeof(LandingState)).Length];

    public GameObject landingZone;
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
        float distance = Vector3.Distance(transform.position, landingZone.transform.position);
        float angle = Quaternion.Angle(transform.rotation, Quaternion.identity);
        float speed = rigidbody.velocity.magnitude;
        simulationManager.UpdateCapsuleData(new CapsuleData(distance, angle, speed));

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
            simulationManager.UpdateSimulationState(SimulationState.Crashed);
        }
    }

    private void CheckLanding()
    {
        if (rigidbody.IsSleeping())
        {
            var capsuleState = ComputeCapsuleState();
            if (capsuleState == LandingState.OnLandingZone)
            {
                simulationManager.UpdateSimulationState(SimulationState.LandedOnLandingZone);
            }
            else if (capsuleState == LandingState.OnGround)
            {
                simulationManager.UpdateSimulationState(SimulationState.LandedOnGround);
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
            if (!IsLegContact(contactPoint) && !hasExploded)
            {
                Explode();
                simulationManager.UpdateSimulationState(SimulationState.Crashed);
            }
        }

        // Checking impulse would be a more general approach, but harder to learn
        if (IsHighSpeedCollision(collision) && !hasExploded)
        {
            Explode();
            simulationManager.UpdateSimulationState(SimulationState.Crashed);
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

public class CapsuleData
{
    public float Distance { get; set; }
    public float Angle { get; set; }
    public float Speed { get; set; }

    public CapsuleData(float distance = Mathf.Infinity, float angle = Mathf.Infinity, float speed = Mathf.Infinity)
    {
        Distance = distance;
        Angle = angle;
        Speed = speed;
    }

    public override bool Equals(object otherObj)
    {
        CapsuleData otherCapsuleData = otherObj as CapsuleData;
        if (otherCapsuleData == null)
            return false;
        else
            return Distance == otherCapsuleData.Distance && Angle == otherCapsuleData.Angle && Speed == otherCapsuleData.Speed;
    }

    public CapsuleData Clone()
    {
        return (CapsuleData) MemberwiseClone();
    }
}