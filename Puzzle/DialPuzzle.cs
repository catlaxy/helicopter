using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// PuzzleBase를 상속 받아 3개의 다이얼로 구성된 금고 퍼즐을 관리합니다.
/// 정답 조합을 맞추면 다이얼이 잠기고, 금고 문 열기 애니메이션 후 씬 종료 대사를 출력하고 다음 씬으로 전환합니다.
/// </summary>
public class DialPuzzle : PuzzleBase
{
    [Tooltip("3개의 DialController를 순서대로 할당")]
    public List<DialController> dials;

    [Tooltip("정답 조합 (3자리 숫자)")]
    public List<int> correctCombination;

    [Header("금고 연출")]
    [Tooltip("금고 문 Animator 컴포넌트")]
    public Animator safeDoorAnimator;

    [Tooltip("금고 문 열기 트리거 이름")]
    public string openTrigger = "Open";

    [Header("애니메이션 후 씬 전환")]
    [Tooltip("금고 문 열기 애니메이션 재생 시간(초)")]
    public float openAnimationDuration = 1f;

    [Tooltip("씬 종료 대사 및 씬 전환을 관리하는 DialogStorage")]
    public DialogStorage dialogStorage;

    [Header("씬 종료 대사 인덱스")]
    [Tooltip("dialogStorage.sceneEndDialogues / nextScenes 리스트의 인덱스")]
    public int endIndex = 0;  // 이 값을 인스펙터에서 0 또는 1 등으로 설정하세요


    /// <summary>
    /// 퍼즐 시작 시 호출됩니다.
    /// 다이얼을 초기화하고 성공 플래그를 false로 리셋합니다.
    /// </summary>
    public override void StartPuzzle()
    {
        base.StartPuzzle();  // isSuccess = false
        foreach (var dial in dials)
        {
            dial.ResetDial();
        }
        Debug.Log("[DialPuzzle] 퍼즐 시작");
    }

    /// <summary>
    /// 다이얼 값이 변경될 때마다 호출됩니다.
    /// 정답 조합과 일치하면 퍼즐을 종료 처리합니다.
    /// </summary>
    public void OnDialValueChanged()
    {
        if (isSuccess)
            return;

        // 진행도 로그
        var prog = string.Join(",", dials.ConvertAll(d => d.currentValue.ToString()));
        Debug.Log($"[DialPuzzle] 현재값 → [{prog}]");

        // 다이얼 수와 정답 길이 일치 여부 검증
        if (dials.Count != correctCombination.Count)
        {
            Debug.LogWarning("[DialPuzzle] 다이얼 수와 정답 길이가 일치하지 않습니다.");
            return;
        }

        // 정답 조합 체크
        for (int i = 0; i < dials.Count; i++)
        {
            if (dials[i].currentValue != correctCombination[i])
                return;
        }

        // 정답일 경우 퍼즐 성공 처리
        Debug.Log("[DialPuzzle] 정답! 금고 열기 실행");
        EndPuzzle(true);
    }

    /// <summary>
    /// 퍼즐 종료 시 호출됩니다.
    /// 성공 시 다이얼 잠금, 금고 문 열기 애니메이션, 씬 종료 대사 및 다음 씬 로드를 순차적으로 수행합니다.
    /// </summary>
    public override void EndPuzzle(bool success)
    {
        if (success)
        {
            // 1) 다이얼 잠금: 스크립트와 콜라이더 비활성화
            foreach (var dial in dials)
            {
                dial.enabled = false;
                var col = dial.GetComponent<Collider2D>();
                if (col != null)
                    col.enabled = false;
            }

            // 2) 금고 문 열기 애니메이션
            if (safeDoorAnimator != null)
            {
                safeDoorAnimator.SetTrigger(openTrigger);
                StartCoroutine(HandleEndSequence());
            }
        }

        // 3) PuzzleBase.EndPuzzle 호출 (isSuccess 설정 및 이벤트 발동)
        base.EndPuzzle(success);
    }

    /// <summary>
    /// 금고 문 열기 애니메이션 종료 후 씬 종료 대사 및 씬 전환을 수행하는 코루틴입니다.
    /// </summary>
    private IEnumerator HandleEndSequence()
    {
        // 애니메이션 재생이 완료될 때까지 대기
        yield return new WaitForSeconds(openAnimationDuration);

        if (dialogStorage != null)
        {
            dialogStorage.PlayEndAndLoadNext(endIndex);
            Debug.Log("[DialPuzzle] 씬 종료 대사 재생 및 씬 전환 실행");
        }
        else
        {
            Debug.LogWarning("[DialPuzzle] DialogStorage가 할당되지 않아 씬 전환을 실행할 수 없습니다.");
        }
    }
}
