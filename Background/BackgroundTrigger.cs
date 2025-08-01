using UnityEngine;

[RequireComponent(typeof(Collider2D))]
/// <summary>
/// 클릭 가능한 트리거를 관리하여, 지정된 배경에서만
/// BackgroundManager.SwitchBackground을 호출합니다.
/// - 페이드/대사 재생 중엔 클릭을 무시  
/// - 트리거를 누를 때마다 전환이 가능하도록 isLocked 제거  
/// </summary>
public class BackgroundTrigger : MonoBehaviour
{
    [Tooltip("트리거가 반응할 배경 ID (BackgroundManager에 등록된 키)")]
    public string requiredBackgroundId;

    [Tooltip("전환할 배경 ID (BackgroundManager.SwitchBackground 호출)")]
    public string targetBackgroundId;

    /// <summary>
    /// Collider2D를 통해 클릭(혹은 터치) 감지 시 호출됩니다.
    /// 1) 페이드 중이면 무시  
    /// 2) 대사 재생 중이면 무시  
    /// 3) 현재 배경이 requiredBackgroundId와 다르면 무시  
    /// 4) 조건 만족 시 SwitchBackground 호출  
    /// </summary>
    private void OnMouseDown()
    {
        if (FadeManager.Instance != null && FadeManager.Instance.IsFading)
            return;

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            return;

        BackgroundManager bgManager = FindFirstObjectByType<BackgroundManager>();
        if (bgManager == null)
            return;

        if (bgManager.CurrentId != requiredBackgroundId)
            return;

        if (!string.IsNullOrEmpty(targetBackgroundId))
        {
            bgManager.SwitchBackground(targetBackgroundId);
        }
    }
}
