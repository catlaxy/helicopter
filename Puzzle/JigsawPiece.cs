using UnityEngine;
using System;

/// <summary>
/// 직쏘 퍼즐의 개별 조각을 나타내며,
/// 마우스 드래그로 움직이고 스냅 조건을 만족하면 정답 위치에 고정됩니다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class JigsawPiece : MonoBehaviour
{
    [Header("정답 위치")]
    [Tooltip("조각이 스냅될 대상 Transform")]
    public Transform targetPosition;

    [Header("스냅 거리 허용값")]
    [Tooltip("이 거리 이내로 이동 시 자동으로 스냅됩니다.")]
    public float snapThreshold = 0.5f;

    public bool IsInCorrectPosition { get; private set; } = false;
    public event Action OnPieceSnapped;

    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 originalPos;
    private bool isInitialized = false;
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    /// <summary>
    /// 퍼즐이 시작될 때 조각을 초기 위치로 되돌리고, 스냅 상태를 해제합니다.
    /// 최초 호출 시 현재 위치를 원위치로 저장합니다.
    /// </summary>
    public void ResetPiece()
    {
        if (!isInitialized)
        {
            originalPos = transform.position;
            isInitialized = true;
        }

        transform.position = originalPos;
        IsInCorrectPosition = false;
    }

    void OnMouseDown()
    {
        Debug.Log($"[JigsawPiece] OnMouseDown 호출됨: {gameObject.name}");
        if (IsInCorrectPosition) return;

        offset = transform.position - GetMouseWorldPos();
        isDragging = true;
    }

    void OnMouseDrag()
    {
        if (!isDragging || IsInCorrectPosition) return;
        transform.position = GetMouseWorldPos() + offset;
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        float dist = Vector3.Distance(transform.position, targetPosition.position);
        if (dist <= snapThreshold)
        {
            transform.position = targetPosition.position;
            IsInCorrectPosition = true;
            OnPieceSnapped?.Invoke();
        }
        else
        {
            transform.position = originalPos;
        }
    }

    /// <summary>
    /// 현재 마우스의 월드 좌표를 반환합니다.
    /// </summary>
    private Vector3 GetMouseWorldPos()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = cam.WorldToScreenPoint(transform.position).z;
        return cam.ScreenToWorldPoint(screenPos);
    }
}
