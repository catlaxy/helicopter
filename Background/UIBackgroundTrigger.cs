using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI 버튼 클릭 시 BackgroundTrigger와 동일한 로직으로
/// BackgroundManager.SwitchBackground을 호출합니다.
/// - 페이드 중 또는 대사 중에는 전환하지 않음
/// - 현재 배경이 requiredBackgroundId와 다를 경우 무시
/// </summary>
[RequireComponent(typeof(Button))]
public class UIBackgroundTrigger : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("트리거가 반응할 현재 배경 ID (BackgroundManager에 등록된 키)")]
    public string requiredBackgroundId;

    [Tooltip("전환할 배경 ID (BackgroundManager.SwitchBackground 호출)")]
    public string targetBackgroundId;

    /// <summary>
    /// UI 클릭 이벤트 핸들러.
    /// 1) 페이드 중이면 무시  
    /// 2) 대사 재생 중이면 무시  
    /// 3) 현재 배경이 requiredBackgroundId와 다르면 무시  
    /// 4) 조건 만족 시 BackgroundManager.SwitchBackground 호출  
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 1) 페이드 중이면 클릭 무시
        if (FadeManager.Instance != null && FadeManager.Instance.IsFading)
            return;

        // 2) 대사 재생 중이면 클릭 무시
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            return;

        // 3) 배경 매니저 확인 및 현재 ID 비교
        BackgroundManager bgManager = Object.FindFirstObjectByType<BackgroundManager>();
        if (bgManager == null)
            return;

        if (bgManager.CurrentId != requiredBackgroundId)
            return;

        // 4) 배경 전환
        if (!string.IsNullOrEmpty(targetBackgroundId))
        {
            bgManager.SwitchBackground(targetBackgroundId);
        }
    }
}
