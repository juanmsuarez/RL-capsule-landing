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

    public Gradient scoreGradient;

    private SimulationData simulationData;
    public event Action<SimulationData> onSimulationDataChanged;

    public Boolean isTraining;
    public Boolean isDebugging;

    private void Start()
    {
        onSimulationDataChanged += DebugSimulationData;
        
        simulationData = new SimulationData();
        UpdateSimulationState(SimulationState.Starting);
    }

    private void DebugSimulationData(SimulationData simulationData)
    {
        if (isDebugging)
        {
            Debug.Log("State: " + simulationData.State);
            Debug.Log("Distance: " + simulationData.CapsuleData.Distance);
            Debug.Log("Angle: " + simulationData.CapsuleData.Angle);
            Debug.Log("Speed: " + simulationData.CapsuleData.Speed);
            Debug.Log("Score: " + simulationData.Score);
        }
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

                case SimulationState.Crashed:
                case SimulationState.LandedOnGround:
                case SimulationState.LandedOnLandingZone:
                    //if (isTraining)
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
        Color newColor = scoreGradient.Evaluate(t);
        landingZone.GetComponent<MeshRenderer>().material.color = newColor;
    }

    IEnumerator RestartSimulation()
    {
        if (capsule != null) {
            Destroy(capsule);
            yield return null;
        }

        simulationData = new SimulationData();
        UpdateSimulationState(SimulationState.Starting);
    }

    public void UpdateCapsuleData(CapsuleData capsuleData)
    {
        if (!simulationData.CapsuleData.Equals(capsuleData))
        {
            simulationData.CapsuleData = capsuleData.Clone();
            onSimulationDataChanged?.Invoke(simulationData);
        }
    }
}

public enum SimulationState
{
    None,
    Starting,
    Flying,
    Crashed,
    LandedOnGround,
    LandedOnLandingZone,
    Finished,
    Restarting
}

public class SimulationData
{
    // Rewards and penalties
    private const float FORWARD_REWARD = .1f;
    private const float STEP_PENALTY = -.05f;
    private const float DISTANCE_REWARD_FACTOR = .2f;
    private const float ANGLE_REWARD_FACTOR = .5f;
    private const float SPEED_REWARD_FACTOR = .3f;

    // Expected landing parameters
    // TODO: read from env or unify in SimulationParameters ScriptableObject
    private const float MAX_LANDING_DISTANCE = 40; // Approximate size of environment
    private const float MAX_LANDING_ANGLE = 90; // Horizontal direction
    private const float MAX_LANDING_SPEED = 15; // Maximum initial speed

    public SimulationState State { get; set; }

    private CapsuleData prevCapsuleData;
    private CapsuleData capsuleData;
    public CapsuleData CapsuleData {
        get => capsuleData;
        set
        {
            prevCapsuleData = capsuleData;
            capsuleData = value;
        }
    }
     
    public float Score {
        get
        {
            float distanceReward = Mathf.Max(1 - Mathf.Pow(capsuleData.Distance / MAX_LANDING_DISTANCE, 0.5f), -1);
            float angleReward = Mathf.Max(1 - Mathf.Pow(capsuleData.Angle / MAX_LANDING_ANGLE, 0.5f), -1);
            float speedReward = Mathf.Max(1 - Mathf.Pow(capsuleData.Speed / MAX_LANDING_SPEED, 0.5f), -1);
            return State switch
            {
                SimulationState.Flying => (capsuleData.Distance < prevCapsuleData.Distance ? FORWARD_REWARD : 0) + STEP_PENALTY,
                var x when 
                    x == SimulationState.Crashed || 
                    x == SimulationState.LandedOnGround || 
                    x == SimulationState.LandedOnLandingZone => DISTANCE_REWARD_FACTOR * distanceReward + ANGLE_REWARD_FACTOR * angleReward + SPEED_REWARD_FACTOR * speedReward,
                _ => 0
            };
        }
    }

    public SimulationData()
    {
        this.State = SimulationState.None;
        prevCapsuleData = new CapsuleData();
        capsuleData = new CapsuleData();
    }
}