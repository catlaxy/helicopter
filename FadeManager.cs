using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// 화면 전체를 블랙(혹은 지정 색)으로 덮는 패널을 통해
/// 페이드 인·아웃을 관리하는 싱글톤 클래스입니다.
/// 기본 페이드와 배경 전환 전용 페이드 지속 시간을 따로 제공합니다.
/// </summary>
public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("페이드에 사용할 전체 화면 Image (Black)")]
    public Image fadeImage;

    [Header("Fade Durations")]
    [Tooltip("일반 페이드 지속 시간(초)")]
    public float fadeDuration = 1f;

    [Tooltip("배경 전환 시 사용할 페이드 지속 시간(초)")]
    public float backgroundFadeDuration = 1f;

    // 페이드 진행 상태 플래그 (true인 동안은 FadeIn/Out 중)
    private bool _isFading = false;
    /// <summary>
    /// 페이드가 진행 중인지 여부를 반환합니다.
    /// </summary>
    public bool IsFading => _isFading;

    /// <summary>
    /// Awake 시 싱글톤 설정 및 초기화 수행
    /// - 인스턴스 중복 방지
    /// - fadeImage 알파를 1로 설정하여 초기 페이드 상태 유지
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
        }
    }

    /// <summary>
    /// 일반 페이드 인(검정→투명)을 수행하고, 완료 시 콜백을 호출합니다.
    /// </summary>
    /// <param name="onComplete">페이드 인 완료 시 실행할 콜백</param>
    public void FadeIn(Action onComplete = null)
    {
        FadeIn(fadeDuration, onComplete);
    }

    /// <summary>
    /// 지정된 지속 시간으로 페이드 인을 수행합니다.
    /// 배경 전환 시에는 이 메서드를 이용해 별도 지속 시간을 적용합니다.
    /// </summary>
    /// <param name="duration">페이드 인에 사용할 지속 시간(초)</param>
    /// <param name="onComplete">페이드 인 완료 시 실행할 콜백</param>
    public void FadeIn(float duration, Action onComplete = null)
    {
        if (fadeImage == null)
        {
            onComplete?.Invoke();
            return;
        }
        StartCoroutine(CoFade(1f, 0f, duration, onComplete));
    }

    /// <summary>
    /// 일반 페이드 아웃(투명→검정)을 수행하고, 완료 시 콜백을 호출합니다.
    /// </summary>
    /// <param name="onComplete">페이드 아웃 완료 시 실행할 콜백</param>
    public void FadeOut(Action onComplete = null)
    {
        FadeOut(fadeDuration, onComplete);
    }

    /// <summary>
    /// 지정된 지속 시간으로 페이드 아웃을 수행합니다.
    /// 배경 전환 시에는 이 메서드를 이용해 별도 지속 시간을 적용합니다.
    /// </summary>
    /// <param name="duration">페이드 아웃에 사용할 지속 시간(초)</param>
    /// <param name="onComplete">페이드 아웃 완료 시 실행할 콜백</param>
    public void FadeOut(float duration, Action onComplete = null)
    {
        if (fadeImage == null)
        {
            onComplete?.Invoke();
            return;
        }
        StartCoroutine(CoFade(0f, 1f, duration, onComplete));
    }

    /// <summary>
    /// 알파를 from→to로 보간하며 페이드 효과를 줍니다.
    /// </summary>
    /// <param name="from">시작 알파 값(0~1)</param>
    /// <param name="to">목표 알파 값(0~1)</param>
    /// <param name="duration">페이드에 사용할 지속 시간(초)</param>
    /// <param name="onComplete">완료 시 호출할 콜백</param>
    private IEnumerator CoFade(float from, float to, float duration, Action onComplete)
    {
        _isFading = true;                                // ▶ 페이드 시작
        float elapsed = 0f;
        fadeImage.gameObject.SetActive(true);
        Color c = fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            c.a = Mathf.Lerp(from, to, t);
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;

        // 페이드 인(완전 투명)이면 이미지 숨김
        if (Mathf.Approximately(to, 0f))
            fadeImage.gameObject.SetActive(false);

        onComplete?.Invoke();                            // ▶ 콜백 실행
        _isFading = false;                               // ▶ 페이드 종료
    }
}
