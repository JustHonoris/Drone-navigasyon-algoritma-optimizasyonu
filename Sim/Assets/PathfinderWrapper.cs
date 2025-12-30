using UnityEngine;
using System.Collections.Generic;

public class PathfinderWrapper
{
    private readonly UIManager uiManager;

    private readonly GridManager gridManager;
    private readonly PathfindingDataCollector dataCollector;
    private readonly AStarPathfinder aStarPathfinder;
    private readonly HillClimbingPathfinder hillClimbingPathfinder;
    private readonly RRTPathfinder rrtPathfinder;
    private readonly BeamSearchPathfinder beamSearchPathfinder;

    public PathfinderWrapper(
        GridManager gridManager,
        PathfindingDataCollector dataCollector,
        UIManager uiManager,  // Yeni eklenen
        AStarPathfinder aStarPathfinder,
        HillClimbingPathfinder hillClimbingPathfinder,
        RRTPathfinder rrtPathfinder,
        BeamSearchPathfinder beamSearchPathfinder)
    {
        this.gridManager = gridManager;
        this.dataCollector = dataCollector;
        this.uiManager = uiManager;  // Yeni eklenen
        this.aStarPathfinder = aStarPathfinder;
        this.hillClimbingPathfinder = hillClimbingPathfinder;
        this.rrtPathfinder = rrtPathfinder;
        this.beamSearchPathfinder = beamSearchPathfinder;
    }
    // PathfinderWrapper.cs'e eklenecek
    public void ResetPathfinders()
    {
        // A* için
        if (aStarPathfinder != null)
        {
            aStarPathfinder.closedSet.Clear();
        }

        // Hill Climbing için
        if (hillClimbingPathfinder != null)
        {
            hillClimbingPathfinder.visitedNodes.Clear();
        }

        // RRT için
        if (rrtPathfinder != null)
        {
            rrtPathfinder.treeNodes.Clear();
        }

        // Beam Search için
        if (beamSearchPathfinder != null)
        {
            beamSearchPathfinder.visitedNodes.Clear();
        }
    }
    public void UpdateAStarParameters(AStarProperties props)
    {
        aStarPathfinder.props = props;
    }
    public List<Vector3> FindPath(SimulationManager.PathfindingAlgorithm algorithm, Vector3 startPos, Vector3 targetPos)
    {
        float startTime = Time.realtimeSinceStartup;
        List<Vector3> path = null;

        switch (algorithm)
        {
            case SimulationManager.PathfindingAlgorithm.AStar:
                path = aStarPathfinder.FindPath(startPos, targetPos);
                break;
            case SimulationManager.PathfindingAlgorithm.HillClimbing:
                path = hillClimbingPathfinder.FindPath(startPos, targetPos);
                break;
            case SimulationManager.PathfindingAlgorithm.RRT:
                path = rrtPathfinder.FindPath(startPos, targetPos);
                break;
            case SimulationManager.PathfindingAlgorithm.BeamSearch:
                path = beamSearchPathfinder.FindPath(startPos, targetPos);
                break;
        }
        float executionTime = (Time.realtimeSinceStartup - startTime);

        if (dataCollector != null && path != null)
        {
            float pathLength = CalculatePathLength(path);
            float pathCost = CalculatePathCost(path);
            float heightDifference = CalculateHeightDifference(path);
            int nodesExplored = GetNodesExplored(algorithm);

            uiManager?.UpdateStatistics(pathLength, executionTime, nodesExplored, pathCost, heightDifference);


            dataCollector.CollectPathData(
                algorithm,
                startPos,
                targetPos,
                path,
                executionTime,
                nodesExplored,
                pathLength,
                pathCost,
                heightDifference
            );
       
        }
        return path;
    }
    private float CalculatePathLength(List<Vector3> path)
    {
        float length = 0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            length += Vector3.Distance(path[i], path[i + 1]);
        }
        return length;
    }

    private float CalculatePathCost(List<Vector3> path)
    {
        float cost = 0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            float distance = Vector3.Distance(path[i], path[i + 1]);
            float heightDiff = Mathf.Abs(path[i + 1].y - path[i].y);
            cost += distance + heightDiff * 1.5f;
        }
        return cost;
    }

    private float CalculateHeightDifference(List<Vector3> path)
    {
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        foreach (var point in path)
        {
            minHeight = Mathf.Min(minHeight, point.y);
            maxHeight = Mathf.Max(maxHeight, point.y);
        }

        return maxHeight - minHeight;
    }

    private int GetNodesExplored(SimulationManager.PathfindingAlgorithm algorithm)
    {
        // Her algoritma için ayrý implement edilebilir
        switch (algorithm)
        {
            case SimulationManager.PathfindingAlgorithm.AStar:
                return aStarPathfinder.closedSet.Count;
            case SimulationManager.PathfindingAlgorithm.BeamSearch:
                return beamSearchPathfinder.visitedNodes.Count;
            case SimulationManager.PathfindingAlgorithm.HillClimbing:
                return hillClimbingPathfinder.visitedNodes.Count;
            case SimulationManager.PathfindingAlgorithm.RRT:
                return rrtPathfinder.treeNodes.Count;
            default:
                return 0;
        }
    }

    // Algoritma parametrelerini güncelleme metodlarý
    public void UpdateBeamSearchParameters(BeamSearchPathfinder.Properties props)
    {
        beamSearchPathfinder.props = props;
    }

    public void UpdateHillClimbingParameters(HillClimbingPathfinder.Properties props)
    {
        hillClimbingPathfinder.props = props;
    }

    public void UpdateRRTParameters(RRTPathfinder.Properties props)
    {
        rrtPathfinder.props = props;
    }
}
