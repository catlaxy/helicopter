using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// 엔딩 트리거 컴포넌트입니다.
/// - 초기 대사 출력 및 오브젝트 토글
/// - 진동/소리 모드 분기
///   · 진동: 진동 효과 → 대사 → 씬 전환
///   · 소리: 진동+소리 효과 → 하이드 대사 → 하이드 이벤트 실행
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EndingTrigger : MonoBehaviour
{
    [Header("1) 초기 엔딩 대사")]
    public Dialogue initialEndingDialogue;

    [Header("2) 진동 모드")]
    public Dialogue aliveEndingDialogue;
    public string aliveEndingSceneName;

    [Header("3) 소리 모드")]
    public Dialogue hideDialogue; // 하이드 대사 (소리 모드)

    [Header("4) 오브젝트 토글")]
    public GameObject[] objectsToEnable;
    public GameObject[] objectsToDisable;

    [Header("5) 진동/소리 효과")]
    public GameObject phoneObject;
    public float vibrationDuration = 0.5f;
    public float vibrationMagnitude = 5f;

    [Header("6) 하이드 이벤트 설정")]
    public GameObject hideTimerSliderUI; // 슬라이더 UI
    public float hideDuration = 5f; // 제한 시간
    public GameObject vibratingSpriteObject; // 흔들릴 스프라이트
    public List<GameObject> validHideTargets; // 숨을 수 있는 대상 리스트
    public string missEndingSceneName; // 성공 시 씬
    public string deathEndingSceneName; // 실패 시 씬

    private bool isVibrate = false;
    private bool isEffectRunning = false;
    private bool isHiding = false;
    private Vector3 spriteOriginalPos;

    private void OnMouseDown()
    {
        if (initialEndingDialogue == null)
        {
            Debug.LogWarning("[EndingTrigger] 초기 대사가 지정되지 않았습니다.");
            return;
        }

        StartCoroutine(HandleEndingSequence());
    }

    /// <summary>
    /// 초기 대사 출력 → 오브젝트 토글 → 모드 분기
    /// </summary>
    private IEnumerator HandleEndingSequence()
    {
        // 오브젝트 토글
        foreach (var obj in objectsToEnable)
            if (obj != null) obj.SetActive(true);
        foreach (var obj in objectsToDisable)
            if (obj != null) obj.SetActive(false);

        // 초기 대사 출력
        DialogueManager.Instance.StartDialogue(initialEndingDialogue);
        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive);

        // 모드 확인
        isVibrate = SaveManager.Instance.IsVibrateMode();

        // 진동/소리 효과 루프 시작
        isEffectRunning = true;
        StartCoroutine(LoopPhoneEffect());
    }

    /// <summary>
    /// 지정된 폰 오브젝트를 진동시키는 루프
    /// </summary>
    private IEnumerator LoopPhoneEffect()
    {
        while (isEffectRunning)
        {
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
            if (phoneObject != null)
            {
                Vector3 original = phoneObject.transform.localPosition;
                float elapsed = 0f;

                while (elapsed < vibrationDuration)
                {
                    float x = Random.Range(-1f, 1f) * vibrationMagnitude;
                    float y = Random.Range(-1f, 1f) * vibrationMagnitude;
                    phoneObject.transform.localPosition = original + new Vector3(x, y, 0);

                    elapsed += Time.deltaTime;
                    yield return null;
                }

                phoneObject.transform.localPosition = original;
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    /// <summary>
    /// 진동 오브젝트 클릭 시 호출 (UI 버튼 또는 콜라이더로 연결)
    /// </summary>
    public void OnPhoneClicked()
    {
        if (!isEffectRunning) return;

        isEffectRunning = false;
        StartCoroutine(HandlePostEffectFlow());
    }

    /// <summary>
    /// 진동 모드 또는 소리 모드 분기 처리
    /// </summary>
    private IEnumerator HandlePostEffectFlow()
    {
        if (isVibrate)
        {
            // 진동 모드: 대사 → 씬 이동
            if (aliveEndingDialogue != null)
                DialogueManager.Instance.StartDialogue(aliveEndingDialogue);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive);

            if (!string.IsNullOrEmpty(aliveEndingSceneName))
                SceneManager.LoadScene(aliveEndingSceneName);
        }
        else
        {
            // 소리 모드: 하이드 대사 → 하이드 이벤트
            if (hideDialogue != null)
                DialogueManager.Instance.StartDialogue(hideDialogue);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive);

            StartCoroutine(StartHideEvent());
        }
    }

    /// <summary>
    /// 하이드 이벤트 시작: 슬라이더 + 스프라이트 진동 + 시간 제한
    /// </summary>
    private IEnumerator StartHideEvent()
    {
        isHiding = true;

        if (hideTimerSliderUI != null)
        {
            hideTimerSliderUI.SetActive(true);
            var slider = hideTimerSliderUI.GetComponent<UnityEngine.UI.Slider>();
            slider.maxValue = hideDuration;
            slider.value = hideDuration;

            float elapsed = 0f;
            spriteOriginalPos = vibratingSpriteObject != null ? vibratingSpriteObject.transform.localPosition : Vector3.zero;

            while (elapsed < hideDuration && isHiding)
            {
                elapsed += Time.deltaTime;
                slider.value = hideDuration - elapsed;

                // 진동 연출
                if (vibratingSpriteObject != null)
                {
                    float x = Random.Range(-1f, 1f) * vibrationMagnitude;
                    float y = Random.Range(-1f, 1f) * vibrationMagnitude;
                    vibratingSpriteObject.transform.localPosition = spriteOriginalPos + new Vector3(x, y, 0);
                }

                yield return null;
            }

            EndHideEvent(success: false);
        }
    }

    /// <summary>
    /// 숨기 이벤트 중 클릭 감지용 메서드입니다.
    /// 숨을 수 있는 오브젝트에 연결해주세요.
    /// </summary>
    /// <param name="target">클릭된 오브젝트</param>
    public void TryHide(GameObject target)
    {
        if (!isHiding) return;

        if (validHideTargets.Contains(target))
        {
            EndHideEvent(success: true);
        }
        else
        {
            EndHideEvent(success: false);
        }
    }

    /// <summary>
    /// 숨기 이벤트 종료 및 결과에 따른 씬 이동
    /// </summary>
    private void EndHideEvent(bool success)
    {
        isHiding = false;

        if (hideTimerSliderUI != null)
            hideTimerSliderUI.SetActive(false);

        if (vibratingSpriteObject != null)
            vibratingSpriteObject.transform.localPosition = spriteOriginalPos;

        string nextScene = success ? missEndingSceneName : deathEndingSceneName;
        if (!string.IsNullOrEmpty(nextScene))
            SceneManager.LoadScene(nextScene);
    }
}
