using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GridManager : MonoBehaviour
{
    private Node[,,] grid;
    private Vector3 gridWorldSize;
    private Vector3 gridWorldPosition;
    public float nodeSize;
    private int gridSizeX, gridSizeY, gridSizeZ;

    public LayerMask obstacleLayer;
    public float MinX => gridWorldPosition.x;
    public float MaxX => gridWorldPosition.x + gridWorldSize.x;
    public float MinY => gridWorldPosition.y;
    public float MaxY => gridWorldPosition.y + gridWorldSize.y;
    public float MinZ => gridWorldPosition.z;
    public float MaxZ => gridWorldPosition.z + gridWorldSize.z;
    public void InitializeGrid(Vector3 worldSize, Vector3 worldPosition, float size)
    {
        gridWorldSize = worldSize;
        gridWorldPosition = worldPosition;
        nodeSize = size;

        gridSizeX = Mathf.RoundToInt(worldSize.x / nodeSize);
        gridSizeY = Mathf.RoundToInt(worldSize.y / nodeSize);
        gridSizeZ = Mathf.RoundToInt(worldSize.z / nodeSize);

        CreateGrid();
    }


    private void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY, gridSizeZ];

        // Create nodes
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    Vector3Int gridPos = new Vector3Int(x, y, z);
                    Vector3 worldPos = GridToWorld(gridPos);
                    bool walkable = !Physics.CheckSphere(worldPos, nodeSize * 0.4f, obstacleLayer);

                    grid[x, y, z] = new Node(gridPos, worldPos) { Walkable = walkable };
                }
            }
        }

        // Set neighbors
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    grid[x, y, z].SetNeighbors(GetNeighbors(x, y, z));
                }
            }
        }
    }

    private List<Node> GetNeighbors(int x, int y, int z)
    {
        List<Node> neighbors = new List<Node>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dy == 0 && dz == 0) continue;

                    int newX = x + dx;
                    int newY = y + dy;
                    int newZ = z + dz;

                    if (IsInBounds(newX, newY, newZ))
                    {
                        neighbors.Add(grid[newX, newY, newZ]);
                    }
                }
            }
        }

        return neighbors;
    }

    private bool IsInBounds(int x, int y, int z)
    {
        return x >= 0 && x < gridSizeX &&
               y >= 0 && y < gridSizeY &&
               z >= 0 && z < gridSizeZ;
    }

    public Vector3 GridToWorld(Vector3Int gridPos)
    {
        return gridWorldPosition + new Vector3(
            (gridPos.x + 0.5f) * nodeSize,
            (gridPos.y + 0.5f) * nodeSize,
            (gridPos.z + 0.5f) * nodeSize
        );
    }

    public Vector3Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - gridWorldPosition;
        return new Vector3Int(
            Mathf.FloorToInt(localPos.x / nodeSize),
            Mathf.FloorToInt(localPos.y / nodeSize),
            Mathf.FloorToInt(localPos.z / nodeSize)
        );
    }

    public Node GetNodeFromWorldPoint(Vector3 worldPos)
    {
        Vector3Int gridPos = WorldToGrid(worldPos);
        if (IsInBounds(gridPos.x, gridPos.y, gridPos.z))
        {
            return grid[gridPos.x, gridPos.y, gridPos.z];
        }
        return null;
    }
}

// Node havuzu için yardýmcý sýnýf


// Grid ayarlarý için yardýmcý struct
public struct GridSettings
{
    public Vector3 worldSize;
    public Vector3 worldPosition;
    public float nodeSize;
    public bool allowDiagonal;
    public LayerMask obstacleLayer;
    public int heightLevelCount;
    public float minHeight;
    public float maxHeight;
}

public class SpatialHashGrid3D
{
    private readonly Dictionary<int, List<Node>> cells;
    private readonly float cellSize;
    private readonly Vector3 gridSize;
    private readonly int heightLevels;

    public SpatialHashGrid3D(Vector3 size, float nodeSize, int heightLevelCount)
    {
        gridSize = size;
        cellSize = nodeSize * 2f; // Optimize edilmiþ hücre boyutu
        heightLevels = heightLevelCount;
        cells = new Dictionary<int, List<Node>>();
    }

    public void AddNode(Node node)
    {
        int hash = CalculateHash(node.WorldPosition);

        if (!cells.ContainsKey(hash))
        {
            cells[hash] = new List<Node>();
        }

        cells[hash].Add(node);
    }

    public void UpdateNode(Node node)
    {
        // Eski konumdan kaldýr ve yeni konuma ekle
        foreach (var cell in cells.Values)
        {
            cell.Remove(node);
        }
        AddNode(node);
    }

    public List<Node> GetNodesInRadius(Vector3 position, float radius)
    {
        List<Node> result = new List<Node>();
        HashSet<int> checkedCells = new HashSet<int>();

        // Yarýçap içindeki tüm hücreleri kontrol et
        int cellRadius = Mathf.CeilToInt(radius / cellSize);
        Vector3Int centerCell = GetCell(position);

        for (int x = -cellRadius; x <= cellRadius; x++)
        {
            for (int y = -cellRadius; y <= cellRadius; y++)
            {
                for (int z = -cellRadius; z <= cellRadius; z++)
                {
                    Vector3Int offset = new Vector3Int(x, y, z);
                    int hash = CalculateHash(centerCell + offset);

                    if (!checkedCells.Contains(hash) && cells.ContainsKey(hash))
                    {
                        checkedCells.Add(hash);
                        foreach (var node in cells[hash])
                        {
                            if (Vector3.Distance(position, node.WorldPosition) <= radius)
                            {
                                result.Add(node);
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    private int CalculateHash(Vector3 position)
    {
        Vector3Int cell = GetCell(position);
        return CalculateHash(cell);
    }

    private int CalculateHash(Vector3Int cell)
    {
        // Uzamsal hash fonksiyonu
        const int prime1 = 73856093;
        const int prime2 = 19349663;
        const int prime3 = 83492791;

        return ((cell.x * prime1) ^ (cell.y * prime2) ^ (cell.z * prime3)) % int.MaxValue;
    }

    private Vector3Int GetCell(Vector3 position)
    {
        return new Vector3Int(
            Mathf.FloorToInt(position.x / cellSize),
            Mathf.FloorToInt(position.y / cellSize),
            Mathf.FloorToInt(position.z / cellSize)
        );
    }
}