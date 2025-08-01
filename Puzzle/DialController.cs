using UnityEngine;

/// <summary>
/// 드래그 입력으로 다이얼을 회전시키는 컴포넌트입니다.
/// - OnMouseDown: 드래그 시작 위치 기록  
/// - OnMouseDrag: 마우스(또는 터치) 위치 변화만큼 회전  
/// - OnMouseUp: 가장 가까운 스텝에 스냅(snap) & 퍼즐 매니저에 값 알림  
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class DialController : MonoBehaviour
{
    [Header("다이얼 설정")]
    [Tooltip("다이얼의 최소 값 (예: 0)")]
    public int minValue = 0;
    [Tooltip("다이얼의 최대 값 (예: 9)")]
    public int maxValue = 9;
    [Tooltip("회전시킬 Transform (보통 자기 자신)")]
    public Transform dialTransform;
    [Tooltip("값 변경 시 알림을 받을 퍼즐 매니저")]
    public DialPuzzle puzzleManager;

    [Tooltip("페이드 인 종료 후 추가로 잠글 시간(초)")]
    public float postFadeLockDuration = 1f;


    [HideInInspector]
    public int currentValue;      // 현재 다이얼 스텝 값

    private float _anglePerStep;  // 한 단계당 각도 (360 / 단계수)
    private float _startAngle;    // 드래그 시작 시 pointer-to-center 각도
    private float _startRotation; // 드래그 시작 시 dialTransform.localEulerAngles.z
    private float _postFadeLockTimer = 0f;    // 내부 타이머


    /// <summary>
    /// Awake에서 단계별 회전 각도 계산 및 초기값 설정
    /// </summary>
    private void Awake()
    {
        int steps = maxValue - minValue + 1;
        _anglePerStep = 360f / steps;
        ResetDial();
    }
    private void Update()
    {
        // 타이머 카운트다운
        if (_postFadeLockTimer > 0f)
            _postFadeLockTimer -= Time.deltaTime;
    }
    /// <summary>
    /// BackgroundManager나 FadeManager에서 페이드 인이 끝났을 때
    /// 이 메서드를 호출해 추가 잠금 타이머를 돌립니다.
    /// </summary>
    public void StartPostFadeLock()
    {
        _postFadeLockTimer = postFadeLockDuration;
    }

    /// <summary>
    /// 다이얼을 초기 상태(minValue)로 리셋하고 시각(회전) 갱신
    /// </summary>
    public void ResetDial()
    {
        currentValue = minValue;
        float angle = -(currentValue - minValue) * _anglePerStep;
        dialTransform.localEulerAngles = new Vector3(0, 0, angle);
    }

    /// <summary>
    /// 마우스(또는 터치) 버튼을 누르는 순간 호출
    /// - 다이얼 중심과 포인터 간의 초기 각도 기록
    /// - 회전 시작 기준점 저장
    /// </summary>
    private void OnMouseDown()
    {
        if (FadeManager.Instance.IsFading                  // 페이드 중
         || DialogueManager.Instance.IsDialogueActive      // 대화 중
         || _postFadeLockTimer > 0f)                       // 페이드 직후 1초
            return;

        Vector2 screenCenter = Camera.main.WorldToScreenPoint(dialTransform.position);
        Vector2 pointerPos = Input.mousePosition;
        _startAngle = Mathf.Atan2(pointerPos.y - screenCenter.y, pointerPos.x - screenCenter.x) * Mathf.Rad2Deg;
        _startRotation = dialTransform.localEulerAngles.z;
    }

    /// <summary>
    /// 누른 상태로 드래그할 때마다 호출
    /// - 현재 포인터 각도 계산 → 시작 각도 차이만큼 회전 갱신
    /// </summary>
    private void OnMouseDrag()
    {
        if (FadeManager.Instance.IsFading
         || DialogueManager.Instance.IsDialogueActive
         || _postFadeLockTimer > 0f)
            return;

        Vector2 screenCenter = Camera.main.WorldToScreenPoint(dialTransform.position);
        Vector2 pointerPos = Input.mousePosition;
        float currentAngle = Mathf.Atan2(pointerPos.y - screenCenter.y, pointerPos.x - screenCenter.x) * Mathf.Rad2Deg;
        float deltaAngle = Mathf.DeltaAngle(_startAngle, currentAngle);
        dialTransform.localEulerAngles = new Vector3(0, 0, _startRotation + deltaAngle);
    }

    /// <summary>
    /// 드래그를 끝내는 순간 호출
    /// - 현 회전값을 가장 가까운 스텝으로 스냅  
    /// - currentValue 업데이트 → 퍼즐 매니저에 알림  
    /// </summary>
    private void OnMouseUp()
    {
        if (FadeManager.Instance.IsFading
         || DialogueManager.Instance.IsDialogueActive
         || _postFadeLockTimer > 0f)
            return;

        // 1) 현 회전값(음수 방향 회전 포함)에서 nearest step 계산
        float rotZ = dialTransform.localEulerAngles.z;
        // 보정: 0~360 사이로 맞추고, 음수로 회전했을 때도 -값 계산
        if (rotZ > 180f) rotZ -= 360f;
        float rawStep = -rotZ / _anglePerStep;
        int step = Mathf.RoundToInt(rawStep) + minValue;
        // 2) 범위 클램프 및 순환 처리
        int range = maxValue - minValue + 1;
        step = (step - minValue) % range;
        if (step < 0) step += range;
        currentValue = step + minValue;

        // 3) 스냅 회전
        float snapAngle = -(currentValue - minValue) * _anglePerStep;
        dialTransform.localEulerAngles = new Vector3(0, 0, snapAngle);

        // 4) 콘솔 로그(디버그)
        Debug.Log($"[DialController] {gameObject.name} snapped → {currentValue}");

        // 5) 퍼즐 매니저에 값 변경 알림
        puzzleManager?.OnDialValueChanged();
    }
}
