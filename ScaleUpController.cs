using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// ScaleUpWindow 내에서
/// - 지정한 아이템 ID를 드래그&드롭받아
/// - lastDroppedItemId에 기록한 뒤
/// - 3단계 아이스 슬라이딩 퍼즐을 순차적으로 실행합니다.
/// </summary>
public class ScaleUpController : MonoBehaviour, IDropHandler
{
    [Header("1) 드롭 영역")]
    [Tooltip("인벤토리 아이템을 드롭받을 영역 (InventoryDropArea)")]
    public GameObject dropArea;
    [Tooltip("퍼즐 시작을 허용할 아이템의 ID")]
    public string requiredItemId;
    [Tooltip("퍼즐 미시작 시 안내 텍스트 (DropHintText)")]
    public GameObject dropHintText;
    /*
    [Header("2) 퍼즐 시퀀스")]
    [Tooltip("순서대로 배치된 퍼즐 루트 오브젝트들: Puzzle1, Puzzle2, Puzzle3")]
    public List<GameObject> puzzleRoots;

    // 내부: 각 루트에서 IceSlidingPuzzle 컴포넌트를 구해 담습니다
    private List<IceSlidingPuzzle> _puzzles = new List<IceSlidingPuzzle>();
    private int _currentIndex = -1;  // -1에서 시작해 0부터 퍼즐 실행
    */
    /// <summary>
    /// 마지막으로 드롭된 아이템의 ID
    /// </summary>
    [HideInInspector]
    public string lastDroppedItemId;

    private void Awake()
    {
       /* // 모든 퍼즐 루트 비활성화, IceSlidingPuzzle 참조 수집 & 콜백 연결
        foreach (var root in puzzleRoots)
        {
            var puzzle = root.GetComponent<IceSlidingPuzzle>();
            if (puzzle == null)
            {
                Debug.LogWarning($"[{nameof(ScaleUpController)}] {root.name}에 IceSlidingPuzzle 컴포넌트가 없습니다.");
            }
            else
            {
                puzzle.OnPuzzleComplete.AddListener(OnPuzzleComplete);
                _puzzles.Add(puzzle);
            }
            root.SetActive(false);
        }*/
    }

    /// <summary>
    /// InventoryItem이 DropArea에 놓이면 호출됩니다.
    /// - requiredItemId 검사
    /// - lastDroppedItemId 기록
    /// - 드롭 UI 숨기기
    /// - 첫 퍼즐부터 순차 실행
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag;
        if (dragged == null) return;

        var invItem = dragged.GetComponent<InventoryItem>();
        if (invItem == null) return;

        // 1) 허용된 아이디인지 체크
        if (invItem.data.itemId != requiredItemId)
        {
            Debug.LogWarning($"드롭된 아이템 ID({invItem.data.itemId})가 요구 ID({requiredItemId})와 일치하지 않습니다.");
            return;
        }

        // 2) 아이템 ID 기록
        lastDroppedItemId = invItem.data.itemId;

        // 3) 드롭 힌트/영역 숨기기
        if (dropHintText != null) dropHintText.SetActive(false);
        if (dropArea != null) dropArea.SetActive(false);

        // 4) 인벤토리 아이템 제거
        Destroy(dragged);
        /*
        // 5) 첫 퍼즐부터 시작
        StartNextPuzzle();
        */
    }

    /// <summary>
    /// 다음 퍼즐 루트를 활성화하고 InitializePuzzle 호출
    /// </summary>
    /*private void StartNextPuzzle()
    {
        _currentIndex++;
        if (_currentIndex < _puzzles.Count)
        {
            puzzleRoots[_currentIndex].SetActive(true);
            _puzzles[_currentIndex].InitializePuzzle();
        }
        else
        {
            Debug.Log("[ScaleUpController] 모든 퍼즐 완료!");
            // TODO: 전체 완료 후 처리 (예: 성공 대사, 보상 등)
        }
    }
    
    /// <summary>
    /// 퍼즐이 완료될 때마다 호출됩니다.
    /// 현재 루트 비활성화 후 다음 퍼즐 시작
    /// </summary>
    private void OnPuzzleComplete()
    {
        if (_currentIndex >= 0 && _currentIndex < puzzleRoots.Count)
            puzzleRoots[_currentIndex].SetActive(false);

        StartNextPuzzle();
    }*/
}
