using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class AStarPathfinder
{
    private readonly GridManager gridManager;
    private readonly BinaryHeap<Node> openSet;
    public readonly HashSet<Node> closedSet;
    public AStarProperties props; // Yeni eklenen
    // Performans optimizasyonlarý için sabitlerimiz
    private const int MAX_ITERATIONS = 40000;
    private const float MAX_PATH_COST = 1000f;
    // AStarPathfinder.cs'e eklenecek
    public AStarProperties GetProperties()
    {
        return props;
    }
    public AStarPathfinder(GridManager gridManager)
    {
        this.gridManager = gridManager;
        this.openSet = new BinaryHeap<Node>();
        this.closedSet = new HashSet<Node>();
        this.props = new AStarProperties
        {
            HeightCostMultiplier = 2.0f,  // Önceki hard-coded deðer
            MaxHeightDifference = 1.5f    // Önceki hard-coded deðer
        };
    }

    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = gridManager.GetNodeFromWorldPoint(startPos);
        Node targetNode = gridManager.GetNodeFromWorldPoint(targetPos);

        if (!ValidateNodes(startNode, targetNode))
        {
            return null;
        }

        // Koleksiyonlarý temizle
        openSet.Clear();
        closedSet.Clear();

        // Baþlangýç node'unu hazýrla
        InitializeStartNode(startNode, targetNode);
        openSet.Add(startNode);

        int iterations = 0;

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            // Ýterasyon limiti kontrolü
            if (++iterations > MAX_ITERATIONS)
            {
                Debug.LogWarning("A* exceeded maximum iterations!");
                return null;
            }

            closedSet.Add(currentNode);

            foreach (Node neighbor in currentNode.Neighbors)
            {
                // Temel kontroller
                if (!IsValidNeighbor(neighbor, currentNode))
                    continue;

                // Yeni maliyet hesaplama
                float movementCost = CalculateMovementCost(currentNode, neighbor);
                float newGCost = currentNode.GCost + movementCost;

                // Maliyet limiti kontrolü
                if (newGCost > MAX_PATH_COST)
                    continue;

                if (newGCost < neighbor.GCost || !openSet.Contains(neighbor))
                {
                    // Node'u güncelle
                    UpdateNeighborNode(neighbor, currentNode, newGCost, targetNode);

                    // Open set'e ekle veya güncelle
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                    else
                    {
                        openSet.UpdateItem(neighbor);
                    }
                }
            }
        }

        return null;
    }

    private bool ValidateNodes(Node start, Node target)
    {
        if (start == null || target == null)
        {
            Debug.LogWarning("Start or target node is null!");
            return false;
        }

        if (!start.Walkable || !target.Walkable)
        {
            Debug.LogWarning("Start or target node is not walkable!");
            return false;
        }

        return true;
    }

    private void InitializeStartNode(Node startNode, Node targetNode)
    {
        startNode.GCost = 0;
        startNode.HCost = CalculateHeuristicCost(startNode, targetNode);
        startNode.ParentNode = null;
    }

    private bool IsValidNeighbor(Node neighbor, Node currentNode)
    {
        // Temel kontroller
        if (neighbor == null || !neighbor.Walkable || closedSet.Contains(neighbor))
            return false;

        // Komþu node'a geçiþ kontrolü
        Vector3 direction = neighbor.WorldPosition - currentNode.WorldPosition;
        float distance = direction.magnitude;

        // Engel kontrolü - Ray cast kullanarak engel tespiti
        if (Physics.Raycast(
            currentNode.WorldPosition,
            direction.normalized,
            distance,
            gridManager.obstacleLayer))
        {
            return false;
        }

        // Yükseklik farký kontrolü
        float heightDifference = Mathf.Abs(neighbor.WorldPosition.y - currentNode.WorldPosition.y);
        if (heightDifference > props.MaxHeightDifference) // Maksimum týrmanma yüksekliði
        {
            return false;
        }

        return true;
    }

    private float CalculateMovementCost(Node from, Node to)
    {
        Vector3 delta = to.WorldPosition - from.WorldPosition;

        // Temel mesafe
        float distance = delta.magnitude;

        // Yükseklik deðiþimi için ek maliyet
        float heightDifference = Mathf.Abs(to.WorldPosition.y - from.WorldPosition.y);
        float heightCost = heightDifference * props.HeightCostMultiplier; // Yükseklik deðiþimi maliyeti

        return distance + heightCost;
    }

    private float CalculateHeuristicCost(Node from, Node to)
    {
        Vector3 delta = to.WorldPosition - from.WorldPosition;

        // Manhattan distance yerine Euclidean distance kullan
        // çünkü 3D ortamda daha doðru sonuç veriyor
        return delta.magnitude;
    }

    private void UpdateNeighborNode(Node neighbor, Node current, float newGCost, Node targetNode)
    {
        neighbor.GCost = newGCost;
        neighbor.HCost = CalculateHeuristicCost(neighbor, targetNode);
        neighbor.ParentNode = current;
    }

    private List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;

        // Path'i geriye doðru oluþtur
        while (currentNode != startNode)
        {
            path.Add(currentNode.WorldPosition);
            currentNode = currentNode.ParentNode;

            // Sonsuz döngü kontrolü
            if (currentNode == null)
            {
                Debug.LogError("Path retracing failed - broken parent chain!");
                return null;
            }
        }

        path.Add(startNode.WorldPosition);
        path.Reverse();

        return path;
    }
}
public struct AStarProperties
{
    public float DiagonalCost { get; set; }
    public float HeightCostMultiplier { get; set; }
    public bool AllowDiagonal { get; set; }
    public float MaxHeightDifference { get; set; }
}

public class BinaryHeap<T> where T : Node
{
    private List<T> items;

    public int Count => items.Count;

    public BinaryHeap()
    {
        items = new List<T>();
    }

    public void Add(T item)
    {
        items.Add(item);
        SortUp(items.Count - 1);
    }

    public T RemoveFirst()
    {
        T firstItem = items[0];
        int lastIndex = items.Count - 1;
        items[0] = items[lastIndex];
        items.RemoveAt(lastIndex);

        if (items.Count > 0)
        {
            SortDown(0);
        }

        return firstItem;
    }

    public void UpdateItem(T item)
    {
        int index = items.IndexOf(item);
        SortUp(index);
    }

    public bool Contains(T item)
    {
        return items.Contains(item);
    }

    public void Clear()
    {
        items.Clear();
    }

    private void SortDown(int index)
    {
        while (true)
        {
            int leftChild = index * 2 + 1;
            int rightChild = index * 2 + 2;
            int swapIndex = 0;

            if (leftChild < items.Count)
            {
                swapIndex = leftChild;

                if (rightChild < items.Count && items[rightChild].FCost < items[leftChild].FCost)
                {
                    swapIndex = rightChild;
                }

                if (items[swapIndex].FCost < items[index].FCost)
                {
                    Swap(index, swapIndex);
                    index = swapIndex;
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }
    }

    private void SortUp(int index)
    {
        int parentIndex = (index - 1) / 2;

        while (true)
        {
            if (items[index].FCost < items[parentIndex].FCost)
            {
                Swap(index, parentIndex);
                index = parentIndex;
                parentIndex = (index - 1) / 2;
            }
            else
            {
                break;
            }

            if (index == 0)
            {
                break;
            }
        }
    }

    private void Swap(int indexA, int indexB)
    {
        T temp = items[indexA];
        items[indexA] = items[indexB];
        items[indexB] = temp;
    }
}