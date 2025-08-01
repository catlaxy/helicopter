using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// DialogueLine.text의 '\n'마다 한 패널로 분리해 순차 타이핑,
/// objectSprite / actorSprite(null 시 비활성) 처리.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialoguePanel;            // 대화 전체 패널
    public Image objectImage;                   // 사물 이미지
    public Image actorImage;                    // 화자 이미지
    public TextMeshProUGUI nameText;            // 화자 이름
    public TextMeshProUGUI dialogueText;        // 대사 텍스트
    public Button nextButton;                   // Next 버튼 (선택)

    public bool IsDialogueActive => dialoguePanel.activeSelf;

    // 내부에서 처리할, '서브라인' 단위 큐
    private class DisplayLine
    {
        public string text;
        public Sprite objectSprite;
        public Sprite actorSprite;
    }
    private Queue<DisplayLine> lineQueue;
    private bool isTyping = false;

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

    lineQueue = new Queue<DisplayLine>();
        dialoguePanel.SetActive(false);

        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextClicked);
    }

    /// <summary>
    /// Dialogue SO로부터, '\n' 단위로 잘라서 DisplayLine 큐에 등록
    /// </summary>
    public void StartDialogue(Dialogue dialogue)
    {
        // ▶ 대화 패널을 UI 최상단으로 올려서, lockOverlay 위에 렌더링되도록 합니다.
        dialoguePanel.transform.SetAsLastSibling();

        // 이름 세팅
        nameText.text = dialogue.speakerName;
        dialoguePanel.SetActive(true);

        // 큐 초기화
        lineQueue.Clear();

        foreach (var dl in dialogue.lines)
        {
            // 화자용 스프라이트 결정
            Sprite actorSpr = null;
            if (dl.objectSprite == null)
            {
                // expressions 리스트에서 키로 찾기
                var entry = dialogue.expressions.Find(e => e.key == dl.expressionKey);
                actorSpr = entry != null ? entry.sprite : dialogue.defaultActorSprite;
            }

            // text를 '\n'으로 나눠 서브라인으로 분리
            var subs = dl.text.Split('\n');
            foreach (var sub in subs)
            {
                lineQueue.Enqueue(new DisplayLine
                {
                    text = sub,
                    objectSprite = dl.objectSprite,
                    actorSprite = actorSpr
                });
            }
        }

        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        // 더 이상 출력할 줄이 없으면 대화 종료
        if (lineQueue.Count == 0)
        {
            dialoguePanel.SetActive(false);
            return;
        }

        // 한 줄 꺼내오기
        var cur = lineQueue.Dequeue();

        // 사물 이미지 모드
        if (cur.objectSprite != null)
        {
            objectImage.gameObject.SetActive(true);
            objectImage.sprite = cur.objectSprite;
            actorImage.gameObject.SetActive(false);
        }
        else
        {
            // 화자 이미지 모드
            objectImage.gameObject.SetActive(false);

            if (cur.actorSprite != null)
            {
                actorImage.gameObject.SetActive(true);
                actorImage.sprite = cur.actorSprite;
            }
            else
            {
                actorImage.gameObject.SetActive(false);
            }
        }

        // 타이핑 시작
        StopAllCoroutines();
        StartCoroutine(TypeLine(cur.text));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.02f);
        }
        isTyping = false;
    }

    private void OnNextClicked()
    {
        if (!isTyping)
            DisplayNextLine();
    }

    void Update()
    {
        if (dialoguePanel.activeSelf && !isTyping && Input.GetMouseButtonDown(0))
            DisplayNextLine();
    }
}
