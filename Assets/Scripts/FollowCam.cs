using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Transform follow;   // CamFollow
    public Transform lookAt;   // CamPivot
    public float posLerp = 10f;
    public float rotLerp = 10f;

    void LateUpdate()
    {
        if (!follow || !lookAt) return;

        transform.position = Vector3.Lerp(transform.position, follow.position, Time.deltaTime * posLerp);

        var dir = (lookAt.position - transform.position).normalized;
        var targetRot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotLerp);
    }
}