using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroSceneController : MonoBehaviour
{
    [Header("페이드 설정")]
    [Tooltip("CanvasGroup 컴포넌트")]
    public CanvasGroup canvasGroup;
    [Tooltip("전체 페이드 인/아웃 시간 (초)")]
    public float fadeDuration = 1f;
    [Tooltip("로고 유지 시간 (초)")]
    public float displayDuration = 3f;
    [Tooltip("인트로 후 로드할 씬 이름")]
    public string nextSceneName = "MainMenu";

    private void Start()
    {
        // 시작하자마자 코루틴 실행
        StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        // 1) 페이드 인 (alpha 0 → 1)
        yield return StartCoroutine(Fade(0f, 1f));

        // 2) 로고 유지
        yield return new WaitForSeconds(displayDuration);

        // 3) 페이드 아웃 (alpha 1 → 0)
        yield return StartCoroutine(Fade(1f, 0f));

        // 4) 다음 씬 로드
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        canvasGroup.alpha = from;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
