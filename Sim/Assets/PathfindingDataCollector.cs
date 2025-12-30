using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

public class PathfindingDataCollector : MonoBehaviour
{
    [Header("File Settings")]
    private string folderPath = @"C:\Users\onury\OneDrive\Desktop\Data";
    public string filePrefix = "pathfinding_analysis";
    public bool appendTimestamp = true;

    [Header("Collection Settings")]
    public bool autoSaveOnNewPath = true;
    public KeyCode manualSaveKey = KeyCode.F5;

    private string csvFilePath;
    private List<PathfindingTestData> collectedData;
    private static readonly string[] CSV_HEADERS = {
        "timestamp",               // ISO 8601 format
        "algorithm",              // Algorithm name as string
        "execution_time_ms",
        "nodes_explored",
        "path_length_m",
        "path_cost",
        "height_difference_m",
        "path_points",
        "success",
        "avg_segment_length_m"
    };

    private void Start()
    {
        Debug.Log("PathfindingDataCollector: Start initialized");
        InitializeDataCollection();
    }

    private void InitializeDataCollection()
    {
        collectedData = new List<PathfindingTestData>();

        try
        {
            Directory.CreateDirectory(folderPath);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = appendTimestamp ? $"{filePrefix}_{timestamp}.csv" : $"{filePrefix}.csv";
            csvFilePath = Path.Combine(folderPath, fileName);

            // Write headers
            File.WriteAllText(csvFilePath, string.Join(",", CSV_HEADERS) + "\n");
            Debug.Log($"CSV file initialized at: {csvFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in InitializeDataCollection: {e.Message}\n{e.StackTrace}");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(manualSaveKey))
        {
            SaveCollectedData();
        }
    }

    public void CollectPathData(
     SimulationManager.PathfindingAlgorithm algorithm,
     Vector3 startPos,
     Vector3 targetPos,
     List<Vector3> path,
     float executionTime,
     int nodesExplored,
     float pathLength,
     float pathCost,
     float heightDifference)
    {
        // Değerleri doğru formatta oluştur
        PathfindingTestData testData = new PathfindingTestData
        {
            Timestamp = DateTime.Now,
            Algorithm = algorithm,
            ExecutionTimeMs = executionTime * 1000f, // Saniyeyi milisaniyeye çevir
            NodesExplored = nodesExplored,
            PathLength = pathLength,
            PathCost = pathCost,
            HeightDifference = heightDifference,
            PathPointCount = path?.Count ?? 0,
            WasPathFound = path != null,
            AverageSegmentLength = path != null && path.Count > 1 ?
                pathLength / (path.Count - 1) : 0
        };

        collectedData.Add(testData);

        if (autoSaveOnNewPath)
        {
            AppendDataToCSV(testData);
        }
    }


    private void AppendDataToCSV(PathfindingTestData data)
    {
        try
        {
            string[] fields = {
            data.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss"),
            data.Algorithm.ToString(),
            ((int)data.ExecutionTimeMs).ToString(),  // Virgülsüz hali
            data.NodesExplored.ToString(),
            ((int)data.PathLength).ToString(),       // Virgülsüz hali
            ((int)data.PathCost).ToString(),         // Virgülsüz hali
            ((int)data.HeightDifference).ToString(), // Virgülsüz hali
            data.PathPointCount.ToString(),
            (data.WasPathFound ? "1" : "0"),
            ((int)data.AverageSegmentLength).ToString() // Virgülsüz hali
        };

            string line = string.Join(",", fields);
            File.AppendAllText(csvFilePath, line + "\n");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error appending data to CSV: {e.Message}");
        }
    }
    public void SaveCollectedData()
    {
        try
        {
            // Write headers
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Join(",", CSV_HEADERS));

            // Write data
            foreach (var data in collectedData)
            {
                string[] fields = {
                    data.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss"),
                    data.Algorithm.ToString(),  // Enum'u string olarak kaydet
                    data.ExecutionTimeMs.ToString("F3"),
                    data.NodesExplored.ToString(),
                    data.PathLength.ToString("F3"),
                    data.PathCost.ToString("F3"),
                    data.HeightDifference.ToString("F3"),
                    data.PathPointCount.ToString(),
                    data.WasPathFound ? "1" : "0",
                    data.AverageSegmentLength.ToString("F3")
                };
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText(csvFilePath, sb.ToString());
            Debug.Log($"Saved {collectedData.Count} records to: {csvFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving data: {e.Message}");
        }
    }

    private struct PathfindingTestData
{
    public DateTime Timestamp;
    public SimulationManager.PathfindingAlgorithm Algorithm;
    public float ExecutionTimeMs;      // Milisaniye cinsinden
    public int NodesExplored;          // Tamsayı
    public float PathLength;           // Metre cinsinden, 2 ondalık
    public float PathCost;             // 2 ondalık
    public float HeightDifference;     // Metre cinsinden, 2 ondalık
    public int PathPointCount;         // Tamsayı
    public bool WasPathFound;          // Boolean (0/1)
    public float AverageSegmentLength; // Metre cinsinden, 2 ondalık
}
}