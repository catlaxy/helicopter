// PrologueController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DialogueData
{
    [Tooltip("대화 블록 텍스트 (여러 줄일 땐 '\\n' 사용)")]
    [TextArea(3, 6)]
    public string text;

    [Tooltip("이 대화 블록과 함께 표시할 이미지 (UI Image 에 할당)")]
    public Sprite image;
}

public class PrologueController : MonoBehaviour
{
    [Header("Dialogue Blocks")]
    [Tooltip("순서대로 보여줄 대화와 이미지 리스트")]
    public List<DialogueData> dialogues = new List<DialogueData>();

    [Header("UI References")]
    [Tooltip("텍스트를 타이핑할 TextMeshProUGUI 컴포넌트")]
    public TextMeshProUGUI prologueText;
    [Tooltip("이미지를 보여줄 UI Image 컴포넌트")]
    public Image prologueImage;

    [Header("Flow Settings")]
    [Tooltip("글자별 타이핑 속도(초)")]
    public float typingSpeed = 0.02f;
    [Tooltip("끝난 뒤 로드할 씬 이름")]
    public string nextSceneName = "Day1";

    // 현재 처리 중인 대화·줄 인덱스
    private int dialogueIndex = 0;
    private string[] lines;
    private int lineIndex = 0;

    // 상태 플래그
    private bool isTyping = false;
    private bool awaitingClick = false;

    void Start()
    {
        // 페이드인 후 첫 대화 블록 로드 및 첫 줄 표시
        FadeManager.Instance.FadeIn(() =>
        {
            LoadDialogueBlock();
            ShowNextLine();
        });
    }

    /// <summary>
    /// dialogues[dialogueIndex]를 읽어 lines 배열으로 분리하고,
    /// prologueImage에 해당 스프라이트를 세팅합니다.
    /// </summary>
    private void LoadDialogueBlock()
    {
        if (dialogueIndex >= dialogues.Count) return;

        // 이미지 세팅
        var sprite = dialogues[dialogueIndex].image;
        if (sprite != null)
        {
            prologueImage.sprite = sprite;
            prologueImage.gameObject.SetActive(true);
        }
        else
        {
            prologueImage.gameObject.SetActive(false);
        }

        // 텍스트 줄 단위 분리
        lines = dialogues[dialogueIndex].text.Split('\n');
        lineIndex = 0;
        dialogueIndex++;
    }

    /// <summary>
    /// 현재 lines[lineIndex]를 한 줄씩 타이핑 출력.
    /// 다 읽으면 다음 줄 또는 다음 블록으로 이동합니다.
    /// </summary>
    private void ShowNextLine()
    {
        // lines가 없거나 인덱스 초과 시
        if (lines == null || lineIndex >= lines.Length)
        {
            // 다음 블록이 남아있으면 로드 & 첫 줄
            if (dialogueIndex < dialogues.Count)
            {
                LoadDialogueBlock();
                ShowNextLine();
            }
            else
            {
                // 모두 끝나면 페이드아웃 → 씬 전환
                FadeManager.Instance.FadeOut(() =>
                {
                    SceneManager.LoadScene(nextSceneName);
                });
            }
            return;
        }

        // 한 줄 꺼내서 타이핑 시작
        StopAllCoroutines();
        StartCoroutine(TypeLine(lines[lineIndex++]));
    }

    /// <summary>
    /// 한 글자씩 텍스트 표시. 완료 후 클릭 대기 상태로.
    /// </summary>
    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        awaitingClick = false;
        prologueText.text = "";

        foreach (char c in line)
        {
            prologueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        awaitingClick = true;
    }

    void Update()
    {
        // 타이핑 완료 & 클릭 대기 중에만 반응
        if (awaitingClick && !isTyping && Input.GetMouseButtonDown(0))
        {
            awaitingClick = false;
            ShowNextLine();
        }
    }
}
