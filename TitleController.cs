using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// 타이틀 씬에서 페이드 및 메뉴 출력, 버튼 동작을 제어하는 컨트롤러입니다.
/// </summary>
public class TitleController : MonoBehaviour
{
    [Header("페이드 설정")]
    public CanvasGroup menuGroup;             // 메뉴 버튼 전체를 묶는 그룹
    public float menuFadeDelay = 1f;          // 페이드인 시작까지의 지연 시간
    public float menuFadeDuration = 1f;       // 페이드인 지속 시간

    [Header("버튼 참조")]
    public Button startButton;
    public Button loadButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("설정창 관련")]
    public GameObject settingsPanel;
    public Slider volumeSlider;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    [Header("다음 씬 이름")]
    public string nextSceneName = "GameScene";

    private void Start()
    {
        // 메뉴 비활성화 상태로 시작
        menuGroup.alpha = 0f;
        menuGroup.interactable = false;
        menuGroup.blocksRaycasts = false;

        settingsPanel.SetActive(false);

        // 페이드 매니저로 화면 페이드 인 → 메뉴 출력
        FadeManager.Instance.FadeIn(() =>
        {
            StartCoroutine(FadeInMenu());
        });

        // 버튼 이벤트 연결
        startButton.onClick.AddListener(OnStartClicked);
        loadButton.onClick.AddListener(OnLoadClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

        InitializeResolutionOptions();
    }

    private IEnumerator FadeInMenu()
    {
        yield return new WaitForSeconds(menuFadeDelay);

        float elapsed = 0f;
        while (elapsed < menuFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / menuFadeDuration);
            menuGroup.alpha = t;
            yield return null;
        }

        menuGroup.alpha = 1f;
        menuGroup.interactable = true;
        menuGroup.blocksRaycasts = true;
    }

    public void OnStartClicked()
    {
        menuGroup.interactable = false;
        FadeManager.Instance.FadeOut(() =>
        {
            SceneManager.LoadScene(nextSceneName);
        });
    }

    public void OnLoadClicked()
    {
        Debug.Log("불러오기 버튼 클릭됨 (미구현)");
    }

    public void OnSettingsClicked()
    {
        settingsPanel.SetActive(true);
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }

    public void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
    }

    public void OnFullscreenChanged(bool isFull)
    {
        Screen.fullScreen = isFull;
    }

    public void OnResolutionChanged(int index)
    {
        Resolution selected = resolutions[index];
        Screen.SetResolution(selected.width, selected.height, Screen.fullScreen);
    }

    /// <summary>
    /// 설정창 닫기 버튼 클릭 시 호출됨.
    /// </summary>
    public void OnCloseSettingsClicked()
    {
        settingsPanel.SetActive(false);
    }


    private Resolution[] resolutions;

    private void InitializeResolutionOptions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string label = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(label);
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
    }
}
