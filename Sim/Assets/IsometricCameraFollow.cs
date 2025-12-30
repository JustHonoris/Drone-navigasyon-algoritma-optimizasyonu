using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsometricCameraFollow : MonoBehaviour
{
    public Transform target; // Ýzometrik bakýþ açýsý için takip edilecek hedef
    public Vector3 offset = new Vector3(-10, 10, -10); // Kameranýn hedefe göre pozisyonu
    public float smoothSpeed = 0.125f; // Kameranýn yumuþak hareket hýzý
    public float rotationSpeed = 100f; // Kamera rotasyon hýzýný kontrol eder
     public float zoomSpeed = 5f; // Kamera yakýnlaþtýrma/uzaklaþtýrma hýzý
    public float minZoom = 5f; // Kamera için minimum yakýnlaþtýrma
    public float maxZoom = 20f; // Kamera için maksimum uzaklaþtýrma
    private float currentRotationY = 0f; // Mevcut Y rotasý

    void LateUpdate()
    {
        if (target != null)
        {
            // X ekseni için yakýnlaþtýrma/uzaklaþtýrma
            float horizontalInputX = Input.GetAxis("Horizontal"); // A/D ya da sol/sað ok tuþlarý
            offset.x = Mathf.Clamp(offset.x - horizontalInputX * zoomSpeed * Time.deltaTime, -maxZoom, maxZoom);

            // Y ekseni için yakýnlaþtýrma/uzaklaþtýrma
            float verticalInput = Input.GetAxis("Vertical"); // W/S ya da yukarý/aþaðý ok tuþlarý
            offset.y = Mathf.Clamp(offset.y - verticalInput * zoomSpeed * Time.deltaTime, minZoom, maxZoom);

            // Z ekseni için yakýnlaþtýrma/uzaklaþtýrma
            if (Input.GetKey(KeyCode.Q)) // Z ekseni için "Q" tuþu
            {
                offset.z = Mathf.Clamp(offset.z - zoomSpeed * Time.deltaTime, -maxZoom, maxZoom);
            }
            if (Input.GetKey(KeyCode.E)) // Z ekseni için "E" tuþu
            {
                offset.z = Mathf.Clamp(offset.z + zoomSpeed * Time.deltaTime, -maxZoom, maxZoom);
            }


            // Kamerayý istenen rotaya döndür
            Quaternion rotation = Quaternion.Euler(0, currentRotationY, 0);
            transform.rotation = rotation;

            // Hedef pozisyon ile ofseti toplayarak hedef konumu hesapla
            Vector3 desiredPosition = target.position + offset;

            // Kamera pozisyonunu yumuþak bir þekilde hedefe doðru taþý
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            // Kameranýn hedefe bakmasýný saðla
            transform.LookAt(target);
        }
    }
}
