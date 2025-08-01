using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 드래그 가능한 아이콘을 지정된 영역 내에서 이동시키며,
/// 어떤 순서로든 콜라이더를 건드리면 대사를 순차적으로 1번→2번→... 순서대로 재생한 뒤 비활성화하고,
/// 대사 재생 중에는 드래그가 중지되며, 대사 종료 시 다시 드래그가 가능해지고,
/// 모든 타겟을 건드리면 퍼즐을 완료하는 퍼즐 스크립트입니다.
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class DragAndTouchPuzzle : PuzzleBase
{
    [Header("1) 드래그 아이콘 설정")]
    [Tooltip("드래그할 아이콘 Transform (스크립트가 붙은 GameObject일 경우 생략 가능)")]
    public Transform iconTransform;

    [Tooltip("아이콘이 드래그 가능한 영역을 지정하는 Collider2D (Bounds 기준)")]
    public Collider2D areaCollider;

    [Header("2) 타겟 설정")]
    [Tooltip("충돌 시 대사를 출력하고 비활성화할 타겟 리스트 (대사 순차 실행용)")]
    public List<TargetInfo> targets = new List<TargetInfo>();

    // 내부 변수: 드래그 처리용
    private Vector3 offset;
    private bool isDragging = false;
    private Vector3 iconStartPos;
    // 대사 재생 중 드래그 가능 여부
    private bool canDrag = true;
    // 출력할 다음 대사 인덱스 (0부터 시작)
    private int nextDialogueIndex;

    /// <summary>
    /// 퍼즐 시작 시 실행되는 메서드.
    /// 아이콘 위치 초기화, 타겟 상태 리셋, 대사 인덱스 및 드래그 가능 상태 초기화를 수행합니다.
    /// </summary>
    public override void StartPuzzle()
    {
        base.StartPuzzle(); // 공통 초기화

        // 아이콘 초기 위치 저장
        if (iconTransform == null)
            iconTransform = transform;
        iconStartPos = iconTransform.position;

        // 각 타겟 초기화
        foreach (var t in targets)
        {
            t.triggered = false;
            if (t.targetCollider != null)
                t.targetCollider.gameObject.SetActive(true);
        }

        // 대사 순차 실행 인덱스 초기화
        nextDialogueIndex = 0;
        // 드래그 가능 상태 초기화
        canDrag = true;
    }

    /// <summary>
    /// 마우스 클릭 시작 시 호출됩니다.
    /// 드래그 가능 상태 확인 후, 아이콘과 마우스 포지션 간 오프셋을 계산합니다.
    /// </summary>
    private void OnMouseDown()
    {
        if (!canDrag) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = iconTransform.position - new Vector3(mouseWorld.x, mouseWorld.y, iconTransform.position.z);
        isDragging = true;
    }

    /// <summary>
    /// 마우스 드래그 중에 호출됩니다.
    /// 드래그 가능 상태와 영역 내 제한을 확인하며 아이콘을 이동시킵니다.
    /// </summary>
    private void OnMouseDrag()
    {
        if (!canDrag || !isDragging || areaCollider == null) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 targetPos = new Vector3(mouseWorld.x, mouseWorld.y, iconTransform.position.z) + offset;

        // 영역 Bounds 내로 포지션 제한
        Bounds bounds = areaCollider.bounds;
        float x = Mathf.Clamp(targetPos.x, bounds.min.x, bounds.max.x);
        float y = Mathf.Clamp(targetPos.y, bounds.min.y, bounds.max.y);
        iconTransform.position = new Vector3(x, y, targetPos.z);
    }

    /// <summary>
    /// 마우스 버튼 해제 시 호출됩니다.
    /// 드래그 중지 처리를 합니다.
    /// </summary>
    private void OnMouseUp()
    {
        isDragging = false;
    }

    /// <summary>
    /// 드래그 아이콘이 타겟 콜라이더에 진입했을 때 호출되는 콜백입니다.
    /// 해당 타겟이 아직 처리되지 않았다면 대사 재생 및 처리 코루틴을 시작합니다.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        foreach (var t in targets)
        {
            if (!t.triggered && t.targetCollider == other)
            {
                StartCoroutine(HandleTargetTrigger(t));
                break;
            }
        }
    }

    /// <summary>
    /// 타겟 충돌 처리 코루틴: 드래그 중지, 대사 출력, 대사 종료 후 드래그 재개, 타겟 비활성화 등을 수행합니다.
    /// 모든 타겟 처리 완료 시 퍼즐을 종료합니다.
    /// </summary>
    private IEnumerator HandleTargetTrigger(TargetInfo t)
    {
        // 드래그 중지
        canDrag = false;
        isDragging = false;

        // 해당 타겟 처리 표시
        t.triggered = true;

        // 순차 대사 출력
        if (nextDialogueIndex < targets.Count && targets[nextDialogueIndex].dialogue != null)
        {
            DialogueManager.Instance.StartDialogue(targets[nextDialogueIndex].dialogue); // fileciteturn1file1
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive);
        }
        nextDialogueIndex++;

        // 충돌 대상 비활성화
        if (t.targetCollider != null)
            t.targetCollider.gameObject.SetActive(false);

        // 드래그 재개
        canDrag = true;

        // 모든 타겟 처리 완료 여부 확인
        bool allDone = true;
        foreach (var item in targets)
        {
            if (!item.triggered)
            {
                allDone = false;
                break;
            }
        }

        // 퍼즐 완료 시 EndPuzzle 호출
        if (allDone)
            EndPuzzle(true);
    }

    /// <summary>
    /// 퍼즐 종료 시 호출되는 메서드.
    /// 추가 후처리가 필요하면 override하여 구현할 수 있습니다.
    /// </summary>
    public override void EndPuzzle(bool success)
    {
        base.EndPuzzle(success);
        // TODO: 퍼즐 완료 후 추가 효과(사운드, UI 변경 등) 구현 가능
    }

    /// <summary>
    /// 타겟 정보(Target Collider와 대사)를 묶는 데이터 구조체입니다.
    /// </summary>
    [Serializable]
    public class TargetInfo
    {
        [Tooltip("충돌 시 체크할 Target Collider (Trigger 설정 필요)")]
        public Collider2D targetCollider;

        [Tooltip("대사 순차 실행에 사용할 Dialogue SO")]
        public Dialogue dialogue;

        [HideInInspector]
        public bool triggered;
    }
}
