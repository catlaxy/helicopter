using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PushLockerPuzzle : MonoBehaviour
{
    [Header("버튼 오브젝트들 (1~8)")]
    public List<Button> buttons;

    [Header("정답 번호 목록 (예: 2,8,1,7)")]
    public List<int> correctCode;

    [Header("배경 전환 인덱스")]
    public int targetBackgroundIndex = 0;

    [Header("성공 시 활성화할 오브젝트들")]
    public List<GameObject> objectsToActivate;

    [Header("성공 시 비활성화할 오브젝트들")]
    public List<GameObject> objectsToDeactivate;

    private HashSet<int> currentInput = new HashSet<int>();
    private Dictionary<Button, int> buttonToNumber = new Dictionary<Button, int>();
    private Dictionary<Button, Image> buttonImages = new Dictionary<Button, Image>();

    private void Start()
    {
        if (buttons.Count != 8)
        {
            Debug.LogError("PushLockerPuzzle: 버튼 개수는 정확히 8개여야 합니다.");
            return;
        }

        for (int i = 0; i < buttons.Count; i++)
        {
            int number = i + 1;
            Button btn = buttons[i];
            buttonToNumber[btn] = number;
            buttonImages[btn] = btn.GetComponent<Image>();
            btn.onClick.AddListener(() => OnButtonPressed(btn));
        }
    }

    private void OnButtonPressed(Button btn)
    {
        int number = buttonToNumber[btn];

        if (currentInput.Contains(number))
        {
            currentInput.Remove(number);
            SetButtonVisual(btn, false);
        }
        else
        {
            if (currentInput.Count >= 4)
                return;

            currentInput.Add(number);
            SetButtonVisual(btn, true);
        }

        if (currentInput.Count == 4)
            CheckAnswer();
    }

    private void CheckAnswer()
    {
        HashSet<int> answer = new HashSet<int>(correctCode);

        if (currentInput.SetEquals(answer))
        {
            Debug.Log("PushLockerPuzzle: 정답 입력됨");

            // 배경 전환
            var bgManager = FindFirstObjectByType<BackgroundManager>();
            if (bgManager != null && targetBackgroundIndex >= 0 && targetBackgroundIndex < bgManager.entries.Count)
            {
                string targetId = bgManager.entries[targetBackgroundIndex].id;
                bgManager.SwitchBackground(targetId);
                Debug.Log($"PushLockerPuzzle: 배경 전환 → {targetId}");
            }

            // 오브젝트 활성화
            foreach (var obj in objectsToActivate)
                if (obj != null) obj.SetActive(true);

            // 오브젝트 비활성화
            foreach (var obj in objectsToDeactivate)
                if (obj != null) obj.SetActive(false);
        }
        else
        {
            Debug.Log("PushLockerPuzzle: 오답. 리셋.");
            ResetButtons();
        }
    }

    private void ResetButtons()
    {
        foreach (var btn in buttons)
            SetButtonVisual(btn, false);

        currentInput.Clear();
    }

    private void SetButtonVisual(Button btn, bool isOn)
    {
        if (buttonImages.TryGetValue(btn, out var img))
        {
            btn.transform.Find("Image_Up").gameObject.SetActive(!isOn);
            btn.transform.Find("Image_Down").gameObject.SetActive(isOn);
        }
    }
}
