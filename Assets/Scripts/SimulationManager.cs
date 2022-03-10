using System;
using UnityEngine;
using Unity.MLAgents.Policies;
using System.Collections;

// Simulation manager isn't a Singleton (or a ScriptableObject) because we need one instance per TrainingEnvironment
public class SimulationManager : MonoBehaviour
{
    public GameObject capsulePrefab;
    private GameObject capsule;
    public GameObject trainingEnvironment;
    public GameObject landingZone;
    public GameObject floor;

    private SimulationData simulationData;
    public event Action<SimulationData> onSimulationDataChanged;
    public Boolean isTraining;

    private void Start()
    {
        onSimulationDataChanged += DebugSimulationData;
        
        simulationData = new SimulationData();
        UpdateSimulationState(SimulationState.Starting);
    }

    private void DebugSimulationData(SimulationData simulationData)
    {
        Debug.Log("State: " + simulationData.State);
        Debug.Log("Distance: " + simulationData.DistanceToLandingZone);
        Debug.Log("Score: " + simulationData.Score);
    }

    public void UpdateSimulationState(SimulationState newSimulationState)
    {
        if (simulationData.State != newSimulationState)
        {
            simulationData.State = newSimulationState;
            onSimulationDataChanged?.Invoke(simulationData);

            switch (newSimulationState)
            {
                case SimulationState.Starting:
                    StartSimulation();
                    break;

                case SimulationState.Flying:
                    break;

                case SimulationState.Collided:
                case SimulationState.LandingSpeedExceeded:
                case SimulationState.LandedOnGround:
                case SimulationState.LandedOnLandingZone:
                    if (isTraining)
                    {
                        ShowScoreInLandingZone(simulationData.Score);
                    }
                    UpdateSimulationState(SimulationState.Finished);
                    break;

                case SimulationState.Finished:
                    UpdateSimulationState(SimulationState.Restarting);
                    break;

                case SimulationState.Restarting:
                    StartCoroutine(RestartSimulation());
                    break;

                default:
                    throw new ArgumentException();
            }
        }
    }

    public void StartSimulation()
    {
        capsule = Instantiate(capsulePrefab, trainingEnvironment.transform, false);

        var capsuleController = capsule.GetComponent<CapsuleController>();
        capsuleController.simulationManager = this;
        capsuleController.landingZone = landingZone;
        capsuleController.floor = floor;

        var capsuleAgent = capsule.GetComponent<CapsuleAgent>();
        capsuleAgent.simulationManager = this;
        capsuleAgent.floor = floor;

        var behaviorParameters = capsule.GetComponent<BehaviorParameters>();
        behaviorParameters.BehaviorType = isTraining ? BehaviorType.Default : BehaviorType.HeuristicOnly;

        capsule.SetActive(true);
    
        UpdateSimulationState(SimulationState.Flying);    
    }

    
    private void ShowScoreInLandingZone(float score)
    {
        float t = Mathf.InverseLerp(-1, 1, score);
        landingZone.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.red, Color.green, t);
    }

    IEnumerator RestartSimulation()
    {
        Debug.Log("Restart");
        if (capsule != null) {
            Debug.Log("Destroy");
            Destroy(capsule);
            yield return null;
        }

        simulationData = new SimulationData();
        UpdateSimulationState(SimulationState.Starting);
    }

    public void UpdateDistanceToLandingZone(float distanceToLandingZone)
    {
        if (simulationData.DistanceToLandingZone != distanceToLandingZone)
        {
            simulationData.DistanceToLandingZone = distanceToLandingZone;
            onSimulationDataChanged?.Invoke(simulationData);
        }
    }
}

public enum SimulationState
{
    None,
    Starting,
    Flying,
    Collided,
    LandingSpeedExceeded,
    LandedOnGround,
    LandedOnLandingZone,
    Finished,
    Restarting
}

public class SimulationData
{
    private const float FORWARD_REWARD = .1f;
    private const float STEP_PENALTY = -.05f;
    private const float COLLISION_PENALTY = -1;
    private const float SPEED_EXCESS_PENALTY = -.25f;
    private const float GROUND_LANDING_REWARD = .5f;
    private const float ZONE_LANDING_REWARD = .75f;
    private const float DISTANCE_REWARD_FACTOR = .25f;

    public SimulationState State { get; set; }

    // public float Angle { get; set; }

    // public float Speed { get; set; }

    private float previousDistanceToLandingZone;
    private float distanceToLandingZone;
    public float DistanceToLandingZone {
        get => distanceToLandingZone;
        set
        {
            previousDistanceToLandingZone = distanceToLandingZone;
            distanceToLandingZone = value;
        }
    }
     
    public float Score {
        get
        {
            // float speedReward =;
            // float angleReward =;
            float distanceReward = 1 / (1 + distanceToLandingZone); // TODO: maybe scale it up?
            return State switch
            {
                // TODO: rem forward reward? unnecessary

                SimulationState.Flying => (distanceToLandingZone < previousDistanceToLandingZone ? FORWARD_REWARD : 0) + STEP_PENALTY, // TODO: continuous reward?
                SimulationState.Collided => COLLISION_PENALTY, // TODO: reward % angle / positively?
                SimulationState.LandingSpeedExceeded => SPEED_EXCESS_PENALTY, // TODO; reward % speed / positively?
                SimulationState.LandedOnGround => GROUND_LANDING_REWARD + DISTANCE_REWARD_FACTOR * distanceReward,
                SimulationState.LandedOnLandingZone => ZONE_LANDING_REWARD + DISTANCE_REWARD_FACTOR * distanceReward,
                _ => 0
            };
        }
    }

    public SimulationData(SimulationState state = SimulationState.None, float distanceToLandingZone = float.MinValue)
    {
        this.State = state;
        this.DistanceToLandingZone = distanceToLandingZone;
        this.previousDistanceToLandingZone = distanceToLandingZone;
    }
}