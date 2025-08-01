using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// JumpScare 씬에서 대화 블록을 순차적으로 타이핑하고  
/// 특정 블록을 점프스케어로 처리해  
/// 1) 이미지와 텍스트 진동  
/// 2) 블록별 개별 타이핑 속도  
/// 3) 점프스케어 블록의 텍스트 크기 조절  
/// 4) 블록 전환 딜레이 동안 텍스트 숨김  
/// 등을 Inspector에서 설정할 수 있는 컨트롤러입니다.
/// </summary>
public class JumpScareController : MonoBehaviour
{
    [System.Serializable]
    public class DialogueData
    {
        [Tooltip("대화 블록 텍스트 (여러 줄일 땐 '\\n' 사용)")]
        [TextArea(3, 6)]
        public string text;

        [Tooltip("이 블록과 함께 표시할 이미지 (UI Image에 할당)")]
        public Sprite image;

        [Tooltip("이 블록을 점프스케어로 처리할지 여부")]
        public bool isJumpScare;
    }

    [Header("JumpScare Blocks")]
    [Tooltip("순서대로 보여줄 대화 블록 리스트")]
    public List<DialogueData> dialogues = new List<DialogueData>();

    [Header("UI References")]
    [Tooltip("텍스트를 타이핑할 TextMeshProUGUI 컴포넌트")]
    public TextMeshProUGUI jumpScareText;
    [Tooltip("이미지를 보여줄 UI Image 컴포넌트")]
    public Image jumpScareImage;

    [Header("Flow Settings")]
    [Tooltip("일반 블록 글자별 타이핑 속도(초)")]
    public float typingSpeed = 0.02f;
    [Tooltip("점프스케어 블록 글자별 타이핑 속도(초)")]
    public float jumpScareTypingSpeed = 0.01f;
    [Tooltip("블록 전환 전 딜레이(초)")]
    public float blockDelay = 0.5f;
    [Tooltip("모든 블록 완료 후 로드할 씬 이름")]
    public string nextSceneName = "Day1";

    [Header("Text Size Settings")]
    [Tooltip("점프스케어 블록일 경우 적용할 텍스트 폰트 크기")]
    public float jumpScareFontSize = 48f;

    [Header("Vibration Settings")]
    [Tooltip("점프스케어 이미지·텍스트 진동 강도(픽셀)")]
    public float vibrationIntensity = 10f;
    [Tooltip("점프스케어 이미지·텍스트 진동 유지 시간(초)")]
    public float vibrationHoldTime = 1f;

    // 내부 상태
    private int dialogueIndex = 0;        // 다음 블록 인덱스
    private string[] lines;               // 현재 블록의 줄 텍스트 배열
    private int lineIndex = 0;            // 현재 줄 인덱스
    private bool isTyping = false;        // 타이핑 중 플래그
    private bool awaitingClick = false;   // 클릭 대기 플래그
    private bool isVibrating = false;     // 진동 중 플래그
    private bool currentIsJumpScare;      // 현재 블록이 점프스케어인지 여부
    private float defaultFontSize;        // 비점프스케어 블록용 기본 폰트 크기
    private Coroutine typingCoroutine;    // 타이핑 코루틴 핸들러

    /// <summary>
    /// 씬 시작 시:
    /// 1) 텍스트의 기본 폰트 크기를 저장  
    /// 2) 페이드 인 → 첫 블록 로드 및 첫 줄 표시
    /// </summary>
    void Start()
    {
        // 비점프스케어 블록에 돌아올 때 사용할 기본 폰트 크기 저장
        defaultFontSize = jumpScareText.fontSize;

        // 페이드 인 후 첫 블록 연출 시작
        FadeManager.Instance.FadeIn(() =>
        {
            LoadBlock();
            ShowNextLine();
        });
    }

    /// <summary>
    /// 다음 DialogueData를 읽어:
    /// - currentIsJumpScare 설정  
    /// - 텍스트 폰트 크기 적용  
    /// - 이미지 활성/비활성  
    /// - 텍스트 줄('\n') 분리  
    /// - 점프스케어 블록이면 VibrateBlock 실행  
    /// </summary>
    private void LoadBlock()
    {
        if (dialogueIndex >= dialogues.Count) return;

        var data = dialogues[dialogueIndex++];
        currentIsJumpScare = data.isJumpScare;

        // 폰트 크기 변경: 점프스케어 블록은 jumpScareFontSize, 아니면 기본 크기
        jumpScareText.fontSize = currentIsJumpScare
            ? jumpScareFontSize
            : defaultFontSize;

        // 이미지 세팅
        if (data.image != null)
        {
            jumpScareImage.sprite = data.image;
            jumpScareImage.gameObject.SetActive(true);
        }
        else
        {
            jumpScareImage.gameObject.SetActive(false);
        }

        // 텍스트 줄 단위 분리
        lines = data.text.Split('\n');
        lineIndex = 0;

        // 점프스케어 블록이면 이미지·텍스트 진동 코루틴 실행
        if (currentIsJumpScare)
            StartCoroutine(VibrateBlock());
    }

    /// <summary>
    /// 현재 줄을 한 글자씩 타이핑:
    /// - currentIsJumpScare에 따라 속도 선택  
    /// 완료 후 awaitingClick=true로 클릭 대기 상태 전환  
    /// </summary>
    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        awaitingClick = false;
        jumpScareText.gameObject.SetActive(true);
        jumpScareText.text = "";

        // 블록별 타이핑 속도 결정
        float speed = currentIsJumpScare
            ? jumpScareTypingSpeed
            : typingSpeed;

        foreach (char c in line)
        {
            jumpScareText.text += c;
            yield return new WaitForSeconds(speed);
        }

        isTyping = false;
        awaitingClick = true;
    }

    /// <summary>
    /// 줄 전환 로직:
    /// - 남은 줄이 있으면 TypeLine 호출  
    /// - 줄이 모두 끝나면 NextBlockWithDelay 호출  
    /// - 모든 블록 완료 시 페이드 아웃 → 씬 전환  
    /// </summary>
    private void ShowNextLine()
    {
        if (lines != null && lineIndex < lines.Length)
        {
            // 이전 타이핑만 정리 (진동은 독립)
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeLine(lines[lineIndex++]));
            return;
        }

        if (dialogueIndex < dialogues.Count)
        {
            StartCoroutine(NextBlockWithDelay());
        }
        else
        {
            FadeManager.Instance.FadeOut(() =>
            {
                SceneManager.LoadScene(nextSceneName);
            });
        }
    }

    /// <summary>
    /// blockDelay만큼 대기하는 동안 텍스트를 숨기고,
    /// 이후 다음 블록 로드 및 첫 줄 표시
    /// </summary>
    private IEnumerator NextBlockWithDelay()
    {
        // 블록 전환 딜레이 동안 텍스트 비활성화
        jumpScareText.gameObject.SetActive(false);
        yield return new WaitForSeconds(blockDelay);

        LoadBlock();
        ShowNextLine();
    }

    /// <summary>
    /// 이미지와 텍스트를 동시에 진동시키는 코루틴:
    /// - vibrationHoldTime 동안 랜덤 오프셋 적용  
    /// - 완료 시 원위치 복귀 및 isVibrating=false로 클릭 잠금 해제  
    /// </summary>
    private IEnumerator VibrateBlock()
    {
        isVibrating = true;
        var imgRT = jumpScareImage.rectTransform;
        var txtRT = jumpScareText.rectTransform;
        Vector3 imgOrig = imgRT.localPosition;
        Vector3 txtOrig = txtRT.localPosition;
        float elapsed = 0f;

        while (elapsed < vibrationHoldTime)
        {
            float x = (Random.value * 2f - 1f) * vibrationIntensity;
            float y = (Random.value * 2f - 1f) * vibrationIntensity;
            if (jumpScareImage.gameObject.activeSelf)
                imgRT.localPosition = imgOrig + new Vector3(x, y, 0f);
            txtRT.localPosition = txtOrig + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 진동 종료 후 원위치 복귀
        imgRT.localPosition = imgOrig;
        txtRT.localPosition = txtOrig;
        isVibrating = false;
    }

    /// <summary>
    /// 매 프레임 클릭 입력 처리:
    /// - awaitingClick && !isTyping && !isVibrating일 때만 클릭 허용
    /// </summary>
    void Update()
    {
        if (awaitingClick && !isTyping && !isVibrating && Input.GetMouseButtonDown(0))
        {
            awaitingClick = false;
            ShowNextLine();
        }
    }
}
