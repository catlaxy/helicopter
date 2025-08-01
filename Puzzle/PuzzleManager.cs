using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [Header("---- 퍼즐 리스트 ----")]
    [SerializeField] private List<PuzzleBase> puzzles;

    [Header("---- 퍼즐 완료 시 보상 ----")]
    [SerializeField] private Locker locker;
    [SerializeField] private Dialogue completionDialogue;
    [SerializeField] private GameObject[] objectsToActivate;

    // 🆕 퍼즐 완료 시 비활성화할 오브젝트들
    [SerializeField] private GameObject[] objectsToDeactivate;

    private int _currentIndex = 0;
    private PuzzleBase _currentPuzzle;

    private void Start()
    {
        if (puzzles == null || puzzles.Count == 0)
        {
            Debug.LogError("PuzzleManager: 퍼즐 리스트 비어 있음");
            return;
        }

        SetupPuzzle(puzzles[0]);
    }

    private void SetupPuzzle(PuzzleBase puzzle)
    {
        _currentPuzzle = puzzle;
        _currentPuzzle.OnPuzzleFinished += HandlePuzzleFinished;
        _currentPuzzle.StartPuzzle();
    }

    private void HandlePuzzleFinished(bool wasSuccess)
    {
        _currentPuzzle.OnPuzzleFinished -= HandlePuzzleFinished;

        if (wasSuccess)
        {
            Debug.Log("PuzzleManager: 퍼즐 성공");
            _currentIndex++;

            if (_currentIndex < puzzles.Count)
            {
                SetupPuzzle(puzzles[_currentIndex]);
            }
            else
            {
                Debug.Log("PuzzleManager: 모든 퍼즐 완료, 보상 처리");

                if (completionDialogue != null)
                {
                    DialogueManager.Instance.StartDialogue(completionDialogue);
                }

                if (locker != null)
                {
                    locker.Unlock();
                }

                if (objectsToActivate != null)
                {
                    foreach (var obj in objectsToActivate)
                        if (obj != null) obj.SetActive(true);
                }

                if (objectsToDeactivate != null)
                {
                    foreach (var obj in objectsToDeactivate)
                        if (obj != null) obj.SetActive(false);
                }
            }
        }
        else
        {
            Debug.Log("PuzzleManager: 퍼즐 실패");
        }
    }
}
