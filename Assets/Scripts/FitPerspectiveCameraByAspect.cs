using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FitPerspectiveCameraByAspect : MonoBehaviour
{
    public float pcFOV = 60f;        // PC에서 보기 좋은 값
    public float mobileFOV = 80f;    // 모바일 전용
    public float pcHeight = 16f;
    public float mobileHeight = 16f;

    void Start()
    {
        var cam = GetComponent<Camera>();
        float aspect = (float)Screen.width / Screen.height;

        if (aspect < 1f) // 세로 화면 = 모바일
        {
            cam.fieldOfView = mobileFOV;
            transform.position = new Vector3(
                transform.position.x,
                mobileHeight,
                transform.position.z
            );
        }
        else // 가로 화면 = PC
        {
            cam.fieldOfView = pcFOV;
            transform.position = new Vector3(
                transform.position.x,
                pcHeight,
                transform.position.z
            );
        }
    }
}