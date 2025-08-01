using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 게임오버 씬 제어 컨트롤러: 비명 → 배경 페이드 → 버튼 페이드 출력
/// </summary>
public class GameOverController : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource screamAudio;

    [Header("Background UI")]
    public CanvasGroup backgroundGroup;

    [Header("Button UI")]
    public CanvasGroup buttonGroup;

    [Header("Button Delay Settings")]
    public float delayAfterBackgroundFade = 3f;
    public float fadeDuration = 1f;

    private void Start()
    {
        // 배경과 버튼을 초기 비가시 상태로 설정
        backgroundGroup.alpha = 0f;
        backgroundGroup.interactable = false;
        backgroundGroup.blocksRaycasts = false;

        buttonGroup.alpha = 0f;
        buttonGroup.interactable = false;
        buttonGroup.blocksRaycasts = false;

        // 연출 시작
        StartCoroutine(GameOverSequence());
    }

    /// <summary>
    /// 비명 → 배경 페이드 → 대기 → 버튼 페이드 순으로 실행
    /// </summary>
    private IEnumerator GameOverSequence()
    {
        // 1) 비명 사운드 재생
        if (screamAudio != null)
        {
            screamAudio.Play();
            yield return new WaitWhile(() => screamAudio.isPlaying);
        }

        // 2) 배경 페이드 인
        yield return StartCoroutine(FadeInCanvasGroup(backgroundGroup, 1f, fadeDuration));

        // 3) 대기 시간
        yield return new WaitForSeconds(delayAfterBackgroundFade);

        // 4) 버튼 페이드 인
        yield return StartCoroutine(FadeInCanvasGroup(buttonGroup, 1f, fadeDuration));

        // 버튼 인터랙션 활성화
        buttonGroup.interactable = true;
        buttonGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// 지정된 CanvasGroup을 페이드 인하는 코루틴
    /// </summary>
    private IEnumerator FadeInCanvasGroup(CanvasGroup group, float targetAlpha, float duration)
    {
        float startAlpha = group.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        group.alpha = targetAlpha;
    }

    /// <summary>
    /// 다시하기 버튼 클릭 시, 이전 씬으로 복귀
    /// </summary>
    public void OnRetryButtonClicked()
    {
        string previousScene = SaveManager.Instance.lastSceneBeforeGameOver;
        if (!string.IsNullOrEmpty(previousScene))
        {
            SceneManager.LoadScene(previousScene);
        }
        else
        {
            Debug.LogWarning("이전 씬 이름이 비어있습니다.");
        }
    }

    /// <summary>
    /// 메인메뉴로 버튼 클릭 시
    /// </summary>
    public void OnMainMenuButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
