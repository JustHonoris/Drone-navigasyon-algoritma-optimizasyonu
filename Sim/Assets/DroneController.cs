using UnityEngine;
using System.Collections.Generic;

public class DroneController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 120f;
    public float waypointThreshold = 0.5f;

    private List<Vector3> path;
    private int currentWaypointIndex;
    private bool pathFound = false;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    // Yolu ayarlamak için public metod
    private void Start()
    {
        // Baþlangýç konumunu kaydet
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }
    public void ResetDrone()
    {
        // Pozisyon ve rotasyonu sýfýrla
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // Yolu temizle
        path = null;
        currentWaypointIndex = 0;
        pathFound = false;
    }
    public void SetPath(List<Vector3> newPath)
    {
        if (newPath != null && newPath.Count > 0)
        {
            path = new List<Vector3>(newPath); // Defensive copy
            currentWaypointIndex = 0;
            pathFound = true;
        }
        else
        {
            pathFound = false;
        }
    }

    private void Update()
    {
        if (!pathFound || path == null || currentWaypointIndex >= path.Count)
            return;

        Vector3 targetPosition = path[currentWaypointIndex];
        Vector3 moveDirection = targetPosition - transform.position;

        // Waypoint'e yeterince yakýnsa bir sonrakine geç
        if (moveDirection.magnitude < waypointThreshold)
        {
            currentWaypointIndex++;
            return;
        }

        // Hareket
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // Dönüþ
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            // X rotasýný sabit tutarak, sadece Y ve Z rotasýný döndür
            targetRotation.x = transform.rotation.x;

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    // Gizmos ile yolu görselleþtir
    void OnDrawGizmos()
    {
        if (path != null && path.Count > 0)
        {
            // Yolu çiz
            Gizmos.color = Color.green;
            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
            }

            // Mevcut hedefi göster
            if (currentWaypointIndex < path.Count)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(path[currentWaypointIndex], waypointThreshold);
            }
        }
    }
}