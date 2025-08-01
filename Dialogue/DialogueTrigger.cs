using UnityEngine;
using System.Collections;

/// <summary>
/// 클릭 시 contentReference에 지정된 Dialogue를 재생하고,
/// invokeSceneEnd가 켜져 있으면 재생이 끝난 뒤 씬 엔드 대사 → 페이드 아웃 → 다음 씬 로드를 실행합니다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour
{
    [Tooltip("클릭 시 재생할 대화 데이터 (ScriptableObject)")]
    public Dialogue contentReference;   // ← 여기에 변수를 선언하세요

    [Header("씬 종료 대사 자동 호출")]
    [Tooltip("이 트리거로 마지막 대사를 재생한 후 Scene End 대사를 호출할지 여부")]
    public bool invokeSceneEnd;

    [Tooltip("씬 시작/종료 대사를 관리하는 DialogStorage 컴포넌트")]
    public DialogStorage dialogStorage;

    [Header("씬 종료 대사 인덱스")]
    [Tooltip("dialogStorage.sceneEndDialogues / nextScenes 리스트의 인덱스")]
    public int endIndex = 0;  // 이 값을 인스펙터에서 0 또는 1 등으로 설정하세요

    private void OnMouseDown()
    {
        if (contentReference == null)
            return;

        // 1) 클릭된 대사 재생
        DialogueManager.Instance.StartDialogue(contentReference);

        // 2) 재생 후 씬 엔드 대사 자동 호출
        if (invokeSceneEnd && dialogStorage != null)
            StartCoroutine(HandleSceneEndAfterDialogue());
    }

    private IEnumerator HandleSceneEndAfterDialogue()
    {
        // 대사가 완전히 끝날 때까지 대기
        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive);

        // 씬 엔드 대사 → 페이드 아웃 → 다음 씬 로드
        dialogStorage.PlayEndAndLoadNext(endIndex);
    }
}
