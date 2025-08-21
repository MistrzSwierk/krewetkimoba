using UnityEngine;
using System.Collections;

public class CameraFollow_LoL : MonoBehaviour
{
    public Transform target;       // czempion
    public Vector3 offset = new Vector3(0, 7, -5); // typowy LoL offset
    public float smoothSpeed = 0.1f;
    public float scrollSpeed = 5f; // szybkość zoomu
    public float minY = 7f;        // minimalna wysokość kamery
    public float maxY = 27f;       // maksymalna wysokość kamery
    public float minZ = -5f;      // minimalna odległość z tyłu
    public float maxZ = -25f;      // maksymalna odległość z tyłu

    void LateUpdate()
    {
        if (target == null) return;

        // Obsługa scrolla myszy
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        offset.y -= scroll * scrollSpeed;
        offset.z += scroll * scrollSpeed; // odwrotna zależność, żeby kamera się oddalała
        offset.y = Mathf.Clamp(offset.y, minY, maxY);
        offset.z = Mathf.Clamp(offset.z, maxZ, minZ);

        // pozycja kamery
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // stały kąt patrzenia
        transform.rotation = Quaternion.Euler(50, 0, 0);
    }
}

