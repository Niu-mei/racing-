using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 4.5f, -7.8f);
    public float followSpeed = 5f;
    public float lookSpeed = 6f;

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPos = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);

        Vector3 lookTarget = target.position + target.forward * 5f + Vector3.up * 1.5f;
        Quaternion desiredRot = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);

        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, lookSpeed * Time.deltaTime);
    }
}
