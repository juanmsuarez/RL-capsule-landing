using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;
using Random = UnityEngine.Random;

public class CapsuleAgent : Agent {
    // Initial capsule state
    private const float START_COORDS_OFFSET = 10; // Minimum distance to training area limits
    private const float INITIAL_HEIGHT = 40;
    private const float MAX_INITIAL_SPEED = 25;
    private const float MAX_INITIAL_ROTATION = 30;
    // Rewards and penalties
    private const float FORWARD_REWARD = 0.1f;
    private const float STEP_PENALTY = -0.05f;
    private const float OBJECTIVE_REWARD = 0.5f;
    // Target capsule state
    private const float COLLISION_EPS = 0.1f;
    private const float TARGET_ROTATION_EPS = 0.1f;
    private const float MAX_LANDING_SPEED = 5;

    // Capsule game object
    private Rigidbody capsuleRigidbody;
    private Collider capsuleCollider;
    private Vector3 previousCapsulePosition;
    public EngineController[] engineControllers;
    // Other game objects
    public GameObject landingZone;
    public GameObject floor;
    private float minX, maxX, minZ, maxZ;

    // Start is called before the first frame update
    void Start()
    {
        capsuleRigidbody = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<Collider>();
        // Define training area limits according to floor bounds
        Collider floorCollider = floor.GetComponent<Collider>();
        minX = floorCollider.bounds.min.x;
        maxX = floorCollider.bounds.max.x;
        minZ = floorCollider.bounds.min.z;
        maxZ = floorCollider.bounds.max.z;
    }

    public override void OnEpisodeBegin()
    {
        // Set the initial values for the capsule's position, rotation, and velocities
        float x = Random.Range(minX + START_COORDS_OFFSET, maxX - START_COORDS_OFFSET);
        float y = INITIAL_HEIGHT;
        float z = Random.Range(minZ + START_COORDS_OFFSET, maxZ - START_COORDS_OFFSET);
        transform.localPosition = new Vector3(x, y, z);

        Quaternion forwardRotation = Quaternion.AngleAxis(Random.Range(-1, 1) * MAX_INITIAL_ROTATION, Vector3.forward);
        Quaternion rightRotation = Quaternion.AngleAxis(Random.Range(-1, 1) * MAX_INITIAL_ROTATION, Vector3.right);
        transform.localRotation = forwardRotation * rightRotation;
        
        capsuleRigidbody.velocity = -transform.up * MAX_INITIAL_SPEED * Random.value;
        
        capsuleRigidbody.angularVelocity = Vector3.zero;

        // Store initial capsule position
        previousCapsulePosition = transform.position;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observes the current state of the capsule
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.localRotation);
        sensor.AddObservation(capsuleRigidbody.velocity);
        sensor.AddObservation(capsuleRigidbody.angularVelocity);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // TODO borrar Debug.Log(Academy.Instance.StepCount);
        // Fire the engine corresponding to the action received (or none)
        int numberOfEngines = engineControllers.Length;
        var fireEngineActions = actionBuffers.DiscreteActions;
        for (int engineIndex = 0; engineIndex < numberOfEngines; engineIndex++)
        {
            if (fireEngineActions[engineIndex] == 1)
            {
                engineControllers[engineIndex].Fire();
            }
        }

        // If the capsule left the training zone, end the episode with a negative reward
        if (transform.localPosition.x < minX || transform.localPosition.x > maxX ||
            transform.localPosition.z < minZ || transform.localPosition.z > maxZ)
        {
            SetReward(-1);
            EndEpisode();
        }
        // If the capsule landed, end the episode with a reward proportional to proximity to landing zone and upward orientation
        else if (Physics.Raycast(capsuleCollider.bounds.center, Vector3.down, capsuleCollider.bounds.extents.y + COLLISION_EPS))
        {
            // TODO: reward landing on platform
            // TODO: change color according to result
            // TODO: continuos reward?

            bool isOrientedUpward = Quaternion.Angle(transform.rotation, Quaternion.identity) <= TARGET_ROTATION_EPS;
            if (isOrientedUpward)
            {
                AddReward(OBJECTIVE_REWARD);
            }

            bool hasSafeVelocity = capsuleRigidbody.velocity.magnitude <= MAX_LANDING_SPEED;
            if (hasSafeVelocity)
            {
                AddReward(OBJECTIVE_REWARD);
            }

            EndEpisode();
        }
        // If the capsule hasn't landed yet, reward flying towards the landing zone and penalize each step
        else {
            float previousDistanceToLandingZone = Vector3.Distance(previousCapsulePosition, landingZone.transform.position);
            float currentDistanceToLandingZone = Vector3.Distance(transform.position, landingZone.transform.position);

            if (currentDistanceToLandingZone < previousDistanceToLandingZone)
            {
                AddReward(FORWARD_REWARD);
            }
            AddReward(STEP_PENALTY);
        }

        // Store current capsule position
        previousCapsulePosition = transform.position;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var fireEngineActions = actionsOut.DiscreteActions;

        var numberOfEngines = engineControllers.Length;
        for (int engineIndex = 0; engineIndex < numberOfEngines; engineIndex++)
        {
            bool playerFiredEngine = Input.GetAxis(engineControllers[engineIndex].fireKey) > 0;
            fireEngineActions[engineIndex] = Convert.ToInt32(playerFiredEngine);
        }
    }
}
