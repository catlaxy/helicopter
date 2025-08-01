using UnityEngine;

/// <summary>
/// 퍼즐 블록 컴포넌트 (상-우-하-좌 순서 연결)
/// </summary>
public class FuseBlock : MonoBehaviour
{
    [Header("그리드 좌표")]
    public Vector2Int gridPosition;

    [Header("고정 여부 (회전 불가)")]
    public bool isFixed = false;

    [Header("연결 상태 (상-우-하-좌 순서)")]
    public bool[] connections = new bool[4];

    private void OnMouseDown()
    {
        if (isFixed) return;
        RotateBlock();
    }

    public void RotateBlock()
    {
        // 시계 방향 90도 회전 (상-우-하-좌 → 좌-상-우-하 순환)
        bool last = connections[3];
        for (int i = 3; i > 0; i--)
            connections[i] = connections[i - 1];
        connections[0] = last;

        // 시각적 회전
        transform.Rotate(Vector3.forward, -90f);

        // 퍼즐 흐름 검사 자동 호출
        FindFirstObjectByType<FuseBoxPuzzle>()?.CheckCurrentFlow();
    }
}
