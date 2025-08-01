using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class ClickEnable : MonoBehaviour
{
    [Header("1) 대사 실행 설정")]
    [Tooltip("클릭했을 때 재생할 대화 데이터 (선택 사항)")]
    public Dialogue dialogue;

    [Tooltip("이 대사를 한 번만 출력하고 다시는 출력하지 않을지 여부 (저장X, 세션 내 1회 제한)")]
    public bool playOnce = false;

    [Tooltip("대사 고유 ID (playOnce가 true일 때 메모리 기준 중복 방지용)")]
    public string dialogueId;

    [Header("2) 대사 종료 후 씬 종료 여부")]
    public bool isEndDialogue = false;

    [Header("3) 씬 종료 대사 스토리지")]
    public DialogStorage dialogStorage;
    public int endIndex = 0;

    // ✅ 세션 내 중복 방지용 정적 메모리
    private static HashSet<string> triggeredIds = new HashSet<string>();

    private void OnMouseDown()
    {
        // 1) 한 번만 출력 옵션이 켜져 있고 이미 실행된 ID라면 무시
        if (playOnce && !string.IsNullOrEmpty(dialogueId))
        {
            if (triggeredIds.Contains(dialogueId))
            {
                Debug.Log($"[ClickEnable] 대사({dialogueId})는 이미 한 번 실행되어 무시됩니다.");
                return;
            }
        }

        // 2) 대사 재생
        if (dialogue != null)
        {
            DialogueManager.Instance.StartDialogue(dialogue);

            if (playOnce && !string.IsNullOrEmpty(dialogueId))
            {
                triggeredIds.Add(dialogueId); // ✅ 중복 방지용 등록
            }
        }

        // 3) 씬 종료 대사 실행 여부
        if (isEndDialogue && dialogStorage != null)
        {
            StartCoroutine(WaitAndPlayEndDialogue());
        }
    }

    private System.Collections.IEnumerator WaitAndPlayEndDialogue()
    {
        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive);
        dialogStorage.PlayEndAndLoadNext(endIndex);
    }
}
