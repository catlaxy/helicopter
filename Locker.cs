// Locker.cs
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// “잠금장치” 역할을 하는 범용 컴포넌트.
/// requiredItemId에 맞는 아이템을 Interaction 드롭으로 받으면 Unlock()을 실행합니다.
/// </summary>
public class Locker : MonoBehaviour
{
    [Header("잠긴 상태일 때 보여줄 오버레이 UI")]
    [Tooltip("잠금 해제 전까지 화면 위를 덮는 GameObject")]
    public GameObject lockOverlay;

    [Header("잠금 해제 시 실행할 이벤트")]
    [Tooltip("잠금이 풀렸을 때 호출할 동작(문 열기, UI 켜기 등)")]
    public UnityEvent OnUnlocked;

    // 내부 잠금 상태 플래그
    private bool isLocked = true;

    private void Start()
    {
        // 시작 시 기본 상태 로그 출력
        Debug.Log($"[Locker] Start() - Initial isLocked={isLocked}");
        lockOverlay.SetActive(isLocked);
    }

    /// <summary>
    /// Interaction 스크립트의 OnInteraction 이벤트에 연결하세요.
    /// </summary>
    public void Unlock()
    {
        Debug.Log("[Locker] Unlock() called");
        // 이미 풀려 있으면 무시
        if (!isLocked)
        {
            Debug.Log("[Locker] Already unlocked, ignoring.");
            return;
        }

        // 잠금 해제
        isLocked = false;
        lockOverlay.SetActive(false);
        Debug.Log("[Locker] Unlocked - overlay hidden, invoking OnUnlocked event.");
        OnUnlocked?.Invoke();
    }

    /// <summary>
    /// 필요하다면 외부에서 잠금을 다시 걸 때 사용하세요.
    /// </summary>
    public void Lock()
    {
        Debug.Log("[Locker] Lock() called");
        if (isLocked)
        {
            Debug.Log("[Locker] Already locked, ignoring.");
            return;
        }

        isLocked = true;
        lockOverlay.SetActive(true);
        Debug.Log("[Locker] Locked - overlay shown.");
    }
}
