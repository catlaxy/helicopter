// PhoneManager.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 휴대폰 UI 전반을 관리하는 싱글톤 매니저 클래스입니다.
/// 인벤토리 슬롯 1번 클릭으로 UI 열기/닫기,
/// 잠금화면 → 비밀번호 입력 → 메인화면 → 앱 화면 전환 로직을 담당하며,
/// 씬 전환 시에도 파괴되지 않습니다.
/// </summary>
public class PhoneManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static PhoneManager Instance { get; private set; }

    [Header("패널 설정")]
    [Tooltip("잠금화면 패널")]
    public GameObject lockScreenPanel;
    [Tooltip("비밀번호 입력 화면 패널")]
    public GameObject unlockPanel;
    [Tooltip("메인화면 패널")]
    public GameObject mainPanel;
    [Tooltip("갤러리 앱 패널")]
    public GameObject galleryPanel;
    [Tooltip("메시지 앱 패널")]
    public GameObject messagePanel;

    [Header("확대 팝업 관련 UI")]
    [Tooltip("이미지 클릭 시 띄울 검은 반투명 전체화면 패널")]
    public GameObject overlayPanel;
    [Tooltip("팝업 창에 보여줄 Image 컴포넌트 참조")]
    public Image fullscreenImage;
    [Tooltip("팝업 닫기 버튼 참조")]
    public Button closeButton;

    [Header("비밀번호 설정")]
    [Tooltip("사용자 입력 비밀번호 필드")]
    public TMP_InputField passwordField;
    [Tooltip("설정된 비밀번호 (예: \"1234\")")]
    public string correctPassword = "1234";


    [Header("---- 오디오 모드 토글 버튼 ----")]
    [SerializeField] private Button modeToggleButton;   // 모드 전환용 UI 버튼
    [SerializeField] private Image modeToggleImage;    // 버튼의 Image 컴포넌트
    [SerializeField] private Sprite soundModeSprite;    // 소리 모드 아이콘
    [SerializeField] private Sprite vibrateModeSprite;  // 진동 모드 아이콘

    // 현재 모드 상태를 저장 (false = 소리, true = 진동)
    private bool isVibrateMode = false;


    // 잠금 해제 완료 여부 플래그
    private bool isUnlocked = false;
    // 현재 휴대폰 UI가 열려 있는지
    private bool isPhoneOpen = false;

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

    // 시작 시 팝업이 보이지 않도록 비활성화
    overlayPanel.SetActive(false);

        // 닫기 버튼 클릭 시 HideFullscreenImage 호출
        closeButton.onClick.AddListener(HideFullscreenImage);
    }

    void Start()
    {
        // 1) 저장된 모드 로드
        isVibrateMode = SaveManager.Instance.IsVibrateMode();

        // 2) 버튼 클릭 리스너 등록
        if (modeToggleButton != null)
            modeToggleButton.onClick.AddListener(ToggleSoundVibrateMode);
        else
            Debug.LogWarning("PhoneManager: Mode Toggle Button이 할당되지 않았습니다.");

        // 3) 초기 버튼 이미지 설정
        UpdateModeButtonImage();

        // 시작 시 모든 패널 숨김
        CloseAll();
    }

    /// <summary>
    /// 인벤토리 슬롯 1번 클릭 시 호출.
    /// 잠금 해제 전에는 항상 LockScreen, 해제 후에는 항상 MainScreen으로 열립니다.
    /// </summary>
    public void TogglePhoneUI()
    {
        if (isPhoneOpen)
        {
            CloseAll();
            isPhoneOpen = false;
            return;
        }

        // 화면 열기 시: 잠금 해제 여부에 따라
        if (!isUnlocked)
            ShowLockScreen();
        else
            ShowMainScreen();

        isPhoneOpen = true;
    }

    /// <summary>
    /// 잠금화면만 표시
    /// </summary>
    private void ShowLockScreen()
    {
        CloseAll();
        lockScreenPanel.SetActive(true);
    }

    /// <summary>
    /// Unlock 버튼 클릭 시 호출.
    /// 잠금해제(비밀번호) 화면 표시
    /// </summary>
    public void OnUnlockButtonClicked()
    {
        ShowPanel(unlockPanel);
    }

    /// <summary>
    /// Submit 버튼 클릭 시 호출.
    /// 올바른 비밀번호 입력 시 잠금 해제 → MainScreen, 실패 시 LockScreen로.
    /// </summary>
    public void OnSubmitPassword()
    {
        if (passwordField.text == correctPassword)
        {
            isUnlocked = true;      // 한 번 풀리면 해제 상태로 고정
            ShowMainScreen();
        }
        else
        {
            ShowLockScreen();
        }
    }

    /// <summary>
    /// 메인 홈 화면 표시
    /// </summary>
    private void ShowMainScreen()
    {
        CloseAll();
        mainPanel.SetActive(true);
    }

    /// <summary>
    /// 앱 버튼 클릭 시 호출.
    /// 'Gallery' 또는 'Message' 앱 화면으로 전환.
    /// </summary>
    /// <param name="appName">"Gallery" 또는 "Message"</param>
    public void OnAppButtonClicked(string appName)
    {
        switch (appName)
        {
            case "Gallery":
                ShowPanel(galleryPanel);
                break;
            case "Message":
                ShowPanel(messagePanel);
                break;
            default:
                ShowMainScreen();
                break;
        }
    }

    /// <summary>
    /// 모든 패널 비활성화.
    /// </summary>
    private void CloseAll()
    {
        lockScreenPanel.SetActive(false);
        unlockPanel.SetActive(false);
        mainPanel.SetActive(false);
        galleryPanel.SetActive(false);
        messagePanel.SetActive(false);
    }

    /// <summary>
    /// 특정 패널만 활성화.
    /// </summary>
    private void ShowPanel(GameObject panel)
    {
        CloseAll();
        panel.SetActive(true);
    }
    /// <summary>
    /// 갤러리 슬롯 클릭 시 호출할 메서드입니다.
    /// 슬롯의 Sprite를 받아 팝업에 적용하고 화면에 띄웁니다.
    /// </summary>
    /// <param name="sprite">확대할 이미지의 스프라이트</param>
    public void ShowFullscreenImage(Sprite sprite)
    {
        // 팝업 패널 활성화
        overlayPanel.SetActive(true);

        // 전달받은 스프라이트로 교체
        fullscreenImage.sprite = sprite;
    }

    /// <summary>
    /// 팝업 닫기 버튼(OnClick)에서 호출됩니다.
    /// 화면에서 팝업을 숨깁니다.
    /// </summary>
    private void HideFullscreenImage()
    {
        overlayPanel.SetActive(false);
    }


    /// <summary>
    /// 진동 모드와 소리 모드를 토글하고, 변경된 상태를 저장합니다.
    /// </summary>
    private void ToggleSoundVibrateMode()
    {
        // 상태 반전
        isVibrateMode = !isVibrateMode;

        // 버튼 이미지 업데이트
        UpdateModeButtonImage();

        // 변경된 모드 상태를 저장 데이터에 기록
        // ← 기존 SaveVibrateMode 호출 대신 SetVibrateMode 사용
        SaveManager.Instance.SetVibrateMode(isVibrateMode);

        // 디버그 로그
        if (isVibrateMode)
            Debug.Log("PhoneManager: 진동 모드로 전환되었습니다.");
        else
            Debug.Log("PhoneManager: 소리 모드로 전환되었습니다.");
    }



    /// <summary>
    /// 현재 isVibrateMode 값에 따라 버튼 이미지를 교체합니다.
    /// </summary>
    private void UpdateModeButtonImage()
    {
        if (modeToggleImage == null)
        {
            Debug.LogWarning("PhoneManager: Mode Toggle Image가 할당되지 않았습니다.");
            return;
        }
        modeToggleImage.sprite = isVibrateMode ? vibrateModeSprite : soundModeSprite;
    }

}
