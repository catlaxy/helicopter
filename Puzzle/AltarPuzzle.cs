using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 여러 개의 AltarSlot을 모니터링하다가,
/// 모든 제단에 올바른 아이템이 놓였을 때 락커를 해제하고
/// 씬 종료 대사를 재생하는 PuzzleBase 파생 클래스입니다.
/// </summary>
public class AltarPuzzle : PuzzleBase
{
    [Header("제단 슬롯 리스트")]
    [Tooltip("순서 없이, 모든 AltarSlot을 드래그로 연결하세요.")]
    public List<AltarSlot> altars;

    [Header("연동 락커")]
    [Tooltip("모두 맞았을 때 Unlock()을 호출할 Locker 컴포넌트를 연결하세요.")]
    public Locker locker;

    [Header("씬 종료 대사 관리")]
    [Tooltip("퍼즐 성공 시 대사를 재생할 DialogStorage를 할당하세요.")]
    public DialogStorage dialogStorage;
    public int endIndex = 0;  // 이 값을 인스펙터에서 0 또는 1 등으로 설정하세요

    [Header("퍼즐 성공 시 활성화할 오브젝트들")]
    [Tooltip("퍼즐 완료 시 자동으로 SetActive(true) 처리될 오브젝트들")]
    public GameObject[] objectsToActivate;


    private int currentGroupIndex = 0;

    /// <summary>
    /// 퍼즐 시작 시 한 번 호출됩니다.
    /// PuzzleBase 기본 초기화 후, 각 AltarSlot의 이벤트를 구독합니다.
    /// </summary>
    public override void StartPuzzle()
    {
        base.StartPuzzle();
        currentGroupIndex = 0;
        Debug.Log($"[DEBUG] currentGroupIndex 초기화됨: {currentGroupIndex}");

        foreach (var altar in altars)
        {
            altar.placedItem = null;
            altar.OnItemPlaced += HandleItemPlaced;
        }
    }

    /// <summary>
    /// 제단 중 하나에 아이템이 올려질 때마다 호출됩니다.
    /// 모든 제단에 아이템이 놓이고, ID가 모두 맞으면 락커 해제, 대사 재생, 퍼즐 종료합니다.
    /// </summary>
    /// <param name="slot">아이템이 놓인 AltarSlot</param>
    private void HandleItemPlaced(AltarSlot slot)
    {
        // 1) 아직 빈 제단이 있으면 대기
        foreach (var altar in altars)
            if (altar.placedItem == null)
                return;

        // 2) 모든 제단에 아이템이 놓였으니, ID 맞는지 검사
        foreach (var altar in altars)
            if (altar.placedItem.itemId != altar.requiredItemId)
                return; // 하나라도 틀리면 대기

        // 3) 모두 맞았으면 락커 해제
        if (locker != null)
            locker.Unlock();
        else
            Debug.LogWarning("[AltarPuzzle] locker 필드가 할당되지 않았습니다.");

        // 🔽 3-1) 오브젝트 활성화
        if (objectsToActivate != null)
        {
            foreach (var obj in objectsToActivate)
                if (obj != null) obj.SetActive(true);
        }

        // 4) 씬 종료 대사 재생
        if (dialogStorage != null)
            dialogStorage.PlayEndAndLoadNext(endIndex);
        else
            Debug.LogWarning("[AltarPuzzle] dialogStorage 필드가 할당되지 않았습니다.");

        // 5) 퍼즐 성공 로그 및 종료 이벤트
        Debug.Log("AltarPuzzle: 모든 제단이 올바른 아이템으로 채워졌습니다. 퍼즐 완료!");
        EndPuzzle(true);
    }

    /// <summary>
    /// 퍼즐이 끝나거나 오브젝트가 비활성화될 때 이벤트 구독을 해제합니다.
    /// </summary>
    private void OnDisable()
    {
        foreach (var altar in altars)
            altar.OnItemPlaced -= HandleItemPlaced;
    }
}
