using UnityEngine;
using System.Collections.Generic;
public class RRTPathfinder
{
    private readonly GridManager gridManager;
    public List<Node> treeNodes;
    private Dictionary<Node, Node> nodeParents;
    public float GetStepSize() => props.stepSize;
    public float GetGoalBias() => props.goalBias;
    public int GetMaxIterations() => props.maxIterations;
    public float GetMinDistance() => props.minDistance;
    public struct Properties
    {
        public float stepSize;
        public float goalBias;
        public int maxIterations;
        public float minDistance;
    }

    public Properties props = new Properties
    {
        stepSize = 2f,
        goalBias = 0.3f,  // Artýrýldý
        maxIterations = 2000, // Artýrýldý
        minDistance = 1.5f
    };

    public RRTPathfinder(GridManager gridManager)
    {
        this.gridManager = gridManager;
        treeNodes = new List<Node>();
        nodeParents = new Dictionary<Node, Node>();
    }

    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = gridManager.GetNodeFromWorldPoint(startPos);
        Node targetNode = gridManager.GetNodeFromWorldPoint(targetPos);

        if (!ValidateNodes(startNode, targetNode))
            return null;

        treeNodes.Clear();
        nodeParents.Clear();
        treeNodes.Add(startNode);

        for (int i = 0; i < props.maxIterations; i++)
        {
            // Hedef noktaya doðrudan ulaþabiliyorsak
            if (CanReachDirectly(treeNodes[treeNodes.Count - 1], targetNode))
            {
                nodeParents[targetNode] = treeNodes[treeNodes.Count - 1];
                return BuildPath(startNode, targetNode);
            }

            Vector3 randomPoint = GetRandomPoint(targetPos);
            Node nearestNode = FindNearestNode(randomPoint);
            Node newNode = TryCreateNode(nearestNode, randomPoint);

            if (newNode != null && !treeNodes.Contains(newNode))
            {
                treeNodes.Add(newNode);
                nodeParents[newNode] = nearestNode;

                if (Vector3.Distance(newNode.WorldPosition, targetPos) < props.minDistance &&
                    CanReachDirectly(newNode, targetNode))
                {
                    nodeParents[targetNode] = newNode;
                    return BuildPath(startNode, targetNode);
                }
            }
        }

        return null;
    }
    private bool CanReachDirectly(Node from, Node to)
    {
        Vector3 direction = to.WorldPosition - from.WorldPosition;
        float distance = direction.magnitude;
        return !Physics.Raycast(from.WorldPosition, direction.normalized, distance, gridManager.obstacleLayer);
    }

    private Vector3 GetRandomPoint(Vector3 targetPos)
    {
        if (Random.value < props.goalBias)
            return targetPos;

        return new Vector3(
            Random.Range(gridManager.MinX, gridManager.MaxX),
            Random.Range(gridManager.MinY, gridManager.MaxY),
            Random.Range(gridManager.MinZ, gridManager.MaxZ)
        );
    }

    private Node FindNearestNode(Vector3 point)
    {
        Node nearest = null;
        float minDist = float.MaxValue;

        foreach (Node node in treeNodes)
        {
            float dist = Vector3.Distance(node.WorldPosition, point);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = node;
            }
        }

        return nearest;
    }

    private Node TryCreateNode(Node fromNode, Vector3 toPoint)
    {
        Vector3 direction = (toPoint - fromNode.WorldPosition).normalized;
        float distance = Mathf.Min(props.stepSize, Vector3.Distance(fromNode.WorldPosition, toPoint));
        Vector3 newPos = fromNode.WorldPosition + direction * distance;

        Node newNode = gridManager.GetNodeFromWorldPoint(newPos);
        if (newNode != null && newNode.Walkable && !Physics.Raycast(fromNode.WorldPosition, direction, distance, gridManager.obstacleLayer))
        {
            return newNode;
        }
        return null;
    }
    private bool ValidateNodes(Node start, Node target)
    {
        return start != null && target != null &&
               start.Walkable && target.Walkable;
    }
    private List<Vector3> BuildPath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.WorldPosition);
            currentNode = nodeParents[currentNode];
        }

        path.Add(startNode.WorldPosition);
        path.Reverse();
        return path;
    }
}
