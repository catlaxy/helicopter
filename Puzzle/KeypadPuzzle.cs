using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;


/// <summary>
/// 비밀번호 입력 시 자리 표시(*)를 지원하는 키패드 퍼즐 스크립트입니다.
/// 틀리면 ERROR 표시 후 자동 초기화됩니다.
/// </summary>
public class KeypadPuzzle : PuzzleBase
{
    [Header("키패드 UI 참조")]
    [Tooltip("키패드 입력 UI를 포함한 Canvas GameObject")]
    [SerializeField] private GameObject keypadCanvas;

    [Tooltip("현재 입력된 숫자를 표시할 TMP_Text UI")]
    [SerializeField] private TMP_Text inputDisplay;

    [Header("숫자 버튼들 (1~9 버튼 순서대로 연결)")]
    [SerializeField] private Button[] numberButtons;

    [Header("퍼즐 설정")]
    [Tooltip("정답으로 사용할 4자리 비밀번호 (예: \"1234\")")]
    [SerializeField] private string correctCode = "";

    [Header("씬 전환 설정")]
    [Tooltip("퍼즐 성공 시 전환할 다음 씬 이름 (비워두면 씬 전환 없음)")]
    [SerializeField] private string nextSceneName;

    [Header("UI 연동 설정")]
    [Tooltip("키패드 퍼즐이 켜질 때 자동으로 비활성화할 오브젝트들")]
    [SerializeField] private GameObject[] objectsToDisableOnStart;


    // 현재 입력한 비밀번호
    private string currentInput = "";

    /// <summary>
    /// 퍼즐 시작 시 호출됩니다.
    /// 버튼 리스너 설정 및 입력 초기화.
    /// </summary>
    public override void StartPuzzle()
    {
        base.StartPuzzle();

        currentInput = "";
        UpdateInputDisplay();

        for (int i = 0; i < numberButtons.Length; i++)
        {
            int digit = i + 1; // 버튼 번호 (1~9)
            numberButtons[i].onClick.RemoveAllListeners();
            numberButtons[i].onClick.AddListener(() => OnButtonPressed(digit));
        }

        keypadCanvas.SetActive(false);
    }

    /// <summary>
    /// 퍼즐 종료 시 호출됩니다.
    /// </summary>
    /// <param name="success">퍼즐 성공 여부</param>
    public override void EndPuzzle(bool success)
    {
        keypadCanvas.SetActive(false);
        // 퍼즐 종료 시 꺼뒀던 오브젝트 다시 켜기
        foreach (var obj in objectsToDisableOnStart)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        base.EndPuzzle(success);

        // 퍼즐 성공 시 → 씬 전환
        if (success && !string.IsNullOrEmpty(nextSceneName))
        {
            StartCoroutine(LoadNextScene());
        }
    }

    /// <summary>
    /// 페이드 아웃 후 지정된 씬으로 이동하는 코루틴입니다.
    /// </summary>
    private IEnumerator LoadNextScene()
    {
        bool isFaded = false;
        FadeManager.Instance.FadeOut(() => isFaded = true);
        yield return new WaitUntil(() => isFaded);

        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// 키패드 오브젝트를 클릭했을 때 UI 표시.
    /// </summary>
    private void OnMouseDown()
    {
        keypadCanvas.SetActive(true);
        // 퍼즐 시작 시 자동으로 꺼질 오브젝트 처리
        foreach (var obj in objectsToDisableOnStart)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    /// <summary>
    /// 숫자 버튼을 눌렀을 때 호출됩니다.
    /// </summary>
    /// <param name="digit">입력된 숫자</param>
    private void OnButtonPressed(int digit)
    {
        Debug.Log($"버튼 눌림: {digit}"); // ✅ 추가된 로그 (숫자 버튼 눌림 확인용)
        if (currentInput.Length >= 4)
            return;

        currentInput += digit.ToString();
        UpdateInputDisplay();

        if (currentInput.Length == 4)
        {
            if (currentInput == correctCode)
            {
                EndPuzzle(true);
            }
            else
            {
                // 오답 처리: 에러 표시 후 초기화
                StartCoroutine(ShowErrorAndReset());
            }
        }
    }

    /// <summary>
    /// TMP_Text에 현재 입력 상태를 표시합니다.
    /// 입력한 숫자 + * 로 표시.
    /// </summary>

    private void UpdateInputDisplay()
    {
        string display = "";

        // 입력된 숫자 부분 (숫자마다 앞뒤 공백 추가)
        for (int i = 0; i < currentInput.Length; i++)
        {
            display += " " + currentInput[i] + " ";
        }

        // 남은 자리수는 공백 포함 별표로 표시
        int remaining = 4 - currentInput.Length;
        string stars = string.Concat(Enumerable.Repeat(" * ", remaining));
        display += stars;

        inputDisplay.text = display;
    }

    /// <summary>
    /// 에러 메시지를 표시 후, 잠시 후 입력 초기화.
    /// </summary>
    private IEnumerator ShowErrorAndReset()
    {
        inputDisplay.text = "ERROR";
        yield return new WaitForSeconds(1.0f); // 1초 후 초기화
        currentInput = "";
        UpdateInputDisplay();
    }
}
