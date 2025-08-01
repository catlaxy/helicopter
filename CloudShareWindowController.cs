using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 클라우드 쉐어 윈도우의 게시판 UI를 제어합니다.
/// - 포스트 열기/닫기(비밀번호 입력 포함)
/// - 포스트 닫힐 때 인벤토리 보상 지급
/// - 보상 지급 시 지정된 Dialogue 재생
/// </summary>
public class CloudShareWindowController : MonoBehaviour
{
    [Header("게시판(목록)")]
    [Tooltip("게시글 목록 컨테이너 GameObject (VerticalLayoutGroup 붙여두세요)")]
    public GameObject postsContainer;

    [Tooltip("각 게시글을 나타내는 버튼과 메타데이터를 연결한 리스트")]
    public List<PostEntry> posts;

    [Header("비밀번호 입력 UI")]
    [Tooltip("비밀번호 입력용 패널 (초기 비활성)")]
    public GameObject passwordPanel;
    [Tooltip("비밀번호 입력 필드")]
    public TMP_InputField passwordInput;
    [Tooltip("확인 버튼")]
    public Button passwordConfirmButton;
    [Tooltip("오류 메시지 표시용 텍스트")]
    public TMP_Text errorText;

    [Header("게시글 내용 UI")]
    [Tooltip("게시글 내용을 표시할 패널 (초기 비활성)")]
    public GameObject contentPanel;
    [Tooltip("게시글 이미지를 보여줄 Image 컴포넌트")]
    public Image contentImage;

    [Header("뒤로가기")]
    [Tooltip("뒤로가기 버튼 (모든 패널 상단에 배치)")]
    public Button backButton;

    [Header("인벤토리 연동")]
    [Tooltip("포스트 보상 지급 시 아이템을 추가할 InventoryManager")]
    public InventoryManager inventoryManager;

    [Header("포스트 보상 매핑")]
    [Tooltip("포스트 ID별로 지급할 아이템과 재생할 대사를 설정하세요")]
    public List<PostReward> postRewards;

    // 이미 보상이 지급된 포스트 ID를 저장해 중복 지급 방지
    private HashSet<string> _claimedPosts;

    // 현재 열려 있거나 마지막으로 선택된 포스트 ID
    private string _selectedId;

    /// <summary>
    /// Inspector에서 설정하는, 버튼과 메타데이터를 묶는 클래스입니다.
    /// </summary>
    [System.Serializable]
    public class PostEntry
    {
        [Tooltip("이 게시글의 고유 ID")]
        public string id;
        [Tooltip("목록에서 클릭할 Button 컴포넌트")]
        public Button button;
        [Tooltip("이 게시글의 비밀번호")]
        public string password;
        [Tooltip("비밀번호 통과 시 표시할 이미지")]
        public Sprite contentImage;

        [Header("보상 설정")]
        [Tooltip("게시글 열기 시 지급할 보상 ItemData (선택)")]
        public ItemData rewardItem;
    }


    /// <summary>
    /// 포스트 닫힐 때 지급할 보상(아이템 + 대사)을 정의하는 구조체입니다.
    /// </summary>
    [System.Serializable]
    public struct PostReward
    {
        [Tooltip("보상을 지급할 포스트의 ID")]
        public string postId;
        [Tooltip("해당 포스트 닫힐 때 인벤토리에 추가할 아이템 데이터")]
        public ItemData rewardItem;
        [Tooltip("해당 포스트 닫힐 때 재생할 대사 Dialogue (없으면 비워두세요)")]
        public Dialogue rewardDialogue;
    }

    /// <summary>
    /// 시작 시 초기화: 리스너 등록, UI 초기 상태 설정, 보상 중복 방지 구조체 초기화
    /// </summary>
    private void Start()
    {
        // 보상 중복 체크용 HashSet 초기화
        _claimedPosts = new HashSet<string>();

        // 1) 게시판 목록 활성화
        postsContainer.SetActive(true);

        // 2) 각 포스트 버튼에 클릭 리스너 등록
        foreach (var entry in posts)
        {
            string id = entry.id;  // 클로저 이슈 방지
            entry.button.onClick.AddListener(() => OnPostButtonClicked(id));
        }

        // 3) 비밀번호 확인 버튼 리스너 등록
        passwordConfirmButton.onClick.AddListener(OnPasswordConfirm);

        // 4) 뒤로가기 버튼 리스너 등록
        backButton.onClick.AddListener(OnBackClicked);

        // 5) 초기 UI 상태: 비밀번호·콘텐츠 패널 숨김, 오류 메시지 초기화
        passwordPanel.SetActive(false);
        contentPanel.SetActive(false);
        errorText.text = "";
    }

    /// <summary>
    /// 포스트 버튼 클릭 시 호출.
    /// - 비밀번호 없으면 바로 ShowContent 호출
    /// - 있으면 passwordPanel 활성화
    /// </summary>
    private void OnPostButtonClicked(string id)
    {
        _selectedId = id;
        var entry = posts.Find(p => p.id == id);
        if (entry == null) return;

        if (string.IsNullOrEmpty(entry.password))
        {
            ShowContent(entry);
        }
        else
        {
            passwordInput.text = "";
            errorText.text = "";
            passwordPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 비밀번호 확인 버튼 클릭 시 호출.
    /// - 입력이 맞으면 ShowContent, 틀리면 오류 메시지
    /// </summary>
    private void OnPasswordConfirm()
    {
        var entry = posts.Find(p => p.id == _selectedId);
        if (entry != null && passwordInput.text == entry.password)
        {
            ShowContent(entry);
        }
        else
        {
            StartCoroutine(BlinkErrorText("틀렸쥬?ㅋㅋㅋㅋ"));

        }
    }

    /// <summary>
    /// 비밀번호 오류 시 텍스트를 깜빡이게 한 뒤 자동으로 숨깁니다.
    /// </summary>
    private IEnumerator BlinkErrorText(string message)
    {
        errorText.text = message;
        errorText.gameObject.SetActive(true);

        for (int i = 0; i < 2; i++)
        {
            errorText.enabled = true;
            yield return new WaitForSeconds(0.3f);
            errorText.enabled = false;
            yield return new WaitForSeconds(0.3f);
        }

        errorText.text = "";
        errorText.enabled = true;
        errorText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 콘텐츠 패널에 이미지 표시 후 활성화
    /// </summary>
    private void ShowContent(PostEntry entry)
    {
        // 1) 이미지 표시
        contentImage.sprite = entry.contentImage;
        contentPanel.SetActive(true);

        // 2) 보상 지급 로직
        if (entry.rewardItem != null)
        {
            // 보상 지급 전 스케일업 토글 체크
            // (FindObjectOfType 대신 FindFirstObjectByType 사용)
            var pcMgr = UnityEngine.Object.FindFirstObjectByType<PCManager>();
            if (pcMgr != null && !pcMgr.useScaleUp)
            {
                pcMgr.HandleScaleUpRequest(); // 토글 꺼짐 안내 대사 출력
            }
            else
            {
                // 토글 켜져 있으면 실제 보상 지급
                var invMgr = UnityEngine.Object.FindFirstObjectByType<InventoryManager>();
                invMgr?.AddItem(entry.rewardItem);
                Debug.Log($"CloudShare: 보상 [{entry.rewardItem.itemName}] 지급");
            }

        }
    }


    /// <summary>
    /// 뒤로가기 버튼 클릭 시 호출.
    /// 1) 콘텐츠 열려 있으면: 보상 지급 + 콘텐츠 닫고 목록으로  
    /// 2) 비밀번호 입력 중이면: 비밀번호 패널 닫고 목록으로  
    /// 3) 목록 열려 있으면: 아무 동작 없이 유지 (창 닫지 않음)  
    /// </summary>
    private void OnBackClicked()
    {
        // 1) 콘텐츠 열려 있으면 보상 지급 & 콘텐츠 패널 닫기
        if (contentPanel.activeSelf)
        {
            GivePostReward(_selectedId);
            contentPanel.SetActive(false);
            postsContainer.SetActive(true);
            return;
        }

        // 2) 비밀번호 입력 중이면 → 목록으로
        if (passwordPanel.activeSelf)
        {
            passwordPanel.SetActive(false);
            postsContainer.SetActive(true);
            return;
        }

        // 3) 목록이 이미 열려 있는 경우 → 아무 동작 없이 유지
        // (기존엔 여기서 gameObject.SetActive(false)로 전체 닫음)
        Debug.Log("[CloudShare] 목록이 이미 열린 상태 - 아무 동작 없이 유지");
    }

    /// <summary>
    /// postRewards 매핑에 따라 아이템을 추가하고, 대사가 지정돼 있으면 재생합니다.
    /// 이미 지급된 포스트는 중복 방지됩니다.
    /// </summary>
    private void GivePostReward(string postId)
    {
        // 이미 지급된 포스트면 그냥 종료
        if (_claimedPosts.Contains(postId))
            return;

        // 매핑 리스트에서 해당 postId 검색
        foreach (var pr in postRewards)
        {
            if (pr.postId == postId)
            {
                // 1) 아이템 지급
                if (pr.rewardItem != null)
                {
                    inventoryManager.AddItem(pr.rewardItem);
                    Debug.Log($"[CloudShare] 보상 아이템 지급: {pr.rewardItem.itemId}");
                }
                // 2) 대사 재생
                if (pr.rewardDialogue != null)
                {
                    DialogueManager.Instance.StartDialogue(pr.rewardDialogue);
                    Debug.Log($"[CloudShare] 보상 대사 재생: {pr.rewardDialogue.name}");
                }
                // 지급 완료 표시
                _claimedPosts.Add(postId);
                return;
            }
        }
    }
}
