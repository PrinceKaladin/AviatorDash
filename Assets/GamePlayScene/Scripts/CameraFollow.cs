using UnityEngine;

public class CameraFollowX : MonoBehaviour
{
    public Transform target;      
    public float smoothSpeed = 0.125f;  
    public Vector3 offset = new Vector3(0, 0, -10);  

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = new Vector3(target.position.x + offset.x, transform.position.y + offset.y, offset.z);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}