/*using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 퓨즈 스위치 오브젝트에 부착합니다.
/// - 클릭으로 전력 ON/OFF 토글
/// - 특정 오브젝트 활성화 시 토글 불가 안내
/// - ON 상태에서 와이어 Interaction 시 안내
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FuseSwitch : MonoBehaviour
{
    [Header("▶ 초기 상태")]
    [Tooltip("true면 ON, false면 OFF로 시작")]
    [SerializeField] private bool isOn = true;

    [Header("▶ 토글 불가 조건용")]
    [Tooltip("이 중 하나라도 활성화 시 토글 불가")]
    [SerializeField] private List<GameObject> blockingObjects;

    [Header("▶ 잠금 해제 조건")]
    [SerializeField] private GameObject unlockConditionObject;   // 이 오브젝트가 활성화돼 있을 때만 잠금 해제
    [SerializeField] private Locker targetLocker;               // 해제할 Locker 컴포넌트

    [Header("▶ 대화 데이터")]
    [Tooltip("토글 불가 시 출력할 대화")]
    [SerializeField] private Dialogue toggleFailDialogue;
    [Tooltip("ON 상태에서 와이어 작업 시 출력할 대화")]
    [SerializeField] private Dialogue wireFailDialogue;

    [Header("▶ 퍼즈박스 제어 참조")]
    [Tooltip("FuseBoxController 참조")]
    [SerializeField] private FuseBoxController fuseBoxController;

    private void Awake()
    {
        if (fuseBoxController == null)
            Debug.LogError("FuseSwitch: FuseBoxController가 할당되지 않았습니다.");
    }

    /// <summary>
    /// 클릭 시 호출됩니다.
    /// blockingObjects 중 하나라도 활성화면 대화 출력 후 리턴,
    /// 아니면 상태 토글하고 OFF→ON 시 잠금 해제 시도.
    /// </summary>
    /// <summary>
    /// 클릭으로 전력 ON/OFF를 토글하는 퓨즈 스위치입니다.
    /// - 특정 조건 충족 시 Locker 잠금 해제
    /// - 전원 상태에 따라 다른 대사 출력
    /// </summary>
    private void OnMouseDown()
    {
        // 1) 토글 불가 조건 체크
        foreach (var obj in blockingObjects)
        {
            if (obj != null && obj.activeSelf)
            {
                // 조건 충족 시 토글 실패 대사 출력 후 종료
                DialogueManager.Instance.StartDialogue(toggleFailDialogue);
                return;
            }
        }

        // 2) 현재 상태 저장 (토글 전)
        bool wasOff = !isOn;

        // 3) 전력 상태 토글 (ON <-> OFF)
        isOn = !isOn;
        Debug.Log($"[FuseSwitch] 전력 상태 변경됨: {(isOn ? "ON" : "OFF")}");

        // 4) FuseBoxController에도 토글 전달 (외부 시스템 동기화)
        fuseBoxController.TogglePower();

        // 5) OFF → ON으로 전환된 경우에만 Locker 해제 시도
        if (wasOff && isOn)
        {
            if (unlockConditionObject != null && unlockConditionObject.activeSelf)
            {
                targetLocker.Unlock();
                Debug.Log("[FuseSwitch] 잠금장치 해제 완료 (조건 충족)");
            }
        }
    }

    /// <summary>
    /// 와이어 Interaction 시 반드시 이 메서드를 호출하세요.
    /// ON 상태이면 대화만 출력하고 false 반환,
    /// OFF 상태면 true 반환하여 정상 진행할 수 있습니다.
    /// </summary>
    /// <returns>OFF 상태면 true</returns>
    public bool CanInteractWire()
    {
        if (isOn)
        {
            DialogueManager.Instance.StartDialogue(wireFailDialogue);
            return false;
        }
        return true;
    }
}*/