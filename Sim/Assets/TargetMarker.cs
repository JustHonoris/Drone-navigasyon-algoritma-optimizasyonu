using UnityEngine;

public class TargetMarker : MonoBehaviour
{
    public float hoverHeight = 0.5f;
    public float rotationSpeed = 50f;
    public float bounceSpeed = 2f;
    public float bounceHeight = 0.2f;

    private Vector3 basePosition;

    private void Start()
    {
        basePosition = transform.position;
    }

    private void Update()
    {
        // Marker'ı döndür ve zıplat
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        float newY = basePosition.y + hoverHeight +
                    Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;

        transform.position = new Vector3(
            transform.position.x,
            newY,
            transform.position.z
        );
    }
}

