using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 pos = target.position + offset;
        pos.z = -10;  // kamera 2D wajib di belakang
        transform.position = pos;
    }
}
