using UnityEngine;
using UnityEngine.EventSystems;

public class DroneTargetController : MonoBehaviour
{
    [Header("References")]
    public SimulationManager simulationManager;
    public DroneController drone;
    public GameObject targetObject; // Hedef olarak kullanılacak obje

    [Header("Target Settings")]
    public bool autoTargetOnStart = false;  // Başlangıçta otomatik hedef belirleme
    public float updateInterval = 0f;      // Hedefi güncelleme aralığı (0 = güncelleme yok)
    public KeyCode startTargetingKey = KeyCode.Space; // Hedeflemeyi başlatma tuşu
    public KeyCode updateTargetKey = KeyCode.R;      // Hedefi güncelleme tuşu

    private bool isTargeting = false; // Hedefleme aktif mi?

    private void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object is not assigned!");
            return;
        }

        if (autoTargetOnStart)
        {
            StartTargeting();
        }
    }

    private void Update()
    {
        // Hedeflemeyi başlatma/durdurma
        if (Input.GetKeyDown(startTargetingKey))
        {
            if (!isTargeting)
            {
                StartTargeting();
            }
            else
            {
                StopTargeting();
            }
        }

        // Manuel hedef güncelleme
        if (Input.GetKeyDown(updateTargetKey) && isTargeting)
        {
            SetTargetFromObject();
        }
    }

    // Hedeflemeyi başlat
    public void StartTargeting()
    {
        isTargeting = true;
        SetTargetFromObject(); // İlk hedefi belirle

        // Periyodik güncelleme gerekiyorsa
        if (updateInterval > 0)
        {
            InvokeRepeating("SetTargetFromObject", updateInterval, updateInterval);
        }

        Debug.Log("Targeting started!");
    }

    // Hedeflemeyi durdur
    public void StopTargeting()
    {
        isTargeting = false;
        if (updateInterval > 0)
        {
            CancelInvoke("SetTargetFromObject");
        }
        Debug.Log("Targeting stopped!");
    }

    // Hedef objenin pozisyonunu kullanarak yeni hedef belirle
    public void SetTargetFromObject()
    {
        if (targetObject != null && simulationManager != null && drone != null)
        {
            Vector3 targetPosition = targetObject.transform.position;

            // Hedef grid sınırları içinde mi kontrol et
            if (simulationManager.IsPositionInGrid(targetPosition))
            {
                simulationManager.TestPathFinding(drone, targetPosition);
            }
            else
            {
                Debug.LogWarning("Target object is outside the grid!");
            }
        }
    }

    // Debug için gizmo çizimi
    private void OnDrawGizmos()
    {
        if (drone != null && targetObject != null && isTargeting)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(drone.transform.position, targetObject.transform.position);
        }
    }
}