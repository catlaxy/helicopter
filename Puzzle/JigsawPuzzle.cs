using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 여러 개의 월드 기반 직소 퍼즐 그룹을 순차 실행하고, 
/// 마지막 그룹 완료 시 스프라이트를 표시한 후 씬 종료 대사를 재생합니다.
/// PuzzleBase를 상속합니다.
/// </summary>
public class JigsawPuzzle : PuzzleBase
{
    [Header("퍼즐 그룹 리스트 (Jigsaw1, Jigsaw2, …)")]
    [Tooltip("각 그룹은 자식으로 JigsawPiece(피스)와 Target(정답 위치)를 갖는 분류용 GameObject입니다.")]
    public List<GameObject> puzzleGroups;

    [Header("진행도 UI")]
    [Tooltip("퍼즐 그룹 하나를 완성할 때마다 1칸씩 채워집니다.")]
    public Slider progressBar;

    [Header("완료 시 표시할 스프라이트")]
    [Tooltip("퍼즐이 모두 완료되면 활성화할 스프라이트 오브젝트(UI Image 또는 SpriteRenderer).")]
    public GameObject completionSpriteObject;

    [Header("씬 종료 대사 관리")]
    [Tooltip("씬 종료 대사를 재생할 DialogStorage 오브젝트를 할당합니다.")]
    public DialogStorage dialogStorage;
    public int endIndex = 0;  // 이 값을 인스펙터에서 0 또는 1 등으로 설정하세요

    // 현재 활성화된 퍼즐 그룹 인덱스
    private int currentGroupIndex = 0;
    // 현재 그룹에 속한 JigsawPiece 리스트
    private List<JigsawPiece> currentPieces = new List<JigsawPiece>();

    /// <summary>
    /// 퍼즐이 시작될 때 호출됩니다.
    /// PuzzleBase 기본 초기화 후,Slider와 스프라이트 객체를 초기 상태로 설정하고 첫 그룹을 활성화합니다.
    /// </summary>
    public override void StartPuzzle()
    {
        base.StartPuzzle();

        // 진행도 슬라이더 초기화 (그룹 수에 맞춰 최대값 설정)
        if (progressBar != null)
        {
            progressBar.maxValue = puzzleGroups.Count;
            progressBar.value = 0;
        }

        // 완료 스프라이트는 처음에 꺼둡니다.
        if (completionSpriteObject != null)
            completionSpriteObject.SetActive(false);

        // 첫 번째 그룹 활성화
        currentGroupIndex = 0;
        ActivateGroup(currentGroupIndex);
    }

    /// <summary>
    /// 지정된 인덱스의 퍼즐 그룹만 활성화하고,
    /// 해당 그룹의 모든 JigsawPiece를 찾아 리셋 & 이벤트를 구독합니다.
    /// </summary>
    /// <param name="index">활성화할 그룹의 인덱스</param>
    private void ActivateGroup(int index)
    {
        // 1) 모든 그룹 비활성화
        for (int i = 0; i < puzzleGroups.Count; i++)
            puzzleGroups[i].SetActive(i == index);

        // 2) 활성 그룹의 JigsawPiece 컴포넌트 수집
        currentPieces.Clear();
        puzzleGroups[index].GetComponentsInChildren<JigsawPiece>(true, currentPieces);

        // 3) 각 피스 초기화 및 OnPieceSnapped 이벤트 등록
        foreach (var piece in currentPieces)
        {
            piece.ResetPiece();
            piece.OnPieceSnapped += OnPieceSnapped;
        }
    }

    /// <summary>
    /// 퍼즐 조각 하나가 스냅될 때마다 호출됩니다.
    /// 그룹 내 모든 조각이 제자리에 있으면 그룹을 클리어 처리합니다.
    /// </summary>
    private void OnPieceSnapped()
    {
        // 아직 완성되지 않은 조각이 있으면 리턴
        foreach (var piece in currentPieces)
        {
            if (!piece.IsInCorrectPosition)
                return;
        }

        // 진행도 슬라이더 1칸 증가
        if (progressBar != null)
            progressBar.value += 1;

        Debug.Log($"Jigsaw Group {currentGroupIndex + 1} Cleared!");

        // 이벤트 언구독
        foreach (var piece in currentPieces)
            piece.OnPieceSnapped -= OnPieceSnapped;

        // 다음 그룹 또는 전체 완료 처리
        currentGroupIndex++;
        if (currentGroupIndex < puzzleGroups.Count)
        {
            ActivateGroup(currentGroupIndex);
        }
        else
        {
            // 1) 완료 스프라이트 표시
            if (completionSpriteObject != null)
                completionSpriteObject.SetActive(true);

            // 2) 씬 종료 대사 재생 (DialogStorage)
            if (dialogStorage != null)
                dialogStorage.PlayEndAndLoadNext(endIndex);

            // 3) PuzzleBase 종료 이벤트 호출 (필요 시 다른 로직 추가 가능)
            EndPuzzle(true);
        }
    }
}
