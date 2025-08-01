using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// 하이드 씬의 전체 흐름을 제어하는 컨트롤러입니다.
/// 인스펙터에서 각 스팟의 성공 여부를 체크하여 숨기 성공/실패를 판단합니다.
/// </summary>
public class HideSceneController : MonoBehaviour
{
    [Header("1) 페이드인 설정")]
    public FadeManager fadeManager;

    [Header("2) 인트로 스프라이트 설정")]
    public Sprite introSprite;
    public SpriteRenderer introSpriteRenderer;

    [Header("3) 진동 설정")]
    public float vibrationDuration = 0.5f;
    public float vibrationMagnitude = 5f;

    [Header("4) 타이머 UI 설정")]
    public Slider timerSlider;
    public GameObject timerUI;
    public float timerDuration = 5f;

    [Header("5) 숨기 스팟 리스트")]
    [Tooltip("각 스팟의 Collider와 성공 여부를 설정하세요.")]
    public List<HideSpotEntry> hideSpots = new List<HideSpotEntry>();

    [Header("6) 대사·다음 씬 매칭")]
    public DialogStorage dialogStorage;

    private void Start()
    {
        if (fadeManager == null)
            StartCoroutine(IntroAndHideSequence());
        else
            fadeManager.FadeIn(() => StartCoroutine(IntroAndHideSequence()));
    }

    /// <summary>
    /// 인트로 연출 → 진동 → 타이머 → 숨기 검사 → 엔딩 처리 순서대로 실행합니다.
    /// </summary>
    private IEnumerator IntroAndHideSequence()
    {
        // 1) 인트로 스프라이트 표시
        if (introSpriteRenderer != null && introSprite != null)
        {
            introSpriteRenderer.sprite = introSprite;
            introSpriteRenderer.gameObject.SetActive(true);
        }

        // 2) 진동 및 흔들기 연출
        yield return StartCoroutine(PlayVibrationEffectOn(introSpriteRenderer?.transform));

        if (introSpriteRenderer != null)
            introSpriteRenderer.gameObject.SetActive(false);

        // 3) 타이머 초기화
        if (timerSlider != null)
        {
            timerSlider.minValue = 0f;
            timerSlider.maxValue = timerDuration;
            timerSlider.value = timerDuration;
        }
        if (timerUI != null) timerUI.SetActive(true);

        // 4) 카운트다운 및 숨기 검사
        float elapsed = 0f;
        bool hidden = false;
        while (elapsed < timerDuration)
        {
            float dt = Time.deltaTime;
            elapsed += dt;

            // 남은 시간 기준으로 핸들 이동
            if (timerSlider != null)
                timerSlider.value = Mathf.Clamp(timerDuration - elapsed, 0f, timerDuration);

            // 각 스팟 검사: 성공 여부 토글 확인
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            foreach (var entry in hideSpots)
            {
                if (entry.collider != null && entry.collider.bounds.Contains(pos))
                {
                    hidden = entry.isSuccessSpot;
                    break;
                }
            }
            if (hidden) break;

            yield return null;
        }

        if (timerUI != null)
            timerUI.SetActive(false);

        // 5) 결과에 따라 엔딩 처리
        int index = hidden ? 0 : 1;
        StartCoroutine(HandleEnding(index));
    }

    /// <summary>
    /// 인덱스에 맞는 대사를 재생하고, 끝난 뒤 해당 씬으로 전환합니다.
    /// </summary>
    private IEnumerator HandleEnding(int index)
    {
        // 대사 재생
        if (dialogStorage.sceneEndDialogues.Count > index)
            DialogueManager.Instance.StartDialogue(dialogStorage.sceneEndDialogues[index]);

        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive);

        // 씬 전환
        if (dialogStorage.nextScenes.Count > index)
            SceneManager.LoadScene(dialogStorage.nextScenes[index]);
    }

    /// <summary>
    /// 모바일 진동 호출 및 UI 흔들기 연출을 수행합니다.
    /// </summary>
    private IEnumerator PlayVibrationEffectOn(Transform target)
    {
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
        if (target != null)
        {
            Vector3 origin = target.localPosition;
            float t = 0f;
            while (t < vibrationDuration)
            {
                float x = Random.Range(-1f, 1f) * vibrationMagnitude;
                float y = Random.Range(-1f, 1f) * vibrationMagnitude;
                target.localPosition = origin + new Vector3(x, y, 0f);
                t += Time.deltaTime;
                yield return null;
            }
            target.localPosition = origin;
        }
        else
        {
            yield return new WaitForSeconds(vibrationDuration);
        }
    }
}

/// <summary>
/// 인스펙터에서 편집 가능한 숨기 스팟 데이터 구조체입니다.
/// collider: 숨을 장소, isSuccessSpot: 성공 여부 토글
/// </summary>
[System.Serializable]
public class HideSpotEntry
{
    [Tooltip("숨을 장소로 사용할 Collider2D")]
    public Collider2D collider;

    [Tooltip("이 스팟에서 숨으면 성공 처리할지 여부")]
    public bool isSuccessSpot;
}
