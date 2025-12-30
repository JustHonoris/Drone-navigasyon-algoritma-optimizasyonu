using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Node
{
    // Core properties
    public Vector3Int GridPosition { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    public bool Walkable { get; set; } = true;

    // Pathfinding properties
    public float GCost { get; set; }
    public float HCost { get; set; }
    public float FCost => GCost + HCost;
    public Node ParentNode { get; set; }

    // Connections
    public List<Node> Neighbors { get; private set; }

    public Node(Vector3Int gridPos, Vector3 worldPos)
    {
        GridPosition = gridPos;
        WorldPosition = worldPos;
        Neighbors = new List<Node>();
        ResetPathfindingData();
    }

    public void ResetPathfindingData()
    {
        GCost = float.MaxValue;
        HCost = float.MaxValue;
        ParentNode = null;
    }

    public void SetNeighbors(List<Node> neighbors)
    {
        Neighbors = neighbors;
    }
}

public struct DroneReservation
{
    public int DroneId { get; set; }
    public float Height { get; set; }
    public float Duration { get; set; }
    public ReservationType Type { get; set; }
}

public enum ReservationType
{
    Temporary,
    Confirmed,
    Emergency
}