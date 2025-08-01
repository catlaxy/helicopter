using UnityEngine;

/// <summary>
/// 퓨즈박스 퍼즐 컨트롤러입니다.
/// - 나사 상태를 관리하고,
/// - 뚜껑 애니메이션 후 퍼즐을 시작합니다.
/// </summary>
public class FuseBoxController : MonoBehaviour
{
    [Header("퓨즈박스 외부/내부 오브젝트")]
    [SerializeField] private GameObject outsideGroup;   // FuseBoxOutside
    [SerializeField] private GameObject insideGroup;    // FuseBoxInside

    [Header("나사 설정")]
    [SerializeField] private int totalScrews = 4;

    [Header("뚜껑 애니메이션 (선택)")]
    [SerializeField] private Animator fuseBoxAnimator;

    [Header("연결할 퓨즈박스 퍼즐")]
    [SerializeField] private FuseBoxPuzzle fuseBoxPuzzle;

    // 내부 상태
    private int screwCount = 0;

    /// <summary>
    /// 나사 하나가 풀릴 때마다 호출됩니다.
    /// 모든 나사가 풀리면 뚜껑이 열립니다.
    /// </summary>
    public void OnScrewRemoved()
    {
        screwCount++;
        Debug.Log($"[FuseBoxController] 나사 풀림: {screwCount}/{totalScrews}");

        if (screwCount >= totalScrews)
        {
            Debug.Log("[FuseBoxController] 모든 나사 풀림 → 뚜껑 열기");
            OnLidOpened(); // 또는 애니메이션 사용 시 fuseBoxAnimator.SetTrigger("OpenLid");
        }
    }

    /// <summary>
    /// 뚜껑 애니메이션 종료 후 호출됩니다.
    /// 외부 비활성화, 내부 활성화, 퍼즐 시작을 처리합니다.
    /// </summary>
    public void OnLidOpened()
    {
        // 외부 비활성화
        outsideGroup.SetActive(false);
        // 내부 활성화
        insideGroup.SetActive(true);
        Debug.Log("[FuseBoxController] 뚜껑 열림 → 내부 오픈");

        // 퍼즐 시작
        if (fuseBoxPuzzle != null)
        {
            fuseBoxPuzzle.StartPuzzle();
            Debug.Log("[FuseBoxController] 퓨즈박스 퍼즐 시작됨");
        }
        else
        {
            Debug.LogWarning("[FuseBoxController] 퍼즐이 연결되지 않았습니다.");
        }
    }
}
