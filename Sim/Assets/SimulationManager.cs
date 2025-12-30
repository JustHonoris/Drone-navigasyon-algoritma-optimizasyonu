using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class SimulationManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector3 gridSize = new Vector3(100f, 20f, 100f);
    public Vector3 gridOffset = Vector3.zero;
    public float nodeSize = 2f;
    public LayerMask obstacleLayer;

    [Header("UI Reference")]
    [SerializeField] private UIManager uiManager;

    [Header("Pathfinding")]
    public PathfindingAlgorithm selectedAlgorithm;

    [Header("Debug Settings")]
    public bool showGrid = true;
    public Color gridColor = new Color(1f, 1f, 1f, 0.2f);

    private GridManager gridManager;
    private PathfinderWrapper pathfinderWrapper;

    // Pathfinder referanslarý
    private AStarPathfinder aStarPathfinder;
    private HillClimbingPathfinder hillClimbingPathfinder;
    private RRTPathfinder rrtPathfinder;
    private BeamSearchPathfinder beamSearchPathfinder;
    // reset ayarlarý
    private readonly Vector3 defaultGridSize = new Vector3(200f, 40f, 200f);
    private readonly Vector3 defaultGridOffset =new Vector3(-150, 1, 0);
    private readonly float defaultNodeSize = 1f;
    private readonly PathfindingAlgorithm defaultAlgorithm = PathfindingAlgorithm.AStar;
    private readonly bool defaultShowGrid = true;
    private readonly Color defaultGridColor = new Color(1f, 1f, 1f, 0.2f);
    public enum PathfindingAlgorithm
    {
        AStar,
        HillClimbing,
        RRT,
        BeamSearch
    }

    private void Awake()
    {
        InitializeManagers();
    }

    void Start()
    {
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("UIManager bulunamadý!");
            }
        }
    }
    public void ResetSimulation()
    {
        // Grid ayarlarýný resetle
        gridSize = defaultGridSize;
        gridOffset = defaultGridOffset;
        nodeSize = defaultNodeSize;
        selectedAlgorithm = defaultAlgorithm;
        showGrid = defaultShowGrid;
        gridColor = defaultGridColor;

        // Grid'i yeniden baþlat
        InitializeManagers();

        // Algoritma özelliklerini resetle

        pathfinderWrapper.ResetPathfinders();

        // Drone'u resetle
        ResetDrones();

        // UI'ý güncelle
        if (uiManager != null)
        {
            uiManager.ResetUI();
        }
    }
    private void ResetAlgorithmProperties()
    {
        // Beam Search default deðerleri
        BeamSearchPathfinder.Properties defaultBeamProps = new BeamSearchPathfinder.Properties
        {
            beamWidth = 200,
            maxIterations = 40000,
            heightWeight = 1.0f
        };
        pathfinderWrapper.UpdateBeamSearchParameters(defaultBeamProps);

        // Hill Climbing default deðerleri
        HillClimbingPathfinder.Properties defaultHillProps = new HillClimbingPathfinder.Properties
        {
            maxAttempts = 50000,
            heightWeight = 1.0f,
            maxRestarts = 8,
            randomJumpProbability = 0.03f
        };
        pathfinderWrapper.UpdateHillClimbingParameters(defaultHillProps);

        // RRT default deðerleri
        RRTPathfinder.Properties defaultRRTProps = new RRTPathfinder.Properties
        {
            stepSize = 2f,
            goalBias = 0.3f,
            maxIterations = 2000,
            minDistance = 1.5f
        };
        pathfinderWrapper.UpdateRRTParameters(defaultRRTProps);

        AStarProperties defaultAStarProps = new AStarProperties
        {
            DiagonalCost = 1.4f,
            HeightCostMultiplier = 1.0f,
            AllowDiagonal = true,
            MaxHeightDifference = 1.5f
        };
        pathfinderWrapper.UpdateAStarParameters(defaultAStarProps);
    }

    private void ResetDrones()
    {
        var drones = FindObjectsOfType<DroneController>();
        foreach (var drone in drones)
        {
            drone.ResetDrone();
        }
    }
    private void InitializeManagers()
    {
        Debug.Log("asdasd");
        // Grid Manager setup
        gridManager = gameObject.AddComponent<GridManager>();
        gridManager.obstacleLayer = obstacleLayer;
        gridManager.InitializeGrid(gridSize, gridOffset, nodeSize);

        // Pathfinders setup
        aStarPathfinder = new AStarPathfinder(gridManager);
        hillClimbingPathfinder = new HillClimbingPathfinder(gridManager);
        rrtPathfinder = new RRTPathfinder(gridManager);
        beamSearchPathfinder = new BeamSearchPathfinder(gridManager);

        // Data collector referansýný al
        var dataCollector = GetComponent<PathfindingDataCollector>();
        if (dataCollector == null)
        {
            Debug.LogWarning("PathfindingDataCollector not found! Data collection will be disabled.");
        }

        // Wrapper'ý baþlat - mevcut pathfinder referanslarýný kullan
        pathfinderWrapper = new PathfinderWrapper(
            gridManager,
            dataCollector,
            uiManager,  // Yeni eklenen
            aStarPathfinder,
            hillClimbingPathfinder,
            rrtPathfinder,
            beamSearchPathfinder
        );
        ResetAlgorithmProperties();

    }
    public void UpdateAStarParameter(string paramName, float value)
    {
        var props = aStarPathfinder.GetProperties();

        switch (paramName)
        {
            case "heightCostMultiplier":
                props.HeightCostMultiplier = value;
                break;
            case "maxHeightDifference":
                props.MaxHeightDifference = value;
                break;
            case "diagonalCost":
                props.DiagonalCost = value;
                break;
        }

        pathfinderWrapper.UpdateAStarParameters(props);
    }
    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        return pathfinderWrapper.FindPath(selectedAlgorithm, startPos, targetPos);
    }

    public void TestPathFinding(DroneController drone, Vector3 targetPosition)
    {
        List<Vector3> path = FindPath(drone.transform.position, targetPosition);
        if (path != null)
        {
            drone.SetPath(path);
            Debug.Log($"Path found with {path.Count} waypoints using {selectedAlgorithm}");
        }
        else
        {
            Debug.LogWarning($"No path found using {selectedAlgorithm}!");
        }
    }

    // Parametre güncelleme metodlarý
    public void UpdateBeamSearchParameter(string paramName, float value)
    {
        BeamSearchPathfinder.Properties props = new BeamSearchPathfinder.Properties
        {
            beamWidth = paramName == "beamWidth" ? (int)value : beamSearchPathfinder.GetBeamWidth(),
            maxIterations = paramName == "maxIterations" ? (int)value : beamSearchPathfinder.GetMaxIterations(),
            heightWeight = paramName == "heightWeight" ? value : beamSearchPathfinder.GetHeightWeight()
        };

        pathfinderWrapper.UpdateBeamSearchParameters(props);
    }

    public void UpdateRRTParameter(string paramName, float value)
    {
        RRTPathfinder.Properties props = new RRTPathfinder.Properties
        {
            stepSize = paramName == "stepSize" ? value : rrtPathfinder.GetStepSize(),
            goalBias = paramName == "goalBias" ? value : rrtPathfinder.GetGoalBias(),
            maxIterations = paramName == "maxIterations" ? (int)value : rrtPathfinder.GetMaxIterations(),
            minDistance = paramName == "minDistance" ? value : rrtPathfinder.GetMinDistance()
        };

        pathfinderWrapper.UpdateRRTParameters(props);
    }

    public void UpdateHillClimbingParameter(string paramName, float value)
    {
        HillClimbingPathfinder.Properties props = new HillClimbingPathfinder.Properties
        {
            maxAttempts = paramName == "maxAttempts" ? (int)value : hillClimbingPathfinder.GetMaxAttempts(),
            heightWeight = paramName == "heightWeight" ? value : hillClimbingPathfinder.GetHeightWeight(),
            maxRestarts = paramName == "maxRestarts" ? (int)value : hillClimbingPathfinder.GetMaxRestarts(),
            randomJumpProbability = paramName == "randomJumpProbability" ? value : hillClimbingPathfinder.GetRandomJumpProbability()
        };

        pathfinderWrapper.UpdateHillClimbingParameters(props);
    }

    void OnDrawGizmos()
    {
        if (!showGrid) return;
        Gizmos.color = gridColor;
        Gizmos.DrawWireCube(gridOffset + gridSize * 0.5f, gridSize);
    }

    public Vector3 GetGridCenter()
    {
        return gridOffset + gridSize * 0.5f;
    }

    public bool IsPositionInGrid(Vector3 position)
    {
        Vector3 localPos = position - gridOffset;
        return localPos.x >= 0 && localPos.x <= gridSize.x &&
               localPos.y >= 0 && localPos.y <= gridSize.y &&
               localPos.z >= 0 && localPos.z <= gridSize.z;
    }

    public void UpdateGridSettings(Vector3 newSize, Vector3 newOffset, float newNodeSize)
    {
        gridSize = newSize;
        gridOffset = newOffset;
        nodeSize = newNodeSize;

        if (Application.isPlaying)
        {
            InitializeManagers();
        }
    }
}