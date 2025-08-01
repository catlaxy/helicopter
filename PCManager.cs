using UnityEngine;

/// <summary>
/// PC 잠금 해제 상태와 씬별 기능 사용 토글만 관리하는 컴포넌트입니다.
/// 기능 버튼 대신 외부에서 이 메서드들을 호출하여
/// - 잠금 전 클릭 시 대사 안내
/// - 토글 꺼짐 시 대사 안내
/// - 토글 켜짐 시 실제 기능(예: ScaleUpController 시작 등) 수행
/// </summary>
public class PCManager : MonoBehaviour
{
    private const string PC_UNLOCK_KEY = "PC";

    [Header("🔒 잠금 관리")]
    [Tooltip("Interaction 성공 시 연결된 Locker")]
    public Locker locker;
    [Tooltip("잠긴 상태 시 보여줄 오버레이 UI")]
    public GameObject lockOverlay;

    [Header("⚙️ 씬별 기능 사용 토글")]
    [Tooltip("ScaleUp 기능 사용 여부")]
    public bool useScaleUp = true;
    [Tooltip("Recovery 기능 사용 여부")]
    public bool useRecovery = true;
    [Tooltip("CloudShare 기능 사용 여부")]
    public bool useCloudShare = true;

    [Header("💬 힌트 대사")]
    [Tooltip("잠금 해제 전 클릭 시 출력할 대사 SO")]
    public Dialogue hintUnlockFirstDialogue;
    [Tooltip("기능 비활성 시 출력할 대사 SO")]
    public Dialogue hintFunctionDisabledDialogue;

    private void Awake()
    {
        // Locker.Unlock() 호출 시 OnUnlocked() 실행
        locker.OnUnlocked.AddListener(OnUnlocked);
    }

    private void Start()
    {
        // 이미 언락된 씬이면 바로 언락 처리, 아니면 오버레이 활성화
        if (SaveManager.Instance.IsSceneUnlocked(PC_UNLOCK_KEY))
            OnUnlocked();
        else
            lockOverlay.SetActive(true);
    }

    /// <summary>
    /// PC가 언락되면 오버레이 숨기고 상태 저장
    /// </summary>
    public void OnUnlocked()
    {
        lockOverlay.SetActive(false);
        SaveManager.Instance.UnlockScene(PC_UNLOCK_KEY);
    }

    /// <summary>
    /// Scale Up 버튼(또는 외부 호출) 시 동작.
    /// 잠금 전, 토글 꺼짐, 토글 켜짐 3가지 분기 처리합니다.
    /// </summary>
    public void HandleScaleUpRequest()
    {
        if (lockOverlay.activeSelf)
            ShowHint(hintUnlockFirstDialogue);
        else if (!useScaleUp)
            ShowHint(hintFunctionDisabledDialogue);
        else
        {
            // TODO: 실제 ScaleUpController.StartNextPuzzle() 등 호출
            Debug.Log("PCManager: ScaleUp 실행");
        }
    }

    /// <summary>
    /// Recovery 버튼(또는 외부 호출) 시 동작.
    /// </summary>
    public void HandleRecoveryRequest()
    {
        if (lockOverlay.activeSelf)
            ShowHint(hintUnlockFirstDialogue);
        else if (!useRecovery)
            ShowHint(hintFunctionDisabledDialogue);
        else
        {
            // TODO: 실제 Recovery 기능 호출
            Debug.Log("PCManager: Recovery 실행");
        }
    }

    /// <summary>
    /// Cloud Share 버튼(또는 외부 호출) 시 동작.
    /// </summary>
    public void HandleCloudShareRequest()
    {
        if (lockOverlay.activeSelf)
            ShowHint(hintUnlockFirstDialogue);
        else if (!useCloudShare)
            ShowHint(hintFunctionDisabledDialogue);
        else
        {
            // TODO: 실제 CloudShareWindowController 열기 호출
            Debug.Log("PCManager: CloudShare 실행");
        }
    }

    /// <summary>
    /// DialogueManager를 통해 대사를 출력합니다. 
    /// </summary>
    /// <param name="dialogue">출력할 Dialogue ScriptableObject</param>
    private void ShowHint(Dialogue dialogue)
    {
        if (dialogue != null)
            DialogueManager.Instance.StartDialogue(dialogue);
    }
}
