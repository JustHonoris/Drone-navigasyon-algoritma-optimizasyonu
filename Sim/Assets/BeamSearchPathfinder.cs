using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class BeamSearchPathfinder
{
    private readonly GridManager gridManager;
    public HashSet<Node> visitedNodes;
    public int GetBeamWidth() => props.beamWidth;
    public int GetMaxIterations() => props.maxIterations;
    public float GetHeightWeight() => props.heightWeight;
    public struct Properties
    {
        public int beamWidth;      // Aynı anda değerlendirilen yol sayısı
        public int maxIterations;  // Maksimum iterasyon sayısı
        public float heightWeight; // Yükseklik değişimi ağırlığı
    }

    public Properties props = new Properties
    {
        beamWidth = 200,        // Artırıldı
        maxIterations = 40000,  // Artırıldı
        heightWeight = 1.0f
    };

    public BeamSearchPathfinder(GridManager gridManager)
    {
        this.gridManager = gridManager;
        visitedNodes = new HashSet<Node>();
    }

    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = gridManager.GetNodeFromWorldPoint(startPos);
        Node targetNode = gridManager.GetNodeFromWorldPoint(targetPos);

        if (!ValidateNodes(startNode, targetNode))
            return null;

        visitedNodes.Clear();
        List<PathCandidate> currentBeam = new List<PathCandidate>
        {
            new PathCandidate(new List<Node> { startNode }, 0)
        };

        for (int iteration = 0; iteration < props.maxIterations; iteration++)
        {
            if (currentBeam.Count == 0) break;

            // Mevcut beam'deki tüm yolları genişlet
            var newCandidates = new List<PathCandidate>();

            foreach (var candidate in currentBeam)
            {
                var lastNode = candidate.path[candidate.path.Count - 1];

                // Hedefe ulaştık mı?
                if (lastNode == targetNode)
                {
                    return ConvertToVectorPath(candidate.path);
                }

                // Komşuları değerlendir
                foreach (var neighbor in GetValidNeighbors(lastNode))
                {
                    var newPath = new List<Node>(candidate.path) { neighbor };
                    float score = EvaluatePath(newPath, targetNode);
                    newCandidates.Add(new PathCandidate(newPath, score));
                    visitedNodes.Add(neighbor);
                }
            }

            // En iyi beam_width kadar yolu seç
            currentBeam = newCandidates
                .OrderByDescending(c => c.score)
                .Take(props.beamWidth)
                .ToList();
        }

        return null;
    }

    private class PathCandidate
    {
        public List<Node> path;
        public float score;

        public PathCandidate(List<Node> path, float score)
        {
            this.path = path;
            this.score = score;
        }
    }

    private List<Node> GetValidNeighbors(Node node)
    {
        return node.Neighbors
            .Where(n => IsValidNeighbor(n, node))
            .OrderByDescending(n => n.WorldPosition.y) // Yukarı yönlü hareketlere öncelik ver
            .ToList();
    }

    private bool IsValidNeighbor(Node neighbor, Node current)
    {
        if (neighbor == null || !neighbor.Walkable || visitedNodes.Contains(neighbor))
            return false;

        Vector3 direction = neighbor.WorldPosition - current.WorldPosition;

        // Yükseklik değişimi kontrolü - daha makul bir limit
        float heightDifference = Mathf.Abs(neighbor.WorldPosition.y - current.WorldPosition.y);
        if (heightDifference > 1.5f) // Daha düşük bir limit
            return false;

        if (Physics.Raycast(current.WorldPosition, direction.normalized, direction.magnitude, gridManager.obstacleLayer))
            return false;

        return true;
    }

    private float EvaluatePath(List<Node> path, Node target)
    {
        if (path.Count == 0) return float.MinValue;

        Node lastNode = path[path.Count - 1];

        // Hedef ile olan toplam mesafe
        float totalDistance = Vector3.Distance(path[0].WorldPosition, target.WorldPosition);
        float currentDistance = Vector3.Distance(lastNode.WorldPosition, target.WorldPosition);

        // İlerleme yüzdesi
        float progressRatio = (totalDistance - currentDistance) / totalDistance;

        // Yatay ve dikey mesafeyi ayrı değerlendir
        Vector3 toTarget = target.WorldPosition - lastNode.WorldPosition;
        float horizontalDistance = new Vector2(toTarget.x, toTarget.z).magnitude;
        float verticalDistance = Mathf.Abs(toTarget.y);

        // Yükseklik değerlendirmesi - hedefin yüksekliğine göre
        float heightPenalty = 0;
        if (lastNode.WorldPosition.y > target.WorldPosition.y + 2f)
        {
            heightPenalty = (lastNode.WorldPosition.y - target.WorldPosition.y) * 0.5f;
        }

        // Path uzunluğu cezası
        float lengthPenalty = (path.Count * 0.1f);

        return progressRatio * 5f - (horizontalDistance + verticalDistance + heightPenalty + lengthPenalty);
    }


    private bool ValidateNodes(Node start, Node target)
    {
        if (start == null || target == null)
            return false;

        if (!start.Walkable || !target.Walkable)
            return false;

        return true;
    }

    private List<Vector3> ConvertToVectorPath(List<Node> path)
    {
        return path.Select(node => node.WorldPosition).ToList();
    }
}