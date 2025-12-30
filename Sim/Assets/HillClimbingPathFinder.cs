using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HillClimbingPathfinder
{
    private readonly GridManager gridManager;
    private List<Node> currentPath;
    public HashSet<Node> visitedNodes;
    private int stuckCount = 0;
    public int GetMaxAttempts() => props.maxAttempts;
    public float GetHeightWeight() => props.heightWeight;
    public int GetMaxRestarts() => props.maxRestarts;
    public float GetRandomJumpProbability() => props.randomJumpProbability;
    public struct Properties
    {
        public int maxAttempts;
        public float heightWeight;
        public int maxRestarts;
        public float randomJumpProbability;
    }

    public  Properties props = new Properties
    {
        maxAttempts = 50000,
        heightWeight = 1.0f,
        maxRestarts = 8,
        randomJumpProbability = 0.03f
    };

    public HillClimbingPathfinder(GridManager gridManager)
    {
        this.gridManager = gridManager;
        currentPath = new List<Node>();
        visitedNodes = new HashSet<Node>();
    }

    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = gridManager.GetNodeFromWorldPoint(startPos);
        Node targetNode = gridManager.GetNodeFromWorldPoint(targetPos);

        if (!ValidateNodes(startNode, targetNode))
            return null;

        currentPath.Clear();
        visitedNodes.Clear();
        stuckCount = 0;

        Node currentNode = startNode;
        currentPath.Add(currentNode);
        float totalDistance = Vector3.Distance(startPos, targetPos);

        for (int attempt = 0; attempt < props.maxAttempts; attempt++)
        {
            Node nextNode = FindNextNode(currentNode, targetNode, totalDistance);

            if (nextNode == null)
                break;

            if (nextNode == targetNode)
            {
                currentPath.Add(nextNode);
                return ConvertToVectorPath(currentPath);
            }

            float currentScore = EvaluateNode(nextNode, targetNode, totalDistance);

            if (Random.value < props.randomJumpProbability)
            {
                stuckCount++;
                if (stuckCount >= props.maxRestarts)
                    break;
            }

            currentPath.Add(nextNode);
            visitedNodes.Add(nextNode);
            currentNode = nextNode;
        }

        return null;
    }

    private Node FindNextNode(Node currentNode, Node targetNode, float totalDistance)
    {
        List<Node> candidates = GetValidNeighbors(currentNode);
        if (candidates.Count == 0) return null;

        // Sýkýþma durumunda rastgele seçim
        if (stuckCount >= props.maxRestarts && Random.value < props.randomJumpProbability)
        {
            return candidates[Random.Range(0, candidates.Count)];
        }

        // En iyi komþuyu bul
        return candidates
            .OrderByDescending(n => EvaluateNode(n, targetNode, totalDistance))
            .FirstOrDefault();
    }

    private List<Node> GetValidNeighbors(Node node)
    {
        return node.Neighbors
            .Where(n => IsValidNeighbor(n, node))
            .ToList();
    }

    private bool IsValidNeighbor(Node neighbor, Node current)
    {
        if (neighbor == null || !neighbor.Walkable || visitedNodes.Contains(neighbor))
            return false;

        Vector3 direction = neighbor.WorldPosition - current.WorldPosition;
        if (Physics.Raycast(current.WorldPosition, direction.normalized, direction.magnitude, gridManager.obstacleLayer))
            return false;

        return true;
    }

    private float EvaluateNode(Node node, Node target, float totalDistance)
    {
        Vector3 toTarget = target.WorldPosition - node.WorldPosition;
        float currentDistance = toTarget.magnitude;

        // Hedefe olan mesafe skoru
        float distanceScore = 1 - (currentDistance / totalDistance);

        // Yükseklik farký skoru
        float heightDifference = Mathf.Abs(node.WorldPosition.y - target.WorldPosition.y);
        float heightScore = 1 - (heightDifference / totalDistance);

        // Yön skoru - sadece hedefe doðru olan yönelimi kontrol et
        float directionScore = 0;
        if (currentPath.Count > 0)
        {
            Vector3 currentDirection = (node.WorldPosition - currentPath[currentPath.Count - 1].WorldPosition).normalized;
            Vector3 targetDirection = toTarget.normalized;
            directionScore = Vector3.Dot(currentDirection, targetDirection);
        }

        // Toplam skor (directionScore'u da hesaba kat)
        return (distanceScore * 1.8f + heightScore * props.heightWeight + directionScore) / 3.8f;
    }

    private bool ValidateNodes(Node start, Node target)
    {
        if (start == null || target == null)
        {
            Debug.LogWarning("Start veya target node null!");
            return false;
        }

        if (!start.Walkable || !target.Walkable)
        {
            Debug.LogWarning("Start veya target node walkable deðil!");
            return false;
        }

        return true;
    }

    private List<Vector3> ConvertToVectorPath(List<Node> path)
    {
        return path.Select(node => node.WorldPosition).ToList();
    }
}