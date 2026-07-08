using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class AutoParkController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI statusText;

    [Header("Refs")]
    public Transform egoRoot;      // 움직일 차 (최상위 부모)
    public Transform entryPoint;   // 0번 키 리셋 위치

    [Header("Obstacle Cars (move only by scenario setup)")]
    public Transform car1;
    public Transform car2;
    public Transform car3;


    [System.Serializable]
    public struct CarSet
    {
        public Transform car1Pose;
        public Transform car2Pose;
        public Transform car3Pose;
    }

    [Header("Scenario car poses (1~4)")]
    public CarSet s1;
    public CarSet s2;
    public CarSet s3;
    public CarSet s4;

    [Header("Scenarios (parents)")]
    public Transform waypointsRoot1;
    public Transform waypointsRoot2;
    public Transform waypointsRoot3;
    public Transform waypointsRoot4;

    [Header("Move")]
    public float speed = 1.4f;
    public float turnSpeed = 6.0f;
    public float arriveDist = 0.05f;

    [Header("Path Visual")]
    public float lineY = 0.02f;    // 바닥에서 살짝 띄우기

    LineRenderer line;
    Coroutine routine;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.enabled = false;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit0Key.wasPressedThisFrame)
            ResetToEntry();

        if (kb.digit1Key.wasPressedThisFrame) StartAutoPark(1);
        if (kb.digit2Key.wasPressedThisFrame) StartAutoPark(2);
        if (kb.digit3Key.wasPressedThisFrame) StartAutoPark(3);
        if (kb.digit4Key.wasPressedThisFrame) StartAutoPark(4);
    }


    CarSet GetCarSet(int s) => s switch
    {
        1 => s1,
        2 => s2,
        3 => s3,
        4 => s4,
        _ => s1
    };

    void ApplyCarSet(int scenario)
    {
        var set = GetCarSet(scenario);

        ApplyOne(car1, set.car1Pose);
        ApplyOne(car2, set.car2Pose);
        ApplyOne(car3, set.car3Pose);
    }

    void ApplyOne(Transform car, Transform pose)
    {
        if (!car) return;

        if (pose == null)
        {
            car.gameObject.SetActive(false);
            return;
        }

        car.gameObject.SetActive(true);
        car.position = pose.position;
        car.rotation = pose.rotation;
    }

    public void ResetToEntry()
    {
        if (egoRoot == null || entryPoint == null)
        {
            Debug.LogError("egoRoot / entryPoint 연결 안 됨");
            return;
        }

        if (routine != null) StopCoroutine(routine);
        routine = null;

        egoRoot.position = entryPoint.position;
        egoRoot.rotation = entryPoint.rotation;

        line.enabled = false;
        if (statusText) statusText.text = "<color=#34D399>●</color> AutoPark Ready";
    }

    // 1번 키: 경로 따라 이동
    public void StartAutoPark(int scenario)
    {
        if (egoRoot == null || entryPoint == null)
        {
            if (statusText) statusText.text = "<color=#EF4444>●</color> Error: Ego or Entry Point not set";
            return;
        }

        if (statusText) statusText.text = $"<color=#FBBF24>●</color> Running Scenario {scenario}...";

        ApplyCarSet(scenario);

        egoRoot.position = entryPoint.position;
        egoRoot.rotation = entryPoint.rotation;

        var wps = GetWaypointsByScenario(scenario);

        List<Transform> path = BuildPath(wps);
        if (path.Count < 2)
        {
            Debug.LogError("경로 포인트 부족");
            if (statusText) statusText.text = "<color=#EF4444>●</color> Failed: Not enough path points";
            return;
        }
        DrawPath(path);

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FollowPath(path));
    }

    Transform[] GetWaypointsByScenario(int scenario)
    {
        Transform root = scenario switch
        {
            1 => waypointsRoot1,
            2 => waypointsRoot2,
            3 => waypointsRoot3,
            4 => waypointsRoot4,
            _ => null
        };

        if (root == null) return null;

        var list = new List<Transform>();
        for (int i = 0; i < root.childCount; i++)
            list.Add(root.GetChild(i));

        // WP01, WP02... 이름이면 이렇게 정렬(선택)
        list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

        return list.ToArray();
    }

    List<Transform> BuildPath(Transform[] wps)
    {
        List<Transform> pts = new()
        {
            entryPoint
        };

        if (wps != null)
        {
            foreach (var w in wps)
            {
                if (w == null) continue;
                pts.Add(w);
            }
        }

        return pts;
    }

    void DrawPath(List<Transform> pts)
    {
        Vector3[] linePts = new Vector3[pts.Count];
        for (int i = 0; i < pts.Count; i++)
            linePts[i] = new Vector3(
                pts[i].position.x,
                egoRoot.position.y + lineY,
                pts[i].position.z
            );

        line.positionCount = linePts.Length;
        line.SetPositions(linePts);
        line.enabled = true;
    }

    IEnumerator FollowPath(List<Transform> pts)
    {
        for (int i = 1; i < pts.Count; i++)
        {
            Transform targetTf = pts[i];
            var wp = targetTf.GetComponent<AutoParkWaypoint>();
            bool reverse = wp != null && wp.reverse;

            Vector3 targetPos = new Vector3(targetTf.position.x, egoRoot.position.y, targetTf.position.z);

            while (Vector3.Distance(egoRoot.position, targetPos) > arriveDist)
            {
                Vector3 dir = targetPos - egoRoot.position;
                dir.y = 0f;

                if (dir.sqrMagnitude > 0.0001f)
                {
                    // 전진이면 목표를 바라봄, 후진이면 목표의 반대 방향을 바라봄
                    Vector3 faceDir = reverse ? -dir : dir;

                    Quaternion look = Quaternion.LookRotation(faceDir.normalized, Vector3.up);
                    egoRoot.rotation = Quaternion.Slerp(egoRoot.rotation, look, Time.deltaTime * turnSpeed);
                }

                egoRoot.position = Vector3.MoveTowards(egoRoot.position, targetPos, speed * Time.deltaTime);
                yield return null;
            }
            if (wp != null && wp.waitSeconds > 0f)
                yield return new WaitForSeconds(wp.waitSeconds);
        }

        // 마지막 스냅 (y 유지)
        Transform last = pts[^1];
        egoRoot.position = new Vector3(last.position.x, egoRoot.position.y, last.position.z);
        egoRoot.rotation = last.rotation;

        line.enabled = false;
        routine = null;
        if (statusText) statusText.text = "<color=#34D399>●</color> AutoPark Ready";
    }
}