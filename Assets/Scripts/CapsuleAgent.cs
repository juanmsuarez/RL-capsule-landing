using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;
using Random = UnityEngine.Random;

public class CapsuleAgent : Agent {
    private const float SIMULATION_BOUNDS_OFFSET = 10;
    private const float INITIAL_HEIGHT = 20;
    private const float MAX_INITIAL_SPEED = 15;
    private const float MAX_INITIAL_ROTATION = 20;

    public SimulationManager simulationManager;

    private Rigidbody capsuleRigidbody;
    public EngineController[] engineControllers;

    public GameObject floor;
    private Bounds simulationBounds;

    void Awake()
    {
        capsuleRigidbody = GetComponent<Rigidbody>();

        Collider floorCollider = floor.GetComponent<Collider>();
        simulationBounds = floorCollider.bounds;

        simulationManager.onSimulationDataChanged += OnFeedbackReceived;
    }

    public override void OnEpisodeBegin()
    {
        RandomInitializeCapsule();
    }

    private void RandomInitializeCapsule()
    {
        transform.position = RandomPosition();
        transform.rotation = RandomRotation();
        capsuleRigidbody.velocity = RandomVelocity();
        capsuleRigidbody.angularVelocity = Vector3.zero;
    }

    private Vector3 RandomPosition()
    {
        float x = Random.Range(simulationBounds.min.x + SIMULATION_BOUNDS_OFFSET, simulationBounds.max.x - SIMULATION_BOUNDS_OFFSET);
        float y = INITIAL_HEIGHT;
        float z = Random.Range(simulationBounds.min.z + SIMULATION_BOUNDS_OFFSET, simulationBounds.max.z - SIMULATION_BOUNDS_OFFSET);
        return new Vector3(x, y, z);
    }

    private Quaternion RandomRotation()
    {
        Quaternion forwardRotation = Quaternion.AngleAxis(Random.Range(-1, 1) * MAX_INITIAL_ROTATION, Vector3.forward);
        Quaternion rightRotation = Quaternion.AngleAxis(Random.Range(-1, 1) * MAX_INITIAL_ROTATION, Vector3.right);
        return forwardRotation * rightRotation;
    }

    private Vector3 RandomVelocity()
    {
        return -transform.up * MAX_INITIAL_SPEED * Random.value;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.localRotation);
        sensor.AddObservation(capsuleRigidbody.velocity);
        sensor.AddObservation(capsuleRigidbody.angularVelocity);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Debug.Log("Step: " + Academy.Instance.StepCount);
        var fireEngineActions = actionBuffers.DiscreteActions;
        FireEngines(fireEngineActions);
    }

    private void FireEngines(ActionSegment<int> fireEngineActions)
    {
        int numberOfEngines = engineControllers.Length;
        for (int engineIndex = 0; engineIndex < numberOfEngines; engineIndex++)
        {
            if (fireEngineActions[engineIndex] == 1)
            {
                engineControllers[engineIndex].Fire();
            }
        }
    }

    public void OnFeedbackReceived(SimulationData newSimulationData)
    {
        Debug.Log("Effective reward added " + newSimulationData.Score);
        AddReward(newSimulationData.Score);

        if (newSimulationData.State == SimulationState.Finished)
        {
            Debug.Log("End episode");
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var fireEngineActions = actionsOut.DiscreteActions;
        MapInputToFireEngineActions(fireEngineActions);
    }

    private void MapInputToFireEngineActions(ActionSegment<int> fireEngineActions)
    {
        var numberOfEngines = engineControllers.Length;
        for (int engineIndex = 0; engineIndex < numberOfEngines; engineIndex++)
        {
            bool playerFiredEngine = Input.GetAxis(engineControllers[engineIndex].fireKey) > 0;
            fireEngineActions[engineIndex] = Convert.ToInt32(playerFiredEngine);
        }
    }

    void OnDestroy()
    {
        Debug.Log("Agent On Destroy");
        simulationManager.onSimulationDataChanged -= OnFeedbackReceived;
    }
}
