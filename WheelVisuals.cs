using UnityEngine;

public class WheelVisuals : MonoBehaviour
{
    public WheelCollider wheelCollider;
    public Transform wheelMesh;

    void Update()
    {
        if (wheelCollider == null || wheelMesh == null)
            return;

        wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
        wheelMesh.position = pos;
        wheelMesh.rotation = rot;
    }
}
